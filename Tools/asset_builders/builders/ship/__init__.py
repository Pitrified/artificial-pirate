"""A sailing ship from a dict of numbers, as objects: the hull with its bowsprit, fittings and
people; its flag (its own material, which waves it), streaming aft from its origin at the masthead,
which the game turns to the wind; and for each mast i, `<name>Mast<i>` (the mast, its tops and
standing rigging), its sail states, `<name>Mast<i>Sailing<b>` for each of the wind's bands b
(sails.WIND_BANDS_DEG: every sail set, the booms swung out only as far as the lee rigging lets them)
and `<name>Mast<i>Anchored` (spars squared, sails furled), and `<name>Mast<i>Stump`, what is left
when it breaks; and each gun's barrel, `<name>GunStarboard<i>` and `<name>GunPort<i>` (stern to
bow) and `<name>GunBow<i>`, `<name>GunStern<i>` where it has them, its origin at its trunnions so it can be aimed. A game shows the hull, the flag, each mast with
one state, a broken mast's stump, and the barrels.
The build reports any rope that still passes through a sail.

A ship type is a builder module whose PARAMS fill this dict and whose build is this one's.
Metres, Z up, origin on the design waterline amidships. The parts are laid out bow toward +Y and
each object turned half round, so the bow faces -Y: the FBX export makes that Unity's +Z.

Hull keys: length (on deck), beam, draft, freeboard (amidships, to the rail), sheer_bow and
sheer_stern (rise of the rail toward each end), sheer_low (where the rail is lowest, as a fraction
from the stern), widest (where the hull is widest, same), transom (the transom's width over the
beam), transom_rake_deg, bulwark (height above the deck), band (the trim band's depth below the rail),
boot (how high the bottom paint shows above the water); tumblehome, optional (how far the side falls in
toward the rail, of the half beam), stations, optional (how many slices the hull is lofted in); raised_decks, optional (each from and to, fractions
from the stern, and height over the deck: a quarterdeck or poop aft, a forecastle forward).

masts: each has at (fraction from the stern), height (above the water), diameter, rake_deg, top_z
(the lower masthead's platform) and crosstrees_z (the topmast's head), shrouds (a side), yards
(each z and length), and optionally a boom (z, length, rise_deg) and a gaff (z, length, peak_deg).
bowsprit: length beyond the stem, steeve_deg, diameter.
sails: a square sail hangs from a yard (top) to the yard below it (bottom) or to a foot {z, length};
a gaff sail fills a mast's gaff and boom; a jib runs from head_z on a mast to a point along the bowsprit
(tack, 0 to 1), its clew clew_aft metres behind the tack at clew_z.
preview_rgb: probe colours by slot name. Optional fittings (wales, gun ports, stern windows, deck
furniture) are described in builders/ship/fittings.py.
"""

from __future__ import annotations

from builders.ship import clearance, fittings, hull, props, rig, rigging, sails
from mathutils import Vector

from builders.ship.mesh import MeshParts, make_object


def _sailing(params: dict, masts: list[rig.Mast], trim: sails.Trim) -> list[MeshParts]:
    per_mast = [MeshParts() for _ in masts]
    sails.sailing(per_mast, params, masts, trim)
    rigging.running(per_mast, params, masts, trim, sails_set=True)
    return per_mast


def _crossings(ropes: list, state: list[MeshParts]) -> list[clearance.Crossing]:
    """Every rope through a sail of the state, its own ropes with the standing ones."""
    every = ropes + [rope for parts in state for rope in parts.ropes]
    return [c for parts in state for c in clearance.crossings(every, parts)]


def build(params: dict) -> None:
    """Make the ship's objects."""
    name = params["name"]
    preview = params["preview_rgb"]
    masts = rig.masts(params)

    def made(object_name: str, parts: MeshParts, origin: Vector | None = None) -> None:
        moved = hull.overhang(params, origin) if origin is not None else None
        make_object(object_name, parts, preview, origin=moved, warp=lambda v: hull.overhang(params, v))

    body = MeshParts()
    hull.hull(body, params)
    hull.rudder(body, params)
    fittings.fit(body, params)
    props.place(body, params)
    rig.bowsprit_pole(body, params)
    standing = [MeshParts() for _ in masts]
    rig.standing(standing, masts)
    rigging.standing(body, standing, params, masts)
    made(name, body)
    for gun, barrel, trunnions in props.barrels(params):
        made(f"{name}{gun}", barrel, origin=trunnions)
    standing_ropes = body.ropes + [rope for parts in standing for rope in parts.ropes]

    tallest = max(range(len(masts)), key=lambda i: masts[i].head.z)
    head = masts[tallest].head
    props.flagstaff(standing[tallest], params, head)
    flag = MeshParts()
    props.flag(flag, params, head, Vector((0.0, -1.0, 0.0)))
    made(f"{name}Flag", flag, origin=head)

    kind_of = {sails.sail_name(i, spec): spec["kind"] for i, spec in enumerate(params["sails"])}
    bands = []
    for wind in sails.WIND_BANDS_DEG:
        trim = sails.Trim.for_wind(wind)
        sailing = _sailing(params, masts, trim)
        while crossed := {kind_of[c.cloth] for c in _crossings(standing_ropes, sailing)}:
            eased = trim.eased(crossed)
            if eased == trim:
                break
            trim = eased
            sailing = _sailing(params, masts, trim)
        bands.append(sailing)

    anchored = [MeshParts() for _ in masts]
    sails.anchored(anchored, params, masts)
    rigging.running(anchored, params, masts, sails.Trim.at_anchor(), sails_set=False)

    for i, mast in enumerate(masts):
        made(f"{name}Mast{i}", standing[i])
        for b, sailing in enumerate(bands):
            made(f"{name}Mast{i}Sailing{b}", sailing[i])
        made(f"{name}Mast{i}Anchored", anchored[i])
        stump = MeshParts()
        rig.stump(stump, mast)
        made(f"{name}Mast{i}Stump", stump)

    for b, per_mast in enumerate(bands):
        clearance.report(f"{name}Sailing{b}", _crossings(standing_ropes, per_mast))
    clearance.report(f"{name}Anchored", _crossings(standing_ropes, anchored))


def report(params: dict) -> dict:
    """For the ship's type, in Unity's axes (X starboard, Y up, Z to the bow), metres from the waterline
    amidships: the starboard guns' muzzles (the port guns mirror them), the bow chasers' and stern guns',
    the helmsman's eyes, and the wind's
    bands the sailing states are set for."""
    def unity(points: list) -> list:
        return [[round(m.x, 4), round(m.z, 4), round(m.y, 4)] for m in (hull.overhang(params, v) for v in points)]

    eye = hull.overhang(params, fittings.helm_eye(params))
    return {"guns": unity(props.battery_muzzles(params, "Starboard")),
            "bow_guns": unity(props.battery_muzzles(params, "Bow")),
            "stern_guns": unity(props.battery_muzzles(params, "Stern")),
            "helm_eye": [round(eye.x, 3), round(eye.z, 3), round(eye.y, 3)],
            "wind_bands_deg": list(sails.WIND_BANDS_DEG)}
