"""A giant three-master with the look of the Amerigo Vespucci (the Italian navy's 1931 training ship);
see builders/ship.py.

Hull from its published particulars (Wikipedia: 82.4 m between perpendiculars, 100.5 m overall,
15.56 m beam, about 7 m draft, masts 50, 54 and 43 m), the masts' places, the bands and the ports
measured on references/giant/vespucci_broadside.jpg. A long black hull with two white bands of ports:
the upper row armed on a covered deck, the lower row painted. The rig is the brig's (builders/brig.py)
scaled above each mast's deck to its height (the lengths too), four yards a mast where she carries six;
the main course is left off (clewed up), since on a hull this broad it meets the lee shrouds at every
brace that keeps it full.
"""

from __future__ import annotations

from builders.ship import build, report  # noqa: F401 - build.py calls the type's build and report

PARAMS = {
    "name": "Giant",
    "length": 82.4,          # m, on deck
    "beam": 15.5,            # m
    "draft": 7.0,            # m
    "freeboard": 10.0,       # m, amidships
    "sheer_bow": 1.6,        # m
    "sheer_stern": 1.0,      # m
    "sheer_low": 0.5,
    "widest": 0.5,
    "transom": 0.6,
    "transom_rake_deg": 12.0,
    "bulwark": 1.5,          # m
    "band": 0.4,             # m, a white line under the rail
    "boot": 0.6,             # m
    "raised_decks": [
        {"from": 0.0, "to": 0.14, "height": 1.3},   # the poop
        {"from": 0.88, "to": 1.0, "height": 1.2},   # the forecastle
    ],
    "masts": [
        {
            "at": 0.79, "height": 50.0, "diameter": 1.3, "rake_deg": 1.0,
            "top_z": 24.1, "crosstrees_z": 37.4, "shrouds": 4,
            "yards": [
                {"z": 22.8, "length": 17.7}, {"z": 36.1, "length": 15.8},
                {"z": 44.3, "length": 10.7}, {"z": 48.7, "length": 8.2},
            ],
        },
        {
            "at": 0.43, "height": 54.0, "diameter": 1.4, "rake_deg": 1.0,
            "top_z": 29.5, "crosstrees_z": 41.0, "shrouds": 4,
            "yards": [
                {"z": 28.0, "length": 18.6}, {"z": 39.7, "length": 16.6},
                {"z": 47.3, "length": 11.3}, {"z": 52.0, "length": 8.6},
            ],
        },
        {
            "at": 0.18, "height": 43.0, "diameter": 1.1, "rake_deg": 2.0,
            "top_z": 21.7, "crosstrees_z": 32.7, "shrouds": 4,
            "yards": [
                {"z": 20.7, "length": 13.9}, {"z": 31.7, "length": 12.4},
                {"z": 38.2, "length": 8.4}, {"z": 41.7, "length": 6.5},
            ],
            "boom": {"z": 13.2, "length": 15.0, "rise_deg": 4.0},
            "gaff": {"z": 23.7, "length": 10.9, "peak_deg": 25.0},
        },
    ],
    "bowsprit": {"length": 20.0, "steeve_deg": 16.0, "diameter": 1.2},
    "sails": [
        {"kind": "square", "mast": 0, "top": 0, "bottom": {"z": 14.0, "length": 16.4}},
        {"kind": "square", "mast": 0, "top": 1, "bottom": 0},
        {"kind": "square", "mast": 0, "top": 2, "bottom": 1},
        {"kind": "square", "mast": 0, "top": 3, "bottom": 2},
        {"kind": "square", "mast": 1, "top": 1, "bottom": 0},
        {"kind": "square", "mast": 1, "top": 2, "bottom": 1},
        {"kind": "square", "mast": 1, "top": 3, "bottom": 2},
        {"kind": "square", "mast": 2, "top": 1, "bottom": 0},
        {"kind": "square", "mast": 2, "top": 2, "bottom": 1},
        {"kind": "square", "mast": 2, "top": 3, "bottom": 2},
        {"kind": "gaff", "mast": 2},
        {"kind": "jib", "mast": 0, "head_z": 39.5, "tack": 0.0, "clew_aft": 8.8, "clew_z": 14.0},
        {"kind": "jib", "mast": 0, "head_z": 46.3, "tack": 0.6, "clew_aft": 17.7, "clew_z": 16.5},
        {"kind": "jib", "mast": 0, "head_z": 50.0, "tack": 1.0, "clew_aft": 21.5, "clew_z": 20.3},
    ],
    "stripes": [[3.8, 5.7], [6.6, 8.2]],
    "gun_ports": [
        {"count": 16, "from": 0.16, "to": 0.84, "z": 4.75, "size": 0.9, "guns": False},
        {"count": 15, "from": 0.18, "to": 0.82, "z": 7.4, "size": 0.9},
    ],
    "chase_ports": {"across": [-1.2, 1.2], "z": 8.6, "size": 0.8},
    "stern_ports": {"across": [-2.0, 2.0], "z": 7.4, "size": 0.8},
    "stern_windows": 9,
    "deck": {"hatches": [[0.62, 5.0, 4.0], [0.33, 3.6, 3.0], [0.84, 2.6, 2.6], [0.52, 3.0, 3.0]], "companionway": 1, "capstan": 0.55,
             "helm": "wheel", "helm_at": 0.1},
    "crew": [[0.04, 0.5], [0.04, -0.5], [0.3, 0.4], [0.38, -0.5], [0.58, 0.3], [0.7, -0.2], [0.8, 0.45], [0.92, 0.0], [0.5, -0.4]],
    "preview_rgb": {
        "HullBottom": [0.05, 0.05, 0.05],
        "Topsides": [0.06, 0.06, 0.06],
        "Trim": [0.90, 0.90, 0.88],
        "ShipWood": [0.42, 0.30, 0.18],
        "Spar": [0.50, 0.37, 0.22],
        "LanternGlass": [1.00, 0.80, 0.45],
        "SailCloth": [0.86, 0.83, 0.75],
        "FurledCloth": [0.72, 0.69, 0.62],
        "Tar": [0.08, 0.07, 0.06],
        "Deck": [0.55, 0.45, 0.33],
        "Gilt": [0.75, 0.58, 0.22],
        "Palette": [0.55, 0.40, 0.30],
        "Flag": [0.10, 0.10, 0.10],
    },
}
