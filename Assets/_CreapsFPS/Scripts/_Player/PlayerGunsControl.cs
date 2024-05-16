using CreapsFPS.ScriptableObjects;
using CreapsFPS.Managers;
using CreapsFPS.UI;
using System.Collections.Generic;
using CreapsFPS.Enums;
using CreapsFPS.NetworkSystem;
using CreapsFPS.NetworkSystem.NetworkObjects;
using Fusion;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace CreapsFPS._Player
{
    public class PlayerGunsControl : NetworkBehaviour
    {
        #region Network Variables
        
        [Networked] private int _activeGunState { get; set; }
        [Networked] private NetworkBool _bombActive { get; set; }
        
        #endregion
        
        #region Variables

        [Header("Player Component")]
        [SerializeField] private PlayerMovementControl _playerMovementControl;
        
        [Header("Bomb")] 
        [SerializeField] private GameObject _bombObject;
        [SerializeField] private List<BombObject> _createdBombObjects;
            
        [Header("Gun")] 
        [SerializeField] private GameObject[] _guns;
        [SerializeField] private ParticleSystem[] _gunParticles;
        [SerializeField] private float[] _particleBasicSize = new []{0.25f,0.25f,0.25f};
        // for Create Bullet Direction 
        [SerializeField] private Transform[] _barrelTransforms;

        [Header("Camera Transforms")] 
        private Transform _cameraTransform;
        
        [Header("Layers")] 
        [SerializeField] private string _detectLayerName = "Targets";
        private int _detectLayer;
        
        [Header("Animator")] 
        [SerializeField] private Animator _animator;
        [SerializeField] private string _animatorParameter = "Anim Layer State";
        
        #endregion

        #region Initialize

        public override void Spawned()
        {
            base.Spawned();
            _detectLayer = LayerMask.NameToLayer(_detectLayerName);
            
            MainApp.OnGameStart += Initialize;
            RPC_GunChanged(0);
        }

        private void Initialize()
        {
            if (Runner.LocalPlayer == Object.InputAuthority)
            {
                _cameraTransform = GamePlayDataManager.Instance.MainCamera.transform;
                if (UIPlayerControllersManager.IsActive)
                {
                    SetGunFeature(0);
                    UIPlayerControllersManager.Instance.PlayerGunSelect.OnChangedSelect += RPC_GunChanged;
                    UIPlayerControllersManager.Instance.PlayerGunSelect.OnChangedSelect += SetGunFeature;
                    UIPlayerControllersManager.Instance.AttackJoystick.JoystickPointerDown += Attack;
                    UIPlayerControllersManager.Instance.BombJoystick.JoystickPointerDown += Bomb;
                }
            }
            
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            MainApp.OnGamePlaySceneStart -= Initialize;
            if (UIPlayerControllersManager.IsActive)
            {
                UIPlayerControllersManager.Instance.PlayerGunSelect.OnChangedSelect -= RPC_GunChanged;
                UIPlayerControllersManager.Instance.PlayerGunSelect.OnChangedSelect -= SetGunFeature;
                UIPlayerControllersManager.Instance.AttackJoystick.JoystickPointerDown -= Attack;
                UIPlayerControllersManager.Instance.BombJoystick.JoystickPointerDown -= Bomb;
            }
        }

        #endregion

        #region Animation Control
        
        public void AnimDefaultLayerState()
        {
            _animator.SetInteger(_animatorParameter, -1);
        }

        #endregion

        #region Attack Control
        
        private void Attack()
        {
            if (_playerMovementControl.IsFreeze)
            {
                return;
            }
            
            RPC_Detect(Runner.LocalPlayer,_cameraTransform.position,_cameraTransform.forward,_activeGunState);
            
            Debug.DrawRay(_cameraTransform.position,_cameraTransform.forward *200f,Color.green,1f);
        }

        private void Bomb()
        {
            if (_playerMovementControl.IsFreeze)
            {
                return;
            }
            
            if (_bombActive)
            {
                RPC_BombActivated(Object.InputAuthority);
            }
            else
            {
                RPC_BombCreate(Object.InputAuthority,this.transform.position + transform.forward*10f);
            }
        }
        
        #endregion

        #region GunControl

        private void SetGunFeature(int index)
        {
            UIPlayerControllersManager.Instance.AttackJoystick.SetGunFeature(index);
        }

        #endregion
        
        #region RPC Methods
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_Detect(PlayerRef playerRef,Vector3 position,Vector3 direction,int gunIndex)
        {
            BulletColor bulletColor = (BulletColor) MainApp.Instance.GetPlayer(playerRef).BulletColorIndex;
            BulletSize bulletSize =  (BulletSize)MainApp.Instance.GetPlayer(playerRef).BulletSizeIndex;
            
            if (Object.HasStateAuthority)
            {
                float bulletRange = GameDataManager.Instance.GunFeatureTypes[gunIndex].BulletRange;
                float bulletPower = GameDataManager.Instance.GunFeatureTypes[gunIndex].AttackPower;
                
                List<LagCompensatedHit> hits = new List<LagCompensatedHit>();
                Runner.LagCompensation.RaycastAll(position,direction,bulletRange, Object.InputAuthority, hits, 1 << _detectLayer);

                foreach (var hit in hits)
                {
                    if (hit.GameObject.TryGetComponent(out TargetObject targetObject))
                    {
                        targetObject.RPC_Damaged(bulletPower,Object.InputAuthority,bulletColor,bulletSize);
                        
                    }
                }
                
            }
            
            if (playerRef == Object.InputAuthority)
            {
                RPC_ShotParticle((int)bulletColor, (int)bulletSize);
            }
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_GunChanged(int selectedIndex)
        {

            for (int i = 0; i < _guns.Length; i++)
            {
                if (i == selectedIndex)
                {
                    _guns[i].SetActive(true);
                    if (_activeGunState != i + 1)
                    {
                        _activeGunState = i;
                        RPC_AnimatorLayerChange(i + 1);
                    }
                }
                else
                {
                    _guns[i].SetActive(false);
                }
            }
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_AnimatorLayerChange(int layerIndex)
        {
            _animator.SetInteger(_animatorParameter, layerIndex);
        }
        
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_BombCreate(PlayerRef playerRef,Vector3 spawnPoint)
        {
            if (Object.HasStateAuthority)
            {
                NetworkObject bombObject = Runner.Spawn(_bombObject, spawnPoint, quaternion.identity);
                bombObject.GetComponent<BombObject>().Initialize(playerRef,spawnPoint);
                _createdBombObjects.Add(bombObject.GetComponent<BombObject>());
            }

            MainApp.Instance.GetPlayer(playerRef).GetComponent<PlayerGunsControl>()._bombActive = true;
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_BombActivated(PlayerRef playerRef)
        {
            if (Object.HasStateAuthority)
            {
                BombObject destroyObject = null;
                foreach (var bombObject in _createdBombObjects)
                {
                    if (bombObject.BombCreatorPlayerRef == playerRef)
                    {
                        bombObject.BombActive();
                        destroyObject = bombObject;
                    } 
                }
                _createdBombObjects.Remove(destroyObject.GetComponent<BombObject>());
            }
            MainApp.Instance.GetPlayer(playerRef).GetComponent<PlayerGunsControl>()._bombActive = false;
        }
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        private void RPC_ShotParticle(int colorIndex, int sizeIndex)
        {
            _gunParticles[_activeGunState].gameObject.SetActive(false);
            _gunParticles[_activeGunState].startColor = GameDataManager.Instance.GunParticleColors[colorIndex];
            _gunParticles[_activeGunState].startSize = _particleBasicSize[sizeIndex] * (sizeIndex + 1);
            _gunParticles[_activeGunState].gameObject.SetActive(true);
        }
        
        #endregion
    }
}