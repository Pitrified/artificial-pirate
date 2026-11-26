"""A giant three-master with the look of the Amerigo Vespucci (the Italian navy's 1931 training ship);
see builders/ship.py.

Hull from its published particulars (Wikipedia: 82.4 m between perpendiculars, 100.5 m overall,
15.56 m beam, about 7 m draft, masts 50, 54 and 43 m), the masts' places, the bands and the ports
measured on references/giant/vespucci_broadside.jpg; the sheer, the overhanging bow and counter, the bands
rising with the sheer, the red bottom and the gilt line from the profile drawing references/giant/vespucci_profile.png. A long black hull with two white bands of ports,
both rows armed on covered decks. The rig is the brig's (builders/brig.py)
scaled above each mast's deck to its height (the lengths too), four yards a mast where she carries six;
the yards 1.4 times the brig's scaled lengths, as wide as hers against the beam, the main course set.
"""

from __future__ import annotations

from builders.ship import build, report  # noqa: F401 - build.py calls the type's build and report

PARAMS = {
    "name": "Giant",
    "length": 82.4,          # m, on deck
    "beam": 15.5,            # m
    "draft": 7.0,            # m
    "freeboard": 10.0,       # m, amidships
    "sheer_bow": 3.2,        # m
    "sheer_stern": 1.8,      # m
    "sheer_low": 0.45,
    "entry": 1.5,
    "bow_overhang": 5.0,     # m, the stem's head forward of its foot
    "stern_overhang": 4.0,   # m, the counter's
    "widest": 0.5,
    "transom": 0.55,
    "transom_water": 0.12,
    "run": 3.0,
    "transom_rake_deg": 12.0,
    "bulwark": 1.5,          # m
    "open_rail": [0.04, 0.87],
    "band": 1.55,            # m from the rail: a white line under the open rail's sill
    "boot": 0.6,             # m
    "tumblehome": 0.08,      # of the half beam, the rail in from the widest
    "stations": 40,
    "raised_decks": [
        {"from": 0.0, "to": 0.14, "height": 1.3},   # the poop
        {"from": 0.88, "to": 1.0, "height": 1.2},   # the forecastle
    ],
    "masts": [
        {
            "at": 0.79, "height": 50.0, "diameter": 1.3, "rake_deg": 1.0,
            "top_z": 24.1, "crosstrees_z": 37.4, "shrouds": 4,
            "yards": [
                {"z": 22.8, "length": 24.8}, {"z": 27.6, "length": 23.4}, {"z": 33.6, "length": 21.6},
                {"z": 39.4, "length": 17.2}, {"z": 44.3, "length": 14.6}, {"z": 48.7, "length": 11.5},
            ],
        },
        {
            "at": 0.43, "height": 54.0, "diameter": 1.4, "rake_deg": 1.0,
            "top_z": 28.9, "crosstrees_z": 41.0, "shrouds": 4,
            "yards": [
                {"z": 27.0, "length": 26.0}, {"z": 31.6, "length": 24.4}, {"z": 37.2, "length": 22.6},
                {"z": 43.0, "length": 17.8}, {"z": 47.6, "length": 15.2}, {"z": 52.0, "length": 12.0},
            ],
        },
        {
            "at": 0.18, "height": 43.0, "diameter": 1.1, "rake_deg": 2.0,
            "top_z": 21.7, "crosstrees_z": 32.7, "shrouds": 4,
            "yards": [
                {"z": 20.7, "length": 19.5}, {"z": 24.6, "length": 18.3}, {"z": 29.8, "length": 16.6},
                {"z": 35.0, "length": 12.2}, {"z": 40.2, "length": 9.1},
            ],
            "boom": {"z": 13.2, "length": 15.0, "rise_deg": 4.0},
            "gaff": {"z": 23.7, "length": 10.9, "peak_deg": 25.0},
        },
    ],
    "lifts_under_sails": False,
    "bowsprit": {"length": 20.0, "steeve_deg": 16.0, "diameter": 1.2},
    "sails": [
        {"kind": "square", "mast": 0, "top": 0, "bottom": {"z": 14.0, "length": 23.0}},
        {"kind": "square", "mast": 0, "top": 1, "bottom": 0},
        {"kind": "square", "mast": 0, "top": 2, "bottom": 1},
        {"kind": "square", "mast": 0, "top": 3, "bottom": 2},
        {"kind": "square", "mast": 0, "top": 4, "bottom": 3},
        {"kind": "square", "mast": 0, "top": 5, "bottom": 4},
        {"kind": "square", "mast": 1, "top": 0, "bottom": {"z": 15.0, "length": 25.0}},
        {"kind": "square", "mast": 1, "top": 1, "bottom": 0},
        {"kind": "square", "mast": 1, "top": 2, "bottom": 1},
        {"kind": "square", "mast": 1, "top": 3, "bottom": 2},
        {"kind": "square", "mast": 1, "top": 4, "bottom": 3},
        {"kind": "square", "mast": 1, "top": 5, "bottom": 4},
        {"kind": "square", "mast": 2, "top": 1, "bottom": 0},
        {"kind": "square", "mast": 2, "top": 2, "bottom": 1},
        {"kind": "square", "mast": 2, "top": 3, "bottom": 2},
        {"kind": "square", "mast": 2, "top": 4, "bottom": 3},
        {"kind": "gaff", "mast": 2},
        {"kind": "jib", "mast": 0, "head_z": 37.0, "tack": 0.0, "clew_aft": 8.8, "clew_z": 14.0},
        {"kind": "jib", "mast": 0, "head_z": 42.0, "tack": 0.3, "clew_aft": 13.0, "clew_z": 15.0},
        {"kind": "jib", "mast": 0, "head_z": 46.3, "tack": 0.6, "clew_aft": 17.7, "clew_z": 16.5},
        {"kind": "staysail", "mast": 1, "tack": 0.85, "head": 0.3, "clew_ahead": 9.0, "clew_z": 13.0},
        {"kind": "staysail", "mast": 2, "tack": 0.85, "head": 0.3, "clew_ahead": 6.0, "clew_z": 12.0},
        {"kind": "jib", "mast": 0, "head_z": 50.0, "tack": 1.0, "clew_aft": 21.5, "clew_z": 20.3},
    ],
    "stripes": [[3.8, 5.7], [6.6, 8.2], [8.25, 8.4, "Gilt"]],
    "bands_follow_sheer": True,
    "stripe_scrolls": True,
    "stern_gallery": True,
    "anchors": True,
    "manned_yards": 2,
    "gun_ports": [
        {"count": 16, "from": 0.16, "to": 0.84, "z": 4.75, "size": 0.9},
        {"count": 15, "from": 0.18, "to": 0.82, "z": 7.4, "size": 0.9},
    ],
    "chase_ports": {"across": [-1.2, 1.2], "z": 8.6, "size": 0.8},
    "stern_ports": {"across": [-0.9, 0.9], "z": 7.4, "size": 0.8},
    "stern_windows": 3,
    "deck": {"hatches": [[0.62, 5.0, 4.0], [0.33, 3.6, 3.0], [0.84, 2.6, 2.6], [0.52, 3.0, 3.0]], "companionway": 1, "capstan": 0.55,
             "helm": "wheel", "helm_at": 0.1},
    "deckhouses": [[0.27, 4.0, 3.2, 2.2], [0.71, 3.6, 3.0, 2.2]],
    "boats": [[0.31, 7.5], [0.57, 7.5]],
    "crew": [[0.04, 0.5], [0.04, -0.5], [0.3, 0.4], [0.38, -0.5], [0.58, 0.3], [0.7, -0.6], [0.8, 0.45], [0.92, 0.0], [0.5, -0.4],
             [0.2, 0.8], [0.16, -0.75], [0.22, -0.7], [0.24, 0.75], [0.35, 0.75], [0.4, -0.8], [0.47, 0.8],
             [0.55, -0.8], [0.6, 0.8], [0.66, -0.75], [0.74, 0.75], [0.77, -0.7], [0.86, -0.6], [0.89, 0.6]],
    "preview_rgb": {
        "HullBottom": [0.45, 0.06, 0.05],
        "Topsides": [0.06, 0.06, 0.06],
        "Trim": [0.90, 0.90, 0.88],
        "ShipWood": [0.42, 0.30, 0.18],
        "Spar": [0.50, 0.37, 0.22],
        "LanternGlass": [1.00, 0.80, 0.45],
        "SailCloth": [0.86, 0.83, 0.75],
        "FurledCloth": [0.72, 0.69, 0.62],
        "Tar": [0.24, 0.16, 0.09],
        "Hemp": [0.55, 0.45, 0.30],
        "Deck": [0.55, 0.45, 0.33],
        "Gilt": [0.75, 0.58, 0.22],
        "Palette": [0.55, 0.40, 0.30],
        "Flag": [0.10, 0.10, 0.10],
    },
}
