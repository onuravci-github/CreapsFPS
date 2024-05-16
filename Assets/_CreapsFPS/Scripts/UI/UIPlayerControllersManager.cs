using CreapsFPS.NetworkSystem;
using CreapsFPS.Utils;
using CreapsFPS.JoystickTypes;
using CreapsFPS.JoystickTypes.SelectableJoysticks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace CreapsFPS.UI
{
    public class UIPlayerControllersManager : SceneSingleton<UIPlayerControllersManager>
    {
        #region Variables Getter Setter

        public ClassicJoystick MoveJoystick
        {
            get => _moveJoystick;
        }

        public ClassicJoystick RotationJoystick
        {
            get => _rotationJoystick;
        }

        public AttackJoystick AttackJoystick
        {
            get => _attackJoystick;
        }
        
        public BulletColorSelect BulletColorSelect
        {
            get => _bulletColorSelect;
        }

        public BulletSizeSelect BulletSizeSelect
        {
            get => _bulletSizeSelect;
        }

        public PlayerGunSelect PlayerGunSelect
        {
            get => _playerGunSelect;
        }

        public TapJoystick PlayerJumpJoystick
        {
            get => _playerJumpJoystick;
        }

        public TapJoystick BombJoystick
        {
            get => _bombJoystick;
        }

        #endregion

        #region Variables

        [Header("Joysticks")] 
        [SerializeField] private ClassicJoystick _moveJoystick;
        [SerializeField] private ClassicJoystick _rotationJoystick;
        [SerializeField] private AttackJoystick _attackJoystick;
        [SerializeField] private TapJoystick _playerJumpJoystick;
        [SerializeField] private TapJoystick _bombJoystick;
        
        [Header("Selectable Joysticks")] 
        [SerializeField] private BulletColorSelect _bulletColorSelect;
        [SerializeField] private BulletSizeSelect _bulletSizeSelect;
        [SerializeField] private PlayerGunSelect _playerGunSelect;

        #endregion
        
        #region Network Data Transfer
        
        public NetworkInputData GetNetworkInputs()
        {
            NetworkInputData networkInputData = new NetworkInputData();

            networkInputData.MovementJoystickHorizontal = _moveJoystick.Horizontal;
            networkInputData.MovementJoystickVertical = _moveJoystick.Vertical;

            networkInputData.RotationJoystickHorizontal = _rotationJoystick.Horizontal;
            networkInputData.RotationJoystickVertical = _rotationJoystick.Vertical;

            networkInputData.AttackJoystickHorizontal = _attackJoystick.Horizontal;
            networkInputData.AttackJoystickVertical = _attackJoystick.Vertical;

            return networkInputData;
        }
        
        #endregion
    }
}