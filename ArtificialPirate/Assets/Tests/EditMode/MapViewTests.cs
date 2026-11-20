namespace Game.Tests.EditMode
{
    using NUnit.Framework;
    using UnityEngine;
    using Game.UI;

    /// <summary>
    /// <see cref="MapView"/>: the view opens on the ship, a drag moves the map with the finger, a
    /// pinch zooms about its midpoint, the zoom and the centre stay within the map, and the
    /// buttons' centre on the ship and reset zoom.
    /// </summary>
    public sealed class MapViewTests
    {
        private const float Frame = 600f;
        private const float Tolerance = 1e-4f;

        private static MapView Opened(Vector2 ship, float start = 0.25f, float closest = 0.1f)
        {
            var view = new MapView();
            view.Show(ship, start, closest);
            return view;
        }

        [Test]
        public void Show_CentresOnTheShip_AtTheStartingZoom()
        {
            MapView view = Opened(new Vector2(0.5f, 0.4f));

            Assert.AreEqual(0.25f, view.Share, Tolerance);
            Assert.AreEqual(0.5f, view.Centre.x, Tolerance);
            Assert.AreEqual(0.4f, view.Centre.y, Tolerance);
            Rect image = view.ImageRect(Frame);
            Assert.AreEqual(Frame * 4f, image.width, Tolerance, "a quarter of the map fills the frame");
        }

        [Test]
        public void Show_AStartWiderThanTheMap_ShowsAllOfIt()
        {
            MapView view = Opened(new Vector2(0.3f, 0.7f), start: 2.3f);

            Assert.AreEqual(1f, view.Share, Tolerance);
            Assert.AreEqual(0.5f, view.Centre.x, Tolerance);
            Assert.AreEqual(0.5f, view.Centre.y, Tolerance);
        }

        [Test]
        public void Show_NearTheEdge_KeepsTheViewOnTheMap()
        {
            MapView view = Opened(new Vector2(0.05f, 0.95f));

            Assert.AreEqual(0.125f, view.Centre.x, Tolerance);
            Assert.AreEqual(0.875f, view.Centre.y, Tolerance);
        }

        [Test]
        public void Drag_TheMapPointUnderTheFingerFollowsIt()
        {
            MapView view = Opened(new Vector2(0.5f, 0.5f));
            var from = new Vector2(300f, 300f);
            var to = new Vector2(360f, 240f);
            Vector2 held = view.UvAt(from, Frame);

            view.PointerDown(1, from);
            view.PointerMove(1, to, Frame);

            Vector2 under = view.UvAt(to, Frame);
            Assert.AreEqual(held.x, under.x, Tolerance);
            Assert.AreEqual(held.y, under.y, Tolerance);
        }

        [Test]
        public void Drag_StopsAtTheMapsEdge()
        {
            MapView view = Opened(new Vector2(0.5f, 0.5f));

            view.PointerDown(1, Vector2.zero);
            view.PointerMove(1, new Vector2(100000f, 100000f), Frame);

            Assert.AreEqual(0.125f, view.Centre.x, Tolerance, "dragged right as far as the west edge");
            Assert.AreEqual(0.875f, view.Centre.y, Tolerance, "dragged down as far as the north edge");
        }

        [Test]
        public void Pinch_ZoomsByTheFingersSpread_AboutTheirMidpoint()
        {
            MapView view = Opened(new Vector2(0.5f, 0.5f));
            var mid = new Vector2(250f, 350f);
            Vector2 held = view.UvAt(mid, Frame);

            view.PointerDown(1, mid + new Vector2(-50f, 0f));
            view.PointerDown(2, mid + new Vector2(50f, 0f));
            view.PointerMove(2, mid + new Vector2(150f, 0f), Frame); // spread 100 -> 200, midpoint moves 50
            Vector2 newMid = mid + new Vector2(50f, 0f);

            Assert.AreEqual(0.125f, view.Share, Tolerance, "twice the spread, twice as close");
            Vector2 under = view.UvAt(newMid, Frame);
            Assert.AreEqual(held.x, under.x, Tolerance, "the point pinched follows the midpoint");
            Assert.AreEqual(held.y, under.y, Tolerance);
        }

        [Test]
        public void Pinch_StopsAtTheZoomsLimits()
        {
            MapView view = Opened(new Vector2(0.5f, 0.5f));

            view.PointerDown(1, new Vector2(290f, 300f));
            view.PointerDown(2, new Vector2(310f, 300f));
            view.PointerMove(2, new Vector2(900f, 300f), Frame);
            Assert.AreEqual(0.1f, view.Share, Tolerance, "no closer than the closest view");

            view.PointerMove(2, new Vector2(292f, 300f), Frame);
            Assert.AreEqual(1f, view.Share, Tolerance, "no wider than the whole map");
        }

        [Test]
        public void LiftingOneOfTwoFingers_TheOtherDragsOn()
        {
            MapView view = Opened(new Vector2(0.5f, 0.5f));
            view.PointerDown(1, new Vector2(200f, 300f));
            view.PointerDown(2, new Vector2(400f, 300f));
            view.PointerUp(1);
            float share = view.Share;
            Vector2 held = view.UvAt(new Vector2(400f, 300f), Frame);

            view.PointerMove(2, new Vector2(420f, 330f), Frame);

            Assert.AreEqual(share, view.Share, Tolerance, "one finger does not zoom");
            Vector2 under = view.UvAt(new Vector2(420f, 330f), Frame);
            Assert.AreEqual(held.x, under.x, Tolerance);
            Assert.AreEqual(held.y, under.y, Tolerance);
        }

        [Test]
        public void CentreOnShip_KeepsTheZoom_ResetZoom_KeepsTheCentre()
        {
            var ship = new Vector2(0.5f, 0.5f);
            MapView view = Opened(ship);
            view.PointerDown(1, new Vector2(300f, 300f));
            view.PointerDown(2, new Vector2(320f, 300f));
            view.PointerMove(2, new Vector2(340f, 300f), Frame);
            view.PointerUp(2);
            view.PointerMove(1, new Vector2(500f, 450f), Frame);
            view.PointerUp(1);
            float zoomed = view.Share;

            view.CentreOnShip();
            Assert.AreEqual(zoomed, view.Share, Tolerance);
            Assert.AreEqual(ship.x, view.Centre.x, Tolerance);
            Assert.AreEqual(ship.y, view.Centre.y, Tolerance);

            view.ResetZoom();
            Assert.AreEqual(0.25f, view.Share, Tolerance);
            Assert.AreEqual(ship.x, view.Centre.x, Tolerance);
            Assert.AreEqual(ship.y, view.Centre.y, Tolerance);
        }
    }
}
