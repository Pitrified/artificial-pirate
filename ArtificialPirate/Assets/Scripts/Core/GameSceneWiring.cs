namespace Game.Core
{
    using UnityEngine;
    using Game.Systems;
    using Game.Gameplay;
    using Game.Gameplay.World;

    /// <summary>
    /// Wires Game scene components (ShipController, CameraController) with Persistent scene
    /// systems (InputManager); hands the world to GameManager to begin its seed, then wires the
    /// ship to the world's ground. Place this on any root object
    /// in Game.unity and assign the serialized references in the Inspector.
    /// </summary>
    public sealed class GameSceneWiring : MonoBehaviour
    {
        [SerializeField] private ShipController _ship;
        [SerializeField] private CameraController _camera;
        [SerializeField] private WorldStreamer _world;

        private void Start()
        {
            var gm = GameManager.Instance;
            if (gm != null)
            {
                gm.RegisterWorld(_world);
            }
            WireGround();

            if (gm == null)
            {
                Log.Warn(LogCat.Gameplay, "GameSceneWiring: GameManager.Instance is null - ship/camera not wired");
                return;
            }

            if (_ship == null)
            {
                Log.Warn(LogCat.Gameplay, "GameSceneWiring: ShipController not assigned");
            }
            else
            {
                _ship.Inject(gm.InputManager);
                gm.RegisterShip(_ship);
                Log.Info(LogCat.Gameplay, "GameSceneWiring: ShipController injected with InputManager");
            }

            if (_camera == null)
            {
                Log.Warn(LogCat.Gameplay, "GameSceneWiring: CameraController not assigned");
            }
            else if (_ship != null && _ship.Type == null)
            {
                Log.Warn(LogCat.Gameplay, "GameSceneWiring: the ship has no type - camera not wired");
            }
            else if (_ship != null)
            {
                _camera.SetTarget(_ship.transform, _ship.Type.CameraOffset, _ship.Type.CameraLookAt);
                _camera.SnapToTarget();
                Log.Info(LogCat.Gameplay, "GameSceneWiring: CameraController wired to ship");
            }
        }

        private void WireGround()
        {
            if (_world == null || _ship == null)
            {
                Log.Warn(LogCat.Gameplay, "GameSceneWiring: WorldStreamer or ShipController not assigned - the ship cannot run aground");
                return;
            }
            if (_ship.TryGetComponent(out HullContact contact))
            {
                contact.UseGround(_world.World);
            }
        }
    }
}
