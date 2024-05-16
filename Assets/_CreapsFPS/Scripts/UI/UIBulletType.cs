using CreapsFPS.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CreapsFPS.UI
{
    public class UIBulletType : MonoBehaviour
    {
        #region Variables

        [Header("Color")]
        [SerializeField] private Image _bulletColorImage;
        [SerializeField] private TextMeshProUGUI _colorText;
        
        [Header("Size")]
        [SerializeField] private TextMeshProUGUI _sizeText;
        [SerializeField] private Image _bulletSizeImage;


        #endregion

        #region Initialize
        
        private void OnEnable()
        {
            UIPlayerControllersManager.Instance.BulletColorSelect.OnChangedSelect += ColorChange;
            UIPlayerControllersManager.Instance.BulletSizeSelect.OnChangedSelect += SizeChange;
        }

        private void OnDisable()
        {
            if (UIPlayerControllersManager.IsActive)
            {
                UIPlayerControllersManager.Instance.BulletColorSelect.OnChangedSelect -= ColorChange;
                UIPlayerControllersManager.Instance.BulletSizeSelect.OnChangedSelect -= SizeChange;
            }
        }

        #endregion

        #region Action Functions

        private void ColorChange(int index)
        {
            _bulletColorImage.sprite = GameDataManager.Instance.BulletColorSprites[index];
            _colorText.text = GameDataManager.Instance.ColorNameList[index];
        }
        
        private void SizeChange(int index)
        {
            _bulletSizeImage.sprite =GameDataManager.Instance.BulletSizeSprites[index];
            _sizeText.text = GameDataManager.Instance.SizeNameList[index];
        }

        #endregion
        
    }
}