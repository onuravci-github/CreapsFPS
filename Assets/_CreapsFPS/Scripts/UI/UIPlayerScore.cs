using System;
using System.Collections;
using System.Collections.Generic;
using CreapsFPS.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CreapsFPS.UI
{
    public class UIPlayerScore : MonoBehaviour
    {
        #region Variables

        [SerializeField] private int _playerNumb;
        [SerializeField] private Image _playerAvatar;
        
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _scoreText;

        #endregion
        
        #region Initialize

        public void Initialize(string playerName,int playerNumb,int playerAvatarIndex)
        {
            _nameText.text = playerName;
            _playerNumb = playerNumb;
            _playerAvatar.sprite = GameDataManager.Instance.PlayerAvatarSprites[playerAvatarIndex];
        }

        #endregion

        public void SetScore(int totalScore)
        {
            _scoreText.text = totalScore.ToString();
        }
        
    }
}
