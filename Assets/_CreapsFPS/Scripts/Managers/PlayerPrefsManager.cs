using UnityEngine;

namespace CreapsFPS.Managers
{
    public static class PlayerPrefsManager
    {
        #region Variables
        
        public static string PlayerName
        {
            get
            {
                return GetStringPlayerData("PlayerName");
            }
            set
            {
                SaveStringPlayerData("PlayerName", value);
            }
        }

        
        public static int PlayerAvatarIndex
        {
            get
            {
                return GetIntPlayerData("PlayerAvatarIndex");
            }
            set
            {
                SaveIntPlayerData("PlayerAvatarIndex", value);
            }
        }

        #endregion
        
        #region Saving Datas

        public static void SaveIntPlayerData(string prefName, int data)
        {
            PlayerPrefs.SetInt(prefName, data);
        }

        public static void SaveFloatPlayerData(string prefName, float data)
        {
            PlayerPrefs.SetFloat(prefName, data);
        }

        public static void SaveStringPlayerData(string prefName, string data)
        {
            PlayerPrefs.SetString(prefName, data);
        }

        public static int GetIntPlayerData(string prefName)
        {
            return PlayerPrefs.GetInt(prefName);
        }

        public static float GetFloatPlayerData(string prefName)
        {
            return PlayerPrefs.GetFloat(prefName);
        }

        public static string GetStringPlayerData(string prefName)
        {
            return PlayerPrefs.GetString(prefName);
        }

        public static void DeletePlayerPrefKey(string prefName)
        {
            PlayerPrefs.DeleteKey(prefName);
        }

        #endregion

    }
}