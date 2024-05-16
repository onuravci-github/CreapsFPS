using CreapsFPS.Enums;
using UnityEngine;
using UnityEngine.Serialization;

namespace CreapsFPS.ScriptableObjects
{
    [CreateAssetMenu(menuName = "CreapsFPS/ScriptableObject/GunFeature", fileName = "GunFeature")]
    public class GunFeature : ScriptableObject
    {
        public string GunName;
        
        [Range(1f, 200f)] public float AttackPower = 1f;
        [Range(0.05f, 10f)] public float AttackSpeed = 1f;
        [Range(1, 300)] public int AmountAmmo = 90;
        [Range(1, 100)] public int Magazine = 30;
        [Range(10f, 2000f)] public float BulletSpeed = 500f;
        [Range(0.1f, 2000f)] public float BulletRange = 300f;
        [Range(1f, 5f)] public float ReloadTime = 1f;

        public BulletSize BulletSize;
    }
}