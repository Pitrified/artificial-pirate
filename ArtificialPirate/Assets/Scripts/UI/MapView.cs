namespace Game.UI
{
    using UnityEngine;

    /// <summary>
    /// What part of the full map is in view, and the gestures that move it: one pointer drags,
    /// two pinch, zooming about their midpoint and following it. The map is a square picture in
    /// uv (0 to 1 from its south-west corner); the view is a square frame of
    /// <c>frame</c> pixels, y down, showing <see cref="Share"/> of the map's side around
    /// <see cref="Centre"/>, and never past the map's edge. See docs/systems/world.md.
    /// </summary>
    public sealed class MapView
    {
        private const int NoPointer = int.MinValue;
        private const float MinPinch = 1f; // px between two fingers below which a pinch is not measured

        private Vector2 _ship;
        private float _start = 1f;
        private float _closest = 1f;

        private int _firstId = NoPointer;
        private int _secondId = NoPointer;
        private Vector2 _first;
        private Vector2 _second;

        /// <summary>The ship's place on the map (uv).</summary>
        public Vector2 Ship => _ship;

        /// <summary>The map point at the frame's centre (uv).</summary>
        public Vector2 Centre { get; private set; } = new Vector2(0.5f, 0.5f);

        /// <summary>The share of the map's side in view: 1 shows all of it.</summary>
        public float Share { get; private set; } = 1f;

        /// <summary>
        /// Starts a view centred on the ship at <paramref name="shipUv"/>, showing
        /// <paramref name="startShare"/> of the map's side, zooming in as far as
        /// <paramref name="closestShare"/>. Shares above 1 are taken as 1.
        /// </summary>
        public void Show(Vector2 shipUv, float startShare, float closestShare)
        {
            _ship = shipUv;
            _closest = Mathf.Clamp(closestShare, Mathf.Epsilon, 1f);
            _start = Mathf.Clamp(startShare, _closest, 1f);
            _firstId = NoPointer;
            _secondId = NoPointer;
            Share = _start;
            Centre = shipUv;
            Clamp();
        }

        /// <summary>Centres the view on the ship, as near as the map's edge allows; the zoom is kept.</summary>
        public void CentreOnShip()
        {
            Centre = _ship;
            Clamp();
        }

        /// <summary>Goes back to the starting zoom; the centre is kept.</summary>
        public void ResetZoom()
        {
            Share = _start;
            Clamp();
        }

        /// <summary>Moves the map with a drag of <paramref name="delta"/> pixels.</summary>
        public void Pan(Vector2 delta, float frame)
        {
            float perPixel = Share / frame;
            Centre = new Vector2(Centre.x - delta.x * perPixel, Centre.y + delta.y * perPixel);
            Clamp();
        }

        /// <summary>
        /// Zooms in by <paramref name="factor"/> (below 1 zooms out), keeping the map point under
        /// <paramref name="point"/> (frame pixels) where it is, within the zoom's limits.
        /// </summary>
        public void ZoomAbout(Vector2 point, float factor, float frame)
        {
            Vector2 held = UvAt(point, frame);
            Share = Mathf.Clamp(Share / factor, _closest, 1f);
            float perPixel = Share / frame;
            float half = frame * 0.5f;
            Centre = new Vector2(held.x - (point.x - half) * perPixel, held.y + (point.y - half) * perPixel);
            Clamp();
        }

        /// <summary>The map point (uv) under <paramref name="point"/> (frame pixels).</summary>
        public Vector2 UvAt(Vector2 point, float frame)
        {
            float perPixel = Share / frame;
            float half = frame * 0.5f;
            return new Vector2(Centre.x + (point.x - half) * perPixel, Centre.y - (point.y - half) * perPixel);
        }

        /// <summary>The map picture's place in the frame: its top-left corner and side, in pixels.</summary>
        public Rect ImageRect(float frame)
        {
            float side = frame / Share;
            float half = frame * 0.5f;
            return new Rect(half - Centre.x * side, half - (1f - Centre.y) * side, side, side);
        }

        /// <summary>A pointer touches the frame at <paramref name="point"/>; a third is ignored.</summary>
        public void PointerDown(int id, Vector2 point)
        {
            if (_firstId == NoPointer)
            {
                _firstId = id;
                _first = point;
            }
            else if (_secondId == NoPointer && id != _firstId)
            {
                _secondId = id;
                _second = point;
            }
        }

        /// <summary>A pointer moves to <paramref name="point"/>: a drag with one, a pinch with two.</summary>
        public void PointerMove(int id, Vector2 point, float frame)
        {
            bool isFirst = id == _firstId;
            if (!isFirst && id != _secondId)
            {
                return;
            }
            Vector2 last = isFirst ? _first : _second;
            if (_secondId == NoPointer)
            {
                Pan(point - last, frame);
            }
            else
            {
                Vector2 other = isFirst ? _second : _first;
                float before = Vector2.Distance(last, other);
                float after = Vector2.Distance(point, other);
                Vector2 midBefore = (last + other) * 0.5f;
                Vector2 midAfter = (point + other) * 0.5f;
                if (before > MinPinch && after > MinPinch)
                {
                    ZoomAbout(midBefore, after / before, frame);
                }
                Pan(midAfter - midBefore, frame);
            }
            if (isFirst)
            {
                _first = point;
            }
            else
            {
                _second = point;
            }
        }

        /// <summary>A pointer lifts; with two down, the other one drags on.</summary>
        public void PointerUp(int id)
        {
            if (id == _firstId)
            {
                _firstId = _secondId;
                _first = _second;
                _secondId = NoPointer;
            }
            else if (id == _secondId)
            {
                _secondId = NoPointer;
            }
        }

        private void Clamp()
        {
            float half = Share * 0.5f;
            Centre = new Vector2(Mathf.Clamp(Centre.x, half, 1f - half), Mathf.Clamp(Centre.y, half, 1f - half));
        }
    }
}
