"""A frigate after the Hermione (2014 replica of the 1779 French frigate); see builders/ship.py.

Hull from its published particulars (Wikipedia: 65 m overall, 11.24 m beam, 5.78 m draft; about 44 m
on deck), the rig's proportions and the paint from references/frigate/hermione_stern.jpg. Its main
battery is on the covered gun deck, the muzzles showing through a row of ports in the ochre band;
two chasers fire through the bow and two guns through the stern.
"""

from __future__ import annotations

from builders.ship import build, report  # noqa: F401 - build.py calls the type's build and report

PARAMS = {
    "name": "Frigate",
    "length": 44.2,          # m, on deck
    "beam": 11.2,            # m
    "draft": 5.4,            # m
    "freeboard": 5.0,        # m, amidships
    "sheer_bow": 0.9,        # m
    "sheer_stern": 0.5,      # m
    "sheer_low": 0.5,
    "widest": 0.48,
    "transom": 0.62,
    "transom_rake_deg": 10.0,
    "bulwark": 1.3,          # m
    "band": 3.0,             # m, the ochre band from the rail down past the gun deck's ports
    "boot": 0.5,             # m
    "raised_decks": [
        {"from": 0.0, "to": 0.24, "height": 1.0},   # the quarterdeck
        {"from": 0.86, "to": 1.0, "height": 0.8},   # the forecastle
    ],
    # The rig is the brig's (builders/brig.py) scaled to each mast's height: proportions it clears its
    # ropes at every wind with. The mizzen carries topsails and the spanker.
    "masts": [
        {
            "at": 0.77, "height": 45.0, "diameter": 0.9, "rake_deg": 3.0,
            "top_z": 18.3, "crosstrees_z": 32.0, "shrouds": 4,
            "yards": [
                {"z": 17.0, "length": 18.2}, {"z": 30.6, "length": 16.3},
                {"z": 39.1, "length": 11.1}, {"z": 43.7, "length": 8.5},
            ],
        },
        {
            "at": 0.5, "height": 51.0, "diameter": 1.0, "rake_deg": 4.0,
            "top_z": 21.5, "crosstrees_z": 36.1, "shrouds": 4,
            "yards": [
                {"z": 19.1, "length": 19.8}, {"z": 34.7, "length": 17.7},
                {"z": 43.9, "length": 12.0}, {"z": 48.9, "length": 9.2},
            ],
        },
        {
            "at": 0.2, "height": 38.0, "diameter": 0.7, "rake_deg": 6.0,
            "top_z": 15.3, "crosstrees_z": 26.9, "shrouds": 3,
            "yards": [
                {"z": 14.3, "length": 11.9}, {"z": 25.9, "length": 10.6},
                {"z": 32.7, "length": 7.2}, {"z": 36.4, "length": 5.5},
            ],
            "boom": {"z": 6.3, "length": 13.0, "rise_deg": 4.0},
            "gaff": {"z": 17.4, "length": 10.0, "peak_deg": 25.0},
        },
    ],
    "bowsprit": {"length": 17.0, "steeve_deg": 16.0, "diameter": 0.8},
    "sails": [
        {"kind": "square", "mast": 0, "top": 0, "bottom": {"z": 7.8, "length": 17.0}},
        {"kind": "square", "mast": 0, "top": 1, "bottom": 0},
        {"kind": "square", "mast": 0, "top": 2, "bottom": 1},
        {"kind": "square", "mast": 0, "top": 3, "bottom": 2},
        {"kind": "square", "mast": 1, "top": 0, "bottom": {"z": 8.5, "length": 18.5}},
        {"kind": "square", "mast": 1, "top": 1, "bottom": 0},
        {"kind": "square", "mast": 1, "top": 2, "bottom": 1},
        {"kind": "square", "mast": 1, "top": 3, "bottom": 2},
        {"kind": "square", "mast": 2, "top": 1, "bottom": 0},
        {"kind": "square", "mast": 2, "top": 2, "bottom": 1},
        {"kind": "square", "mast": 2, "top": 3, "bottom": 2},
        {"kind": "gaff", "mast": 2},
        {"kind": "jib", "mast": 0, "head_z": 35.5, "tack": 0.0, "clew_aft": 9.1, "clew_z": 7.8},
        {"kind": "jib", "mast": 0, "head_z": 41.2, "tack": 0.6, "clew_aft": 18.3, "clew_z": 10.4},
        {"kind": "jib", "mast": 0, "head_z": 45.0, "tack": 1.0, "clew_aft": 22.2, "clew_z": 14.3},
    ],
    "wales": [1.7, 3.4],
    "gun_ports": {"count": 13, "from": 0.17, "to": 0.82, "z": 2.55, "size": 0.62},
    "chase_ports": {"across": [-0.9, 0.9], "z": 3.0, "size": 0.55},
    "stern_ports": {"across": [-1.3, 1.3], "z": 3.0, "size": 0.55},
    "stern_windows": 7,
    "deck": {"hatches": [[0.6, 3.6, 3.0], [0.36, 2.6, 2.2], [0.88, 1.8, 1.8]], "companionway": 1, "capstan": 0.42,
             "helm": "wheel", "helm_at": 0.12},
    "crew": [[0.05, 0.5], [0.05, -0.5], [0.42, 0.4], [0.55, -0.5], [0.7, 0.0], [0.9, 0.3], [0.3, -0.3]],
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
