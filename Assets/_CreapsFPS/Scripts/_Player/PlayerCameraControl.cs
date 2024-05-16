using CreapsFPS.Managers;
using CreapsFPS.NetworkSystem;
using Fusion;
using UnityEngine;

namespace CreapsFPS._Player
{
    public class PlayerCameraControl : NetworkBehaviour
    {
        #region Variables Getter Setter

        public Transform TargetTransform
        {
            get => _targetTransform;
        }

        #endregion
        
        #region Variables

        [SerializeField] private Transform _targetTransform;

        #endregion

        #region Initialize
        
        public override void Spawned()
        {
            base.Spawned();

            MainApp.OnGameStart += Initialize;

        }

        public void Initialize()
        {
            if (Runner.LocalPlayer == Object.InputAuthority)
            {
                if (GamePlayDataManager.IsActive)
                {
                    GamePlayDataManager.Instance.MainVirtualCamera.Follow = _targetTransform;
                }
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            MainApp.OnGameStart -= Initialize;
        }

        #endregion

    }
}