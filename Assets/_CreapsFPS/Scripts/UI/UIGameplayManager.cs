using System.Collections.Generic;
using CreapsFPS._Player;
using CreapsFPS.Managers;
using CreapsFPS.NetworkSystem;
using CreapsFPS.Utils;
using Fusion;
using MEC;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace CreapsFPS.UI
{
    public class UIGameplayManager : NetworkSceneSingleton<UIGameplayManager>
    {
        #region Network Variables
        
        [Networked] private TickTimer _gameStartCountdown { get; set; }
        [Networked] private TickTimer _refreshScoreBoard { get; set; }
        
        [Networked] private NetworkBool _isStartingGameplayScene { get; set; }
        [Networked] private int _remainingTime{ get; set; }
        
        #endregion
        
        #region Variables
        
        [Header("Player")]
        [SerializeField] private TextMeshProUGUI _localPlayerName;
        
        [Header("Containers & Canvas")]
        [SerializeField] private GameObject _generalCanvasContainer;
        [SerializeField] private GameObject _movementJoystickCanvas;
        [SerializeField] private GameObject _buttonJoystickCanvas;

        [Header("Target Shoot Game Active Bullets")]
        [SerializeField] private GameObject _targetShootActiveBulletContainer;
        [SerializeField] private Image _targetShootActiveBulletColor;
        [SerializeField] private TextMeshProUGUI _targetShootActiveBulletColorText;
        [SerializeField] private Image _targetShootActiveBulletSize;
        [SerializeField] private TextMeshProUGUI _targetShootActiveBulletSizeText;
        [SerializeField] private GameObject _targetShootNextBulletContainer;
        [SerializeField] private Image _targetShootNextBulletColor;
        [SerializeField] private TextMeshProUGUI _targetShootNextBulletColorText;
        [SerializeField] private Image _targetShootNextBulletSize;
        [SerializeField] private TextMeshProUGUI _targetShootNextBulletSizeText;
        
        [Header("Game Loop Message Texts")]
        [SerializeField] private GameObject _loadingContainer;
        [SerializeField] private TextMeshProUGUI _startingGameText;
        [SerializeField] private TextMeshProUGUI _remainingTimeText;
        [SerializeField] private GameObject _endingGameContainer;
        [SerializeField] private TextMeshProUGUI _endingGameText;
        
        [Header("Players Score")]
        [SerializeField] private GameObject _playerScoreContainer;
        [SerializeField] private List<UIPlayerScore> _playerScore;
        [SerializeField] private TextMeshProUGUI _localPlayerScoreText;

        [Header("Addressable Referance")] 
        [SerializeField] private AssetReference _playerScorePrefab;

        private bool gameStart = false;
        
        #endregion

        #region Initialize

        public override void Spawned()
        {
            _remainingTime = 3;
            SetTickerRemainingTime();
            SetRefreshScoreBoardTime();
            
            MainApp.OnGameStart += OnGameStart;

            if (Object.HasStateAuthority)
            {
                _startingGameText.text = "Creaps\nLet's Start Game";
            }
            else
            {
                _startingGameText.text = "Waiting Host";
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            MainApp.OnGameStart -= OnGameStart;
        }

        #endregion

        #region Updates

        public override void FixedUpdateNetwork()
        {
            if (_isStartingGameplayScene)
            {
                if (_gameStartCountdown.Expired(Runner))
                {
                    SetTickerRemainingTime();
                    _remainingTime--;
                    RemainingTimeUpdated();
                    if (_remainingTime == 0)
                    {
                        if (Object.HasStateAuthority)
                        {
                            RPC_StartGame();
                        }
                    }
                }
            }

            if (gameStart)
            {
                if (_refreshScoreBoard.Expired(Runner))
                {
                    SetRefreshScoreBoardTime();
                    if (_playerScore.Count < Runner.SessionInfo.PlayerCount)
                    {
                        foreach (var player in FindObjectsOfType<Player>())
                        {
                            bool isAlreadyAddedPlayer = false;
                            foreach (KeyValuePair<PlayerRef, Player> playerDic in MainApp.Instance.PlayerDictionary)
                            {
                                if (player.Object.InputAuthority == playerDic.Value.Object.InputAuthority)
                                {
                                    isAlreadyAddedPlayer = true;
                                }
                            }
                            if (!isAlreadyAddedPlayer)
                            {
                                MainApp.Instance.SetPlayer(player.Object.InputAuthority,player);
                            }
                        }
                    }
                }
            }
        }
        
        #endregion

        #region UI Updates

        public void TargetPropertiesUIUpdate(int bulletColorIndex, int bulletSizeIndex)
        {
            _targetShootActiveBulletContainer.SetActive(true);
            
            _targetShootActiveBulletColor.sprite = GameDataManager.Instance.BulletColorSprites[bulletColorIndex];
            _targetShootActiveBulletColorText.text = GameDataManager.Instance.ColorNameList[bulletColorIndex];
            
            _targetShootActiveBulletSize.sprite = GameDataManager.Instance.BulletSizeSprites[bulletSizeIndex];
            _targetShootActiveBulletSizeText.text = GameDataManager.Instance.SizeNameList[bulletSizeIndex];
        }
        
        public void NextTargetPropertiesUIUpdate(int bulletColorIndex, int bulletSizeIndex)
        {
            _targetShootNextBulletContainer.SetActive(true);
            
            _targetShootNextBulletColor.sprite = GameDataManager.Instance.BulletColorSprites[bulletColorIndex];
            _targetShootNextBulletColorText.text = GameDataManager.Instance.ColorNameList[bulletColorIndex];
            
            _targetShootNextBulletSize.sprite = GameDataManager.Instance.BulletSizeSprites[bulletSizeIndex];
            _targetShootNextBulletSizeText.text = GameDataManager.Instance.SizeNameList[bulletSizeIndex];

            Timing.RunCoroutine(NextTargetPropertiesClose());
        }

        private IEnumerator<float> NextTargetPropertiesClose()
        {
            yield return Timing.WaitForSeconds(4.5f);
            if (_targetShootNextBulletContainer != null)
            {
                _targetShootNextBulletContainer.SetActive(false);
            }
        }

        private async void ScoreBoardCreate()
        {
            if (_playerScore.Count == MainApp.Instance.PlayerDictionary.Count)
            {
                return;
            }
            
            for (int i = _playerScore.Count; i < MainApp.Instance.PlayerDictionary.Count; i++)
            {
                var createPlayerScore = await Addressables.InstantiateAsync(_playerScorePrefab,_playerScoreContainer.transform).Task;
                _playerScore.Add(createPlayerScore.GetComponent<UIPlayerScore>());
            }

            int count = 0;
            foreach (KeyValuePair<PlayerRef, Player> playerDic in MainApp.Instance.PlayerDictionary)
            {
                _playerScore[count].Initialize(playerDic.Value.PlayerName.ToString(),playerDic.Value.PlayerNumb,playerDic.Value.PlayerAvatarIndex);
                count++;
            }
            
        }
        
        public void ScoreBoardUpdate()
        {
            _localPlayerScoreText.text = "Score " + MainApp.Instance.GetLocalPlayer(Runner.LocalPlayer).PlayerScore;

            int count = 0;
            foreach (KeyValuePair<PlayerRef, Player> playerDic in MainApp.Instance.PlayerDictionary)
            {
                _playerScore[count].SetScore(playerDic.Value.PlayerScore);
                count++;
            }
        }
        
        
        #endregion

        #region RPC Methods

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_StartLevel()
        {
            _isStartingGameplayScene = true;
            _startingGameText.text = "Game Starting ...";
            _remainingTimeText.text  = _remainingTime.ToString();
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_StartGame()
        {
            _isStartingGameplayScene = false;
            MainApp.OnGameStart?.Invoke();
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_ScoreBoardUpdate()
        {
            ScoreBoardCreate();
        }
        #endregion
        
        #region TickTimer Setter
        
        public void SetTickerRemainingTime()
        {
            _gameStartCountdown = TickTimer.CreateFromSeconds(Runner, 1f);
        }
        
        public void SetRefreshScoreBoardTime()
        {
            _refreshScoreBoard = TickTimer.CreateFromSeconds(Runner, 0.25f);
        }

        private void RemainingTimeUpdated()
        { 
            _remainingTimeText.text = _remainingTime.ToString();
        }
        #endregion
        
        #region Action Functions
        
        public void OnGameStart()
        {
            gameStart = true;
            
            _loadingContainer.SetActive(false);
            _movementJoystickCanvas.SetActive(true);
            _buttonJoystickCanvas.SetActive(true);
            _generalCanvasContainer.SetActive(true);

            _localPlayerName.text = MainApp.Instance.GetLocalPlayer(Runner.LocalPlayer).PlayerName.ToString();
            ScoreBoardCreate();
        }

        #endregion
        
    }
}