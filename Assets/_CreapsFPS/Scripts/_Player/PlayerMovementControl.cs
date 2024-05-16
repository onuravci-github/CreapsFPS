using CreapsFPS.Enums;
using CreapsFPS.NetworkSystem;
using CreapsFPS.UI;
using Fusion;
using UnityEngine;

namespace CreapsFPS._Player
{    
    public class PlayerMovementControl : NetworkBehaviour
    {
        #region Network Variables
        
        [Networked] private int _animState { get; set; }
        [Networked] private NetworkBool _jumpStart { get; set; }
        [Networked] public NetworkBool IsFreeze { get; set; }
        [Networked] public TickTimer _freezeFinishTimer { get; set; }
        
        #endregion
        
        #region Variables
        
        [Header("Rotation Transforms")]
        [SerializeField] private Transform _bodyTransform;
        [SerializeField, Range(1, 180)] private int _rotationValue = 120;
        [SerializeField, Range(0, 300)] private int _autoRotationValue = 120;
        [SerializeField] private bool _isSpawn = false;
        [SerializeField] private float _gravity = -9.81f;

        [Header("Animation")]
        [SerializeField] private Animator _animator;
        [SerializeField] private string _animatorParameter = "Anim State";
        [SerializeField] private float _jumpPower;
        [SerializeField] private float _jumpStartTime;
        [SerializeField] private float _jumpTimeLimit;

        [Header("Freeze")] 
        [SerializeField] private float _freezeTime = 5f;
        
        #endregion

        #region Initialize
        
        public override void Spawned()
        {
            _isSpawn = true;
            base.Spawned();
            
            MainApp.OnGamePlaySceneStart += Initialize;
            _freezeFinishTimer = TickTimer.None;
        }

        public void Initialize()
        {
            if (Runner.LocalPlayer == Object.InputAuthority)
            {
                if (UIPlayerControllersManager.IsActive)
                {
                    UIPlayerControllersManager.Instance.PlayerJumpJoystick.JoystickPointerDown += RPC_JumpStart;
                }
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            MainApp.OnGamePlaySceneStart -= Initialize;
            
            if (UIPlayerControllersManager.IsActive)
            {
                UIPlayerControllersManager.Instance.PlayerJumpJoystick.JoystickPointerDown -= RPC_JumpStart;
            }
        }

        #endregion

        #region Movement & Rotate Character
        
        public override void FixedUpdateNetwork()
        {
            if (_freezeFinishTimer.Expired(Runner))
            {
                _freezeFinishTimer = TickTimer.None;
                IsFreeze = false;
                RPC_SetFreezeTimer(Object.InputAuthority, 0);
                RPC_Freeze(Object.InputAuthority, false);
            }
            if (IsFreeze)
            {
                if (!_freezeFinishTimer.IsRunning)
                {
                    RPC_SetFreezeTimer(Object.InputAuthority, _freezeTime);
                }
                return;
            }
            
            if (_isSpawn && GetInput(out NetworkInputData networkInputData))
            {
                Move(networkInputData);
                RotateCharacter(networkInputData);
                if (_jumpStart)
                {
                    Jump();
                }
            }
        }

        private void Move(NetworkInputData networkInputData)
        {
            if (networkInputData.MovementJoystickHorizontal != 0 || networkInputData.MovementJoystickVertical != 0)
            {
                transform.position += WalkDirection(networkInputData);
                //transform.localEulerAngles += Vector3.up * networkInputData.MovementJoystickHorizontal*Runner.DeltaTime *_autoRotationValue;
                MoveAnimationControl(networkInputData);
            }
            else
            {
                IdleAnimationControl();
            }
        }

        private void IdleAnimationControl()
        {
            if (!_jumpStart && _animState != (int)AnimState.Idle)
            {
                RPC_AnimStateChange((int)AnimState.Idle);
            }
        }
        
        private void MoveAnimationControl(NetworkInputData networkInputData)
        {
            bool isPlayerFast = (Mathf.Sqrt(Mathf.Pow(networkInputData.MovementJoystickHorizontal, 2) + Mathf.Pow(networkInputData.MovementJoystickVertical, 2)) > 0.5f);
            
            if(isPlayerFast && _animState != (int)AnimState.Run)
            {
                RPC_AnimStateChange((int)AnimState.Run);
            }
            else if (!isPlayerFast && _animState != (int)AnimState.Walk)
            {
                RPC_AnimStateChange((int)AnimState.Walk);
            }
        }
        
        private void RotateCharacter(NetworkInputData networkInputData)
        {
            transform.localEulerAngles += new Vector3(0, networkInputData.RotationJoystickHorizontal, 0) * Runner.DeltaTime * _rotationValue;
            _bodyTransform.localEulerAngles -= new Vector3(networkInputData.RotationJoystickVertical, 0, 0) * Runner.DeltaTime * _rotationValue / 2f;
        }

        private void Jump()
        {
            if (Time.time - _jumpStartTime <= _jumpTimeLimit*0.75f)
            {
                transform.position += Vector3.up * _jumpPower/2f;
            }
            else if (Time.time - _jumpStartTime <= _jumpTimeLimit)
            {
                transform.position += Vector3.up * _jumpPower;
            }
            else if (_jumpStart)
            {
                _jumpStart = false;
            }
        }

        private Vector3 WalkDirection(NetworkInputData networkInputData)
        {
            Vector3 directionForward = transform.forward;
            Vector3 directionRight = transform.right;

            Vector3 moveDirection = directionRight * networkInputData.MovementJoystickHorizontal + directionForward * networkInputData.MovementJoystickVertical;
            moveDirection += Vector3.up*_gravity/50f;
            
            return moveDirection;
        }
        #endregion
        
        #region RPC Methods
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_AnimStateChange(int animState)
        {
            if (_animState != animState)
            {
                _animState = animState;
                _animator.SetInteger(_animatorParameter,animState);
            }
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_JumpStart()
        {
            if (!_jumpStart && !IsFreeze)
            {
                _jumpStart = true;
                _jumpStartTime = Time.time;
                RPC_AnimStateChange((int)AnimState.Jump);
            }
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_Freeze(PlayerRef playerRef,bool value)
        {
            MainApp.Instance.GetPlayer(playerRef).GetComponent<PlayerMovementControl>().IsFreeze = value;
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_SetFreezeTimer(PlayerRef playerRef, float value)
        {
            if (value == 0)
            {
                MainApp.Instance.GetPlayer(playerRef).GetComponent<PlayerMovementControl>()._freezeFinishTimer = TickTimer.None;
            }
            else
            {
                MainApp.Instance.GetPlayer(playerRef).GetComponent<PlayerMovementControl>()._freezeFinishTimer = TickTimer.CreateFromSeconds(Runner,value);
            }
        }
        
        #endregion
    }
}