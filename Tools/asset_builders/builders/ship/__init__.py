"""A sailing ship from a dict of numbers, as objects: the hull with its standing rig, fittings and
people; its flag (its own material, which waves it); and two sail states over it, `<name>Sailing`
(trimmed to one wind, every sail set) and `<name>Anchored` (spars squared, sails furled). A game
shows the hull, the flag and one state.

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

from builders.ship import fittings, hull, props, rig, rigging, sails
from builders.ship.mesh import MeshParts, make_object


def build(params: dict) -> None:
    """Make the ship's three objects."""
    name = params["name"]
    preview = params["preview_rgb"]
    masts = rig.masts(params)

    body = MeshParts()
    hull.hull(body, params)
    hull.rudder(body, params)
    fittings.fit(body, params)
    props.place(body, params)
    rig.standing(body, params, masts)
    rigging.standing(body, params, masts)
    make_object(name, body, preview)

    trim = sails.Trim.for_wind(sails.WIND_FROM_DEG)
    flag = MeshParts()
    props.flag(flag, params, max(masts, key=lambda m: m.head.z).head, trim.downwind)
    make_object(f"{name}Flag", flag, preview)

    sailing = MeshParts()
    sails.sailing(sailing, params, masts, trim)
    rigging.running(sailing, params, masts, trim, sails_set=True)
    make_object(f"{name}Sailing", sailing, preview)

    anchored = MeshParts()
    sails.anchored(anchored, params, masts)
    rigging.running(anchored, params, masts, sails.Trim.at_anchor(), sails_set=False)
    make_object(f"{name}Anchored", anchored, preview)
