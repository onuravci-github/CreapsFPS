using CreapsFPS.Enums;
using UnityEngine;

namespace CreapsFPS.JoystickTypes.SelectableJoysticks
{

    public class BulletSizeSelect : SelectableJoystick
    {
        #region Variables
        
        public BulletSize ActiveSizeType;
        [SerializeField] private BulletSize[] _selectedSizeType;

        #endregion
         
        #region Action Control
        
        private void OnEnable()
        {
            OnChangedSelect += ChangedSelect;
        }

        private void OnDisable()
        {
            OnChangedSelect -= ChangedSelect;
        }

        private void ChangedSelect(int selectedIndex)
        {
            ActiveSizeType = _selectedSizeType[selectedIndex];
        }
        
        #endregion
    }
}