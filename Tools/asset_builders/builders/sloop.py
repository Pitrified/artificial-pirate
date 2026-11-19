"""A sloop after the Providence (1976 replica of a 1768 Continental sloop); see builders/ship.py.

Hull from its published particulars (classicsailboats.org), the rig measured on
references/sloop/providence_side.jpg, the sails from references/sloop/providence_sail.jpg.
"""

from __future__ import annotations

from builders.ship import build  # noqa: F401 - build.py calls the type's build

PARAMS = {
    "name": "Sloop",
    "length": 20.3,          # m, on deck
    "beam": 6.2,             # m
    "draft": 2.4,            # m
    "freeboard": 2.5,        # m, amidships
    "sheer_bow": 0.6,        # m
    "sheer_stern": 0.4,      # m
    "sheer_low": 0.45,
    "widest": 0.5,
    "transom": 0.6,
    "transom_rake_deg": 20.0,
    "bulwark": 0.9,          # m
    "band": 0.6,             # m
    "boot": 0.3,             # m
    "masts": [
        {
            "at": 0.69, "height": 28.5, "diameter": 0.55, "rake_deg": 5.0,
            "top_z": 17.6, "crosstrees_z": 23.0, "shrouds": 3,
            "yards": [{"z": 17.0, "length": 12.0}, {"z": 22.5, "length": 10.0}, {"z": 26.5, "length": 6.5}],
            "boom": {"z": 4.5, "length": 16.5, "rise_deg": 4.0},
            "gaff": {"z": 15.5, "length": 11.0, "peak_deg": 35.0},
        },
    ],
    "bowsprit": {"length": 10.0, "steeve_deg": 25.0, "diameter": 0.45},
    "sails": [
        {"kind": "gaff", "mast": 0},
        {"kind": "square", "mast": 0, "top": 1, "bottom": 0},
        {"kind": "square", "mast": 0, "top": 2, "bottom": 1},
        {"kind": "jib", "mast": 0, "head_z": 25.5, "tack": 1.0, "clew_aft": 11.0, "clew_z": 5.5},
        {"kind": "jib", "mast": 0, "head_z": 16.5, "tack": 0.0, "clew_aft": 6.5, "clew_z": 4.8},
    ],
    "wales": [1.25],
    "gun_ports": {"count": 6, "from": 0.2, "to": 0.75, "z": 2.05, "size": 0.5},
    "stern_windows": 3,
    "deck": {"hatches": [[0.45, 2.0, 1.6], [0.82, 1.2, 1.2]], "helm": "tiller"},
    "crew": [[0.12, 0.0], [0.5, 0.5], [0.62, -0.4], [0.85, 0.2]],
    "preview_rgb": {
        "HullBottom": [0.05, 0.05, 0.05],
        "Topsides": [0.85, 0.82, 0.74],
        "Trim": [0.55, 0.12, 0.10],
        "ShipWood": [0.42, 0.30, 0.18],
        "SailCloth": [0.86, 0.83, 0.75],
        "Tar": [0.08, 0.07, 0.06],
        "Deck": [0.55, 0.45, 0.33],
        "Gilt": [0.75, 0.58, 0.22],
        "Palette": [0.55, 0.40, 0.30],
        "Flag": [0.10, 0.10, 0.10],
    },
}
