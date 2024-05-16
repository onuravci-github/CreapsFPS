using System.Collections;
using System.Collections.Generic;
using CreapsFPS._Player;
using CreapsFPS.Enums;
using CreapsFPS.Gameplay;
using CreapsFPS.Managers;
using CreapsFPS.UI;
using Fusion;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace CreapsFPS.NetworkSystem.NetworkObjects
{
    public class TargetObject : NetworkBehaviour
    {
        #region Network Variables

        [Networked] public float Health { get; set; }
        [Networked] private NetworkBool _isDeath { get; set; }
        [Networked] private Vector3 _startPosition { get; set; }
        [Networked] private PlayerRef _lastHitPlayerRef { get; set; }
        [Networked] private int _lastHitBulletColor { get; set; }
        [Networked] private int _lastHitBulletSize { get; set; }
        [Networked] private TickTimer _deSpawnTimer { get; set; }
        #endregion
        
        #region Variables

        [Header("Feature")]
        public BulletColor ColorNumb;
        [SerializeField] private int _score = 10;
        [SerializeField] private float _maxHealth;
        
        [Header("Render Rotate")]
        [Range(-180,180)][SerializeField] private float _rotateValue;
        [FormerlySerializedAs("randomRotate")] [SerializeField] private bool _randomRotate = false;
        [Range(-180,0)][SerializeField] private int _randomRotateMin;
        [Range(0f,180)][SerializeField] private int _randomRotateMax;

        [Header("Particles")]
        [SerializeField] private ParticleSystem _hitParticle;
        [SerializeField] private ParticleSystem _destroyParticle;
        [SerializeField] private float _particleBasicSize = 0.25f;
        [SerializeField] private float _destroyTime = 0.25f;
        
        #endregion

        #region Initialize

        public override void Spawned()
        {
            Health = _maxHealth;
            _rotateValue = Random.Range(_randomRotateMin, _randomRotateMax);
            transform.SetParent(TargetShooterGameControl.Instance.SpawnParent);
        }

        public void Initialize(Vector3 position)
        {
            transform.position = position;
            
            List<LagCompensatedHit> hits = new List<LagCompensatedHit>();
            Runner.LagCompensation.OverlapSphere(this.transform.position, 10f, Object.InputAuthority, hits,1 << LayerMask.NameToLayer("Targets"));

            foreach (var hit in hits)
            {
                if (hit.GameObject.GetComponent<TargetObject>() != this)
                {
                    Runner.Despawn(this.GetComponent<NetworkObject>());
                }
            }
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
            if (_deSpawnTimer.Expired(Runner))
            {
                _deSpawnTimer = TickTimer.None;
                Runner.Despawn(GetComponent<NetworkObject>());
                return;
            }
            
            if (_isDeath)
            {
                return;
            }
            
            if (Object.HasStateAuthority)
            {
                transform.localEulerAngles += Vector3.up * _rotateValue * Runner.DeltaTime;
                
                if (Health <= 0)
                {
                    _isDeath = true;


                    bool colorEquals = (int) ColorNumb == _lastHitBulletColor && (int) ColorNumb == MainApp.Instance.GetPlayer(_lastHitPlayerRef).BulletColorIndex;
                    bool gainScore = colorEquals && _lastHitBulletSize == MainApp.Instance.GetPlayer(_lastHitPlayerRef).BulletSizeIndex;
                    
                    if (gainScore)
                    {
                        Player.GainScore?.Invoke(_lastHitPlayerRef,_score);
                    }
                    else
                    {
                        Player.GainScore?.Invoke(_lastHitPlayerRef,-_score);
                    }
                    
                    RPC_OnDestroyParticle();
                    _deSpawnTimer = TickTimer.CreateFromSeconds(Runner,_destroyTime);
                }
            }
        }

        #endregion

        #region RPC Methods

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_Damaged(float attackPower,PlayerRef playerRef,BulletColor lastHitBulletColor,BulletSize lastHitBulletSize)
        {
            if (_isDeath)
            {
                return;
            }
            
            if (Health > 0)
            {
                Health -= attackPower;
                _lastHitPlayerRef = playerRef;
                _lastHitBulletColor = (int)lastHitBulletColor;
                _lastHitBulletSize = (int)lastHitBulletSize;
            }
            
            _hitParticle.gameObject.SetActive(false);
            _hitParticle.startColor = GameDataManager.Instance.GunParticleColors[(int)ColorNumb];
            _hitParticle.startSize = _particleBasicSize * _particleBasicSize * ((int)lastHitBulletSize + 1);  ;
            _hitParticle.gameObject.SetActive(true);
        }
        
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_OnDestroyParticle()
        {
            _destroyParticle.gameObject.SetActive(true);
        }
        
        #endregion

    }
}