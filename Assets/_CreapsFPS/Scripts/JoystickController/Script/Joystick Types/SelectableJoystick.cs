using CreapsFPS.Enums;
using CreapsFPS.BaseJoystick;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CreapsFPS.JoystickTypes
{

    public class SelectableJoystick : Joystick
    {
        #region Variables

        private SelectableType _selectableType;
        public Action<int> OnChangedSelect;

        [Header("Visual Type")] [SerializeField]
        private BulletSelectedVisualType _visualType;

        [Header("Images")] [SerializeField] private Transform _selectedImageContainer;
        [SerializeField] private Image[] _selectedImages;

        [Header("Color & Index")] [SerializeField]
        private Color _halfTransparent = new Color(1f, 1f, 1f, 0.35f);

        [SerializeField] private int _selectedIndex = 0;
        private int _oldIndex = -1;

        #endregion

        #region Touch Functions

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            _selectedImageContainer.gameObject.SetActive(true);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);

            switch (_visualType)
            {
                case BulletSelectedVisualType.Circle:
                    break;
                case BulletSelectedVisualType.Horizontal:

                    float horizontalLimit = Mathf.Abs(HorizontalBotLimit) + HorizontalTopLimit;
                    for (int i = 0; i < _selectedImages.Length; i++)
                    {
                        if (Horizontal > HorizontalBotLimit && Horizontal >
                            HorizontalBotLimit + (i * horizontalLimit) / _selectedImages.Length)
                        {
                            _selectedIndex = i;
                        }
                    }

                    if (_oldIndex != _selectedIndex)
                    {
                        ColorChangeImages();
                    }

                    break;
                case BulletSelectedVisualType.Vertical:

                    float verticalLimit = Mathf.Abs(VerticalBotLimit) + VerticalTopLimit;
                    for (int i = 0; i < _selectedImages.Length; i++)
                    {
                        if (Vertical < VerticalTopLimit &&
                            Vertical < VerticalTopLimit - (i * verticalLimit) / _selectedImages.Length)
                        {
                            _selectedIndex = i;
                        }
                    }

                    if (_oldIndex != _selectedIndex)
                    {
                        ColorChangeImages();
                    }

                    break;
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);

            OnChangedSelect?.Invoke(_selectedIndex);

            _selectedImageContainer.gameObject.SetActive(false);
            _oldIndex = -1;
        }


        #endregion

        #region UI Editing

        public void ColorChangeImages()
        {
            for (int i = 0; i < _selectedImages.Length; i++)
            {
                if (i == _selectedIndex)
                {
                    _selectedImages[i].color = Color.white;
                }
                else
                {
                    _selectedImages[i].color = _halfTransparent;
                }
            }
        }

        #endregion
        
    }
}