"""A sailing ship from a dict of numbers, as objects: the hull with its bowsprit, fittings and
people; its flag (its own material, which waves it); and for each mast i, `<name>Mast<i>` (the mast,
its tops and standing rigging), its two sail states, `<name>Mast<i>Sailing` (trimmed to one wind,
every sail set, the booms swung out only as far as the lee rigging lets them) and
`<name>Mast<i>Anchored` (spars squared, sails furled), and `<name>Mast<i>Stump`, what is left when
it breaks. A game shows the hull, the flag, each mast with one state, and a broken mast's stump.
The build reports any rope that still passes through a sail.

A ship type is a builder module whose PARAMS fill this dict and whose build is this one's.
Metres, Z up, origin on the design waterline amidships. The parts are laid out bow toward +Y and
each object turned half round, so the bow faces -Y: the FBX export makes that Unity's +Z.

Hull keys: length (on deck), beam, draft, freeboard (amidships, to the rail), sheer_bow and
sheer_stern (rise of the rail toward each end), sheer_low (where the rail is lowest, as a fraction
from the stern), widest (where the hull is widest, same), transom (the transom's width over the
beam), transom_rake_deg, bulwark (height above the deck), band (the trim band's depth below the rail),
boot (how high the bottom paint shows above the water).

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

    body = MeshParts()
    hull.hull(body, params)
    hull.rudder(body, params)
    fittings.fit(body, params)
    props.place(body, params)
    rig.bowsprit_pole(body, params)
    standing = [MeshParts() for _ in masts]
    rig.standing(standing, masts)
    rigging.standing(body, standing, params, masts)
    make_object(name, body, preview)
    standing_ropes = body.ropes + [rope for parts in standing for rope in parts.ropes]

    trim = sails.Trim.for_wind(sails.WIND_FROM_DEG)
    flag = MeshParts()
    props.flag(flag, params, max(masts, key=lambda m: m.head.z).head, trim.downwind)
    make_object(f"{name}Flag", flag, preview)

    gaff_sails = {sails.sail_name(i, spec) for i, spec in enumerate(params["sails"]) if spec["kind"] == "gaff"}
    sailing = _sailing(params, masts, trim)
    while trim.boom_deg and any(c.cloth in gaff_sails for c in _crossings(standing_ropes, sailing)):
        trim = trim.boom_in()
        sailing = _sailing(params, masts, trim)

    anchored = [MeshParts() for _ in masts]
    sails.anchored(anchored, params, masts)
    rigging.running(anchored, params, masts, sails.Trim.at_anchor(), sails_set=False)

    for i, mast in enumerate(masts):
        make_object(f"{name}Mast{i}", standing[i], preview)
        make_object(f"{name}Mast{i}Sailing", sailing[i], preview)
        make_object(f"{name}Mast{i}Anchored", anchored[i], preview)
        stump = MeshParts()
        rig.stump(stump, mast)
        make_object(f"{name}Mast{i}Stump", stump, preview)

    for state, per_mast in (("Sailing", sailing), ("Anchored", anchored)):
        clearance.report(f"{name}{state}", _crossings(standing_ropes, per_mast))
