using CreapsFPS.Utils;
using CreapsFPS.ScriptableObjects;
using System.Collections.Generic;
using CreapsFPS.Enums;
using UnityEngine;

namespace CreapsFPS.Managers
{
    public class GameDataManager : Singleton<GameDataManager>
    {
        #region Variables
        
        [Header("Scriptable Object")]
        public List<GunFeature> GunFeatureTypes;
        
        [Header("Enums")]
        public List<BulletColor> BulletColors;
        public List<BulletSize> BulletSizes;
        
        [Header("Sprites")]
        public Sprite[] BulletColorSprites;
        public Sprite[] BulletSizeSprites;
        public Sprite[] PlayerAvatarSprites;
        
        [Header("Strings")]
        public string[] SizeNameList = new[] {"Small", "Medium", "Large"};
        public string[] ColorNameList = new[] {"Red", "Blue", "Green"};
        
        [Header("Colors")]
        public Color[] GunParticleColors = new[] {Color.red,Color.blue,Color.green};
        #endregion
    }
}