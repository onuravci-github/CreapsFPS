using System.Collections.Generic;
using CreapsFPS._Player;
using CreapsFPS.Enums;
using CreapsFPS.Gameplay;
using Fusion;
using UnityEngine;

namespace CreapsFPS.NetworkSystem.NetworkObjects
{
    public class BombObject : NetworkBehaviour
    {
        #region Network Variables
        
        [Networked] private TickTimer _bombTimer { get; set; }
        [Networked] private TickTimer _bombDestroyTimer { get; set; }
        [Networked] public PlayerRef BombCreatorPlayerRef { get; set; }
        
        #endregion
        
        #region Variables

        // _bombTime -1 : Manuel Explode Bomb
        [SerializeField] private int _bombTime = -1;
        [SerializeField] private int _destroyTime = 5;
        [SerializeField] private Vector3 _bombStarRotation = new Vector3(-90, 0, 0);
        [SerializeField] private float _bombEffectRadius = 100f;
        
        [Header("Particles")]
        [SerializeField] private ParticleSystem _destroyParticle;
        
        #endregion

                
        #region Initialize

        public override void Spawned()
        {
            transform.localEulerAngles = _bombStarRotation;
            
            if (_bombTime != -1)
            {
                SetBombTimer(_bombTime);
            }
        }

        public void Initialize(PlayerRef playerRef,Vector3 position)
        {
            transform.position = position;
            RPC_SetCreator(playerRef);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (Object.HasStateAuthority)
            {
                TargetShooterGameControl.Instance.SpawnCount--;
            }
        }

        #endregion

        #region Updates

        public override void Render()
        {
            if (Object.HasStateAuthority)
            {
                if (_bombTimer.Expired(Runner))
                {
                    _bombTimer = TickTimer.None;
                    BombActive();
                }

                if (_bombDestroyTimer.Expired(Runner))
                {
                    _bombDestroyTimer = TickTimer.None;
                    Runner.Despawn(this.GetComponent<NetworkObject>());
                }
                
            }
        }

        public void BombActive()
        {
            if (Object.HasStateAuthority)
            {
                List<LagCompensatedHit> hits = new List<LagCompensatedHit>();
                Runner.LagCompensation.OverlapSphere(transform.position, _bombEffectRadius, Object.InputAuthority, hits,
                    1 << LayerMask.NameToLayer("Player"));

                foreach (var hit in hits)
                {
                    var playerMovementControl = hit.GameObject.GetComponent<PlayerMovementControl>();
                    if (playerMovementControl && playerMovementControl.Object.InputAuthority != BombCreatorPlayerRef)
                    {
                        playerMovementControl.RPC_Freeze(playerMovementControl.Object.InputAuthority, true);
                    }
                }

                RPC_DestroyParticle();
                SetBombDestroyTimer(_destroyTime);
            }
        }

        public void SetBombTimer(float activeStageChangeTime)
        {
            _bombTimer = TickTimer.CreateFromSeconds(Runner, activeStageChangeTime);
        }
        
        public void SetBombDestroyTimer(float activeStageChangeTime)
        {
            _bombDestroyTimer = TickTimer.CreateFromSeconds(Runner, activeStageChangeTime);
        }
        #endregion

        #region RPC Methods

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_SetCreator(PlayerRef playerRef)
        {
            BombCreatorPlayerRef = playerRef;
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_DestroyParticle()
        {
            _destroyParticle.startSize = _bombEffectRadius;
            _destroyParticle.startLifetime = _destroyTime;
            _destroyParticle.gameObject.SetActive(true);
        }
        
        #endregion
    }
}
