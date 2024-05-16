using Fusion;
using UnityEngine;

namespace CreapsFPS.NetworkSystem
{
    public struct NetworkInputData : INetworkInput
    {
        [Header("Movement Joystick")] 
        public float MovementJoystickHorizontal;
        public float MovementJoystickVertical;

        [Header("Rotation Joystick")] 
        public float RotationJoystickHorizontal;
        public float RotationJoystickVertical;

        [Header("Attack Joystick")] 
        public float AttackJoystickHorizontal;
        public float AttackJoystickVertical;

    }
}