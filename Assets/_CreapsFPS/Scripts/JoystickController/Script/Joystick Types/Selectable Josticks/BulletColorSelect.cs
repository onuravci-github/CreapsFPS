using CreapsFPS.Enums;
using UnityEngine;

namespace CreapsFPS.JoystickTypes.SelectableJoysticks
{

    public class BulletColorSelect : SelectableJoystick
    {
        #region Variables
        
        public BulletColor ActiveColorType;
        [SerializeField] private BulletColor[] _selectedColorType;
        
        #endregion

        #region Initialize
        
        private void OnEnable()
        {
            OnChangedSelect += ChangedSelect;
        }

        private void OnDisable()
        {
            OnChangedSelect -= ChangedSelect;
        }

        #endregion
        
        #region Action Functions
        
        private void ChangedSelect(int selectedIndex)
        {
            ActiveColorType = _selectedColorType[selectedIndex];
        }
        
        #endregion
    }
}