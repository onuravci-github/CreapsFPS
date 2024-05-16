using CreapsFPS.BaseJoystick;
using System;
using System.Collections.Generic;
using CreapsFPS.Enums;
using CreapsFPS.Managers;
using CreapsFPS.ScriptableObjects;
using MEC;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CreapsFPS.JoystickTypes
{

    public class AttackJoystick : Joystick
    {
        #region Variables

        public Action JoystickPointerDown;
        public Action JoystickPointerUp;

        [SerializeField] private List<GunFeature> _gunFeatures;
        [SerializeField] private List<int> _activeGunFeaturesIndex;
        [SerializeField] private List<int> _gunMagazineLimits;
        [SerializeField] private int _activeGunFeature;
        
        private float _startTime = -100;
        [SerializeField] private bool _isDown;
        [SerializeField] private bool _isReload;
        [SerializeField] private TextMeshProUGUI _ammoText;
        #endregion
        
        #region Initialize
        
        private void Awake()
        {
            SetGunFeature(0);
        }

        #endregion

        #region Touch Functions
        
        public override void OnPointerDown(PointerEventData eventData)
        {
            if (_gunFeatures[_activeGunFeature].Magazine == 0)
            {
                ReloadAmmo();
                return;
            }

            if (_isReload)
            {
                return;
            }
            
            if (Time.time - _startTime < _gunFeatures[_activeGunFeature].AttackSpeed)
            {
                return;
            }
            
            _isDown = true;
            _startTime = Time.time;
            
            _background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
            base.OnPointerDown(eventData);
            JoystickPointerDown?.Invoke();

            _gunFeatures[_activeGunFeature].Magazine--;
            UpdateAmmoText();
        }

        private void Update()
        {
            if (!_isDown)
            {
                return;
            }
            
            if (_gunFeatures[_activeGunFeature].Magazine == 0)
            {
                ReloadAmmo();
                return;
            } 
            
            if (_isReload)
            {
                return;
            }
            
            if (Time.time - _startTime <= _gunFeatures[_activeGunFeature].AttackSpeed)
            {
                return;
            }
            
            _startTime = Time.time;
            
            JoystickPointerDown?.Invoke();
            _gunFeatures[_activeGunFeature].Magazine--;
            UpdateAmmoText();
            
        }
        
        public override void OnPointerUp(PointerEventData eventData)
        {
            if (!_isDown)
            {
                return;
            }
            _isDown = false;
            
            base.OnPointerUp(eventData);
        }
        
        #endregion

        #region GunFeatureUpdate

        public void SetGunFeature(int index)
        {
            for (int i = 0; i < _activeGunFeaturesIndex.Count; i++)
            {
                if (index == _activeGunFeaturesIndex[i])
                {
                    _activeGunFeature = index;
                    UpdateAmmoText();
                    return;
                }
            }
            
            _gunFeatures.Add(Instantiate(GameDataManager.Instance.GunFeatureTypes[index]));
            _activeGunFeaturesIndex.Add(index);
            _gunMagazineLimits.Add(_gunFeatures[index].Magazine);
            _activeGunFeature = index;
            UpdateAmmoText();
        }
        
        public void ReloadAmmo()
        {
            if (_isReload)
            {
                return;   
            }
            
            _isReload = true;
            Timing.RunCoroutine(C_ReloadAmmo());
        }
        
        private IEnumerator<float> C_ReloadAmmo()
        {
            yield return Timing.WaitForSeconds(_gunFeatures[_activeGunFeature].ReloadTime);

            if (_gunFeatures[_activeGunFeature].AmountAmmo >= _gunMagazineLimits[_activeGunFeature])
            {
                if (_gunFeatures[_activeGunFeature].Magazine > 0)
                {
                    _gunFeatures[_activeGunFeature].AmountAmmo -= _gunMagazineLimits[_activeGunFeature] - _gunFeatures[_activeGunFeature].Magazine;
                    _gunFeatures[_activeGunFeature].Magazine = _gunMagazineLimits[_activeGunFeature];
                }
                else
                {
                    _gunFeatures[_activeGunFeature].AmountAmmo -= _gunMagazineLimits[_activeGunFeature];
                    _gunFeatures[_activeGunFeature].Magazine = _gunMagazineLimits[_activeGunFeature];
                }
            }
            else if(_gunFeatures[_activeGunFeature].AmountAmmo != 0)
            {
                _gunFeatures[_activeGunFeature].Magazine = _gunFeatures[_activeGunFeature].AmountAmmo;
                _gunFeatures[_activeGunFeature].AmountAmmo = 0;
            }
            
            _isReload = false;
            UpdateAmmoText();
        }

        public void UpdateAmmoText()
        {
            _ammoText.text = _gunFeatures[_activeGunFeature].Magazine.ToString() + "/" + _gunFeatures[_activeGunFeature].AmountAmmo;
        }
        #endregion
    }
}