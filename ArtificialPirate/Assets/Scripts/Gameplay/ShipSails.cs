namespace Game.Gameplay
{
    using System;
    using UnityEngine;

    /// <summary>
    /// The sails a ship shows: furled at anchor; set under way, each standing mast showing the
    /// sailing state for the band its heading puts the world's wind in (<see cref="SO_SailTrim"/>);
    /// and the flag streaming downwind. A broken mast is left alone. On <c>PF_Ship</c>'s root, its
    /// masts set on each variant. See docs/systems/ship-controller.md.
    /// </summary>
    public sealed class ShipSails : MonoBehaviour
    {
        /// <summary>One mast's sail states: furled, and set for each of the wind's bands.</summary>
        [Serializable]
        public struct MastSails
        {
            [Tooltip("The sails furled.")]
            [SerializeField] private GameObject _anchored;

            [Tooltip("The sails set, one object for each of the trim's bands, in its order.")]
            [SerializeField] private GameObject[] _bands;

            /// <summary>Initializes a new <see cref="MastSails"/>.</summary>
            public MastSails(GameObject anchored, GameObject[] bands)
            {
                _anchored = anchored;
                _bands = bands;
            }

            /// <summary>The sails furled.</summary>
            public GameObject Anchored => _anchored;

            /// <summary>The sails set, one for each band.</summary>
            public GameObject[] Bands => _bands;
        }

        private const int Furled = -1;
        private const float Downwind = 180f;

        [SerializeField] private SO_SailTrim _trim;

        [Tooltip("Under way with every sail set; off, at anchor with the sails furled.")]
        [SerializeField] private bool _set = true;

        [SerializeField] private MastSails[] _masts = Array.Empty<MastSails>();

        [Tooltip("The masts, so a broken one is left alone; none, and every mast stands.")]
        [SerializeField] private ShipMasts _shipMasts;

        [Tooltip("The flag, built streaming aft from its origin at the masthead.")]
        [SerializeField] private Transform _flag;

        [Tooltip("The mast that carries the flag.")]
        [SerializeField] private int _flagMast;

        private Transform _transform;
        private Quaternion _flagRest = Quaternion.identity;
        private Vector3 _flagAxis = Vector3.up;
        private float _windFromDeg;
        private int _band = Furled;
        private bool _started;

        /// <summary>Whether the sails are set (under way) rather than furled.</summary>
        public bool Set => _set;

        /// <summary>The band shown, or -1 while furled.</summary>
        public int Band => _band;

        /// <summary>Where the wind comes from, in degrees clockwise from north.</summary>
        public float WindFromDeg => _windFromDeg;

        private void Awake() => Begin();

        private void Update() => Show();

        /// <summary>The wind to trim to, from <paramref name="windFromDeg"/> clockwise from north.</summary>
        public void UseWind(float windFromDeg)
        {
            _windFromDeg = windFromDeg;
            Show();
        }

        /// <summary>Sets the sails (<c>true</c>) or furls them.</summary>
        public void SetSails(bool set)
        {
            _set = set;
            Show();
        }

        /// <summary>Wires the trim, the masts and the flag (ships built in code: tests).</summary>
        internal void Configure(SO_SailTrim trim, MastSails[] masts, ShipMasts shipMasts, Transform flag, int flagMast, bool set)
        {
            _trim = trim;
            _masts = masts;
            _shipMasts = shipMasts;
            _flag = flag;
            _flagMast = flagMast;
            _set = set;
            _started = false;
            Begin();
        }

        private void Begin()
        {
            if (_started)
            {
                return;
            }
            _started = true;
            _transform = transform;
            if (_flag != null)
            {
                _flagRest = _flag.localRotation;
                _flagAxis = _flag.parent != null ? _flag.parent.InverseTransformDirection(_transform.up) : Vector3.up;
            }
            _band = Furled;
            ShowAll(_set && _trim != null ? _trim.Band(OffBow(_windFromDeg), Furled) : Furled);
        }

        /// <summary>Shows the band the heading puts the wind in, and turns the flag downwind.</summary>
        internal void Show()
        {
            if (!_started || _trim == null)
            {
                return;
            }
            int band = _set ? _trim.Band(OffBow(_windFromDeg), _band) : Furled;
            if (band != _band)
            {
                ShowAll(band);
            }
            if (_flag != null && !Broken(_flagMast))
            {
                _flag.localRotation = Quaternion.AngleAxis(OffBow(_windFromDeg + Downwind) - Downwind, _flagAxis) * _flagRest;
            }
        }

        /// <summary>Where a bearing lies off the bow, in degrees, positive on starboard.</summary>
        private float OffBow(float bearingDeg) => Mathf.DeltaAngle(_transform.eulerAngles.y, bearingDeg);

        private bool Broken(int mast) => _shipMasts != null && _shipMasts.IsBroken(mast);

        private void ShowAll(int band)
        {
            _band = band;
            for (int i = 0; i < _masts.Length; i++)
            {
                if (Broken(i))
                {
                    continue;
                }
                MastSails mast = _masts[i];
                if (mast.Anchored != null)
                {
                    mast.Anchored.SetActive(band == Furled);
                }
                GameObject[] bands = mast.Bands ?? Array.Empty<GameObject>();
                for (int b = 0; b < bands.Length; b++)
                {
                    if (bands[b] != null)
                    {
                        bands[b].SetActive(b == band);
                    }
                }
            }
        }
    }
}
