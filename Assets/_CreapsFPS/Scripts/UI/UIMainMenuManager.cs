using System;
using CreapsFPS.Managers;
using CreapsFPS.NetworkSystem;
using CreapsFPS.Utils;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace CreapsFPS.UI
{
    public class UIMainMenuManager : SceneSingleton<UIMainMenuManager>
    {
        #region Variables
        
        [SerializeField] private Image _playerAvatarImage;
        
        [SerializeField] private TMP_InputField _nameInputField;
        [SerializeField] private TMP_InputField _sessionNameInputField;
        [SerializeField] private Button _playButton;
        [SerializeField] private GameMode _gameMode = GameMode.AutoHostOrClient;
        #endregion

        #region Initialize

        private void Start()
        {
            _playerAvatarImage.sprite = GameDataManager.Instance.PlayerAvatarSprites[PlayerPrefsManager.PlayerAvatarIndex];
            if (PlayerPrefsManager.PlayerName == String.Empty)
            {
                PlayerPrefsManager.PlayerName = "Player " + Random.Range(0, 100);
            }
            _nameInputField.text = PlayerPrefsManager.PlayerName;
        }

        #endregion
        
        
        #region Button Functions
        
        public void PlayGame()
        {
            _playButton.interactable = false;
            _sessionNameInputField.interactable = false;
            _nameInputField.interactable = false;
            
            MainApp.Instance.StartGame(_gameMode, _sessionNameInputField.text);
        }
        
        public void PlayerAvatarChange(int index)
        {
            PlayerPrefsManager.PlayerAvatarIndex = index;
            _playerAvatarImage.sprite = GameDataManager.Instance.PlayerAvatarSprites[index];
        }
        
        #endregion


        #region Input Field Functions

        public void PlayerNameChange()
        {
            PlayerPrefsManager.PlayerName = _nameInputField.text;
        }

        #endregion
    }
    
    
}