using CreapsFPS.BaseJoystick;
using UnityEngine.EventSystems;

namespace CreapsFPS.JoystickTypes
{

    public class ClassicJoystick : Joystick
    {
        #region Touch Functions
        
        public override void OnPointerDown(PointerEventData eventData)
        {
            _background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
            base.OnPointerDown(eventData);
        }
        
        #endregion

    }

}