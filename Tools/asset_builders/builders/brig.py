"""A brig after the Niagara (1988 rebuild of the 1813 brig); see builders/ship.py.

Hull from its published particulars (Wikipedia), the rig measured on
references/brig/niagara_sail.jpg. The draft is not published; 3.4 m is assumed.
"""

from __future__ import annotations

from builders.ship import build  # noqa: F401 - build.py calls the type's build

PARAMS = {
    "name": "Brig",
    "length": 33.7,          # m, on deck
    "beam": 9.8,             # m
    "draft": 3.4,            # m, assumed
    "freeboard": 3.0,        # m, amidships
    "sheer_bow": 0.8,        # m
    "sheer_stern": 0.3,      # m
    "sheer_low": 0.5,
    "widest": 0.45,
    "transom": 0.65,
    "transom_rake_deg": 8.0,
    "bulwark": 1.2,          # m
    "band": 1.4,             # m, the gun deck's band
    "boot": 0.4,             # m
    "masts": [
        {
            "at": 0.75, "height": 34.5, "diameter": 0.8, "rake_deg": 4.0,
            "top_z": 14.0, "crosstrees_z": 24.5, "shrouds": 4,
            "yards": [
                {"z": 13.0, "length": 14.0}, {"z": 23.5, "length": 12.5},
                {"z": 30.0, "length": 8.5}, {"z": 33.5, "length": 6.5},
            ],
        },
        {
            "at": 0.40, "height": 36.0, "diameter": 0.8, "rake_deg": 6.0,
            "top_z": 14.5, "crosstrees_z": 25.5, "shrouds": 4,
            "yards": [
                {"z": 13.5, "length": 14.0}, {"z": 24.5, "length": 12.5},
                {"z": 31.0, "length": 8.5}, {"z": 34.5, "length": 6.5},
            ],
            "boom": {"z": 6.0, "length": 15.0, "rise_deg": 4.0},
            "gaff": {"z": 16.5, "length": 11.0, "peak_deg": 25.0},
        },
    ],
    "bowsprit": {"length": 17.0, "steeve_deg": 16.0, "diameter": 0.6},
    "sails": [
        {"kind": "square", "mast": 0, "top": 0, "bottom": {"z": 6.0, "length": 13.0}},
        {"kind": "square", "mast": 0, "top": 1, "bottom": 0},
        {"kind": "square", "mast": 0, "top": 2, "bottom": 1},
        {"kind": "square", "mast": 0, "top": 3, "bottom": 2},
        {"kind": "square", "mast": 1, "top": 1, "bottom": 0},
        {"kind": "square", "mast": 1, "top": 2, "bottom": 1},
        {"kind": "square", "mast": 1, "top": 3, "bottom": 2},
        {"kind": "gaff", "mast": 1},
        {"kind": "jib", "mast": 0, "head_z": 27.2, "tack": 0.0, "clew_aft": 7.0, "clew_z": 6.0},
        {"kind": "jib", "mast": 0, "head_z": 31.6, "tack": 0.6, "clew_aft": 14.0, "clew_z": 8.0},
        {"kind": "jib", "mast": 0, "head_z": 34.5, "tack": 1.0, "clew_aft": 17.0, "clew_z": 11.0},
    ],
    "wales": [1.2, 2.9],
    "gun_ports": {"count": 9, "from": 0.15, "to": 0.82, "z": 2.25, "size": 0.65},
    "stern_windows": 5,
    "deck": {"hatches": [[0.58, 3.0, 2.4], [0.3, 2.0, 1.8], [0.88, 1.6, 1.6]], "capstan": 0.22,
             "helm": "wheel", "helm_at": 0.09},
    "crew": [[0.1, 0.3], [0.09, -0.3], [0.45, 0.4], [0.5, -0.5], [0.66, 0.0], [0.88, 0.3]],
    "preview_rgb": {
        "HullBottom": [0.05, 0.05, 0.05],
        "Topsides": [0.06, 0.06, 0.06],
        "Trim": [0.72, 0.55, 0.25],
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
