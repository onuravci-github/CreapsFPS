using CreapsFPS.BaseJoystick;
using System;
using System.Collections.Generic;
using CreapsFPS.Enums;
using CreapsFPS.Managers;
using CreapsFPS.ScriptableObjects;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CreapsFPS.JoystickTypes
{
    public class TapJoystick : Joystick
    {
        #region Variables

        public Action JoystickPointerDown;
        public Action JoystickPointerUp;
        
        private float _startTime = -100;
        [SerializeField] private float _waitTime;
        [SerializeField] private bool _isDown;
        
        [SerializeField] private int _tapLimit = -1;
        [SerializeField] private int _tapCount;
        #endregion
        
        #region Touch Functions
        
        public override void OnPointerDown(PointerEventData eventData)
        {
            if (_tapCount >= _tapLimit && _tapLimit != -1)
            {
                return;
            }
            
            if (Time.time - _startTime <= _waitTime)
            {
                return;
            }
            
            _isDown = true;
            _startTime = Time.time;
            
            _background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
            base.OnPointerDown(eventData);
            JoystickPointerDown?.Invoke();
            _tapCount++;
        }
        
        public override void OnPointerUp(PointerEventData eventData)
        {
            if (!_isDown)
            {
                return;
            }
            _isDown = false;
            base.OnPointerUp(eventData);
            JoystickPointerUp?.Invoke();
        }
        
        #endregion
        
    }
}