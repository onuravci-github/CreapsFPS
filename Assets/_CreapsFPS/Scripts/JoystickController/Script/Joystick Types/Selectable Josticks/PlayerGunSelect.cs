using CreapsFPS.Managers;
using CreapsFPS.ScriptableObjects;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CreapsFPS.JoystickTypes.SelectableJoysticks
{
    
    public class PlayerGunSelect : SelectableJoystick
    {
        #region Variables
        
        public GunFeature ActiveGunType;
        private List<GunFeature> _selectedGunType = new List<GunFeature>();

        [SerializeField] private TextMeshProUGUI _gunNameText;

        #endregion

        #region Initialize

        private void Awake()
        {
            _selectedGunType = GameDataManager.Instance.GunFeatureTypes;
        }
        
        private void OnEnable()
        {
            OnChangedSelect += ChangedSelect;
        }

        private void OnDisable()
        {
            OnChangedSelect -= ChangedSelect;
        }
        
        #endregion
        
        #region Action Control
        
        private void ChangedSelect(int selectedIndex)
        {
            ActiveGunType = _selectedGunType[selectedIndex];
            _gunNameText.text = ActiveGunType.GunName;
        }
        
        #endregion
    }
}