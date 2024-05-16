using CreapsFPS.UI;
using System;
using System.Collections.Generic;
using CreapsFPS.Utils;
using CreapsFPS._Player;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace CreapsFPS.NetworkSystem
{
    public class MainApp : NetworkSingleton<MainApp>, INetworkRunnerCallbacks
    {
        #region Variables

        public static Action OnGamePlaySceneStart;
        public static Action OnGameStart;
        
        private bool _gameStart;
        
        [Header("Network Runner")]
        [SerializeField] private NetworkRunner _runner;
        [SerializeField] private NetworkPrefabRef _playerPrefab;
        private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
        public Dictionary<PlayerRef, Player> PlayerDictionary = new Dictionary<PlayerRef, Player>();
        
        [Header("Session Settings")] 
        [SerializeField] private int _playerLimit;
        private string _lobbyId;
        private SessionInfo activeSessionInfo;

        private UIPlayerControllersManager _playerControllers;
        #endregion
        
        #region Initialize
        
        private void OnGUI()
        {
            if (!_gameStart && SceneManager.GetActiveScene().name == "Gameplay" && _runner.IsServer)
            {
                if (GUI.Button(new Rect(0, 80, 200, 40), "Start Level"))
                {
                    StartLevel();
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            Application.targetFrameRate = 60;
        }

        #endregion

        #region Start Game Functions

        public async void StartGame(GameMode mode, string privateSessionName)
        {
            // Create the Fusion runner and let it know that we will be providing user input
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
            
            var sessionName = "Standard";
            if (privateSessionName != "")
            {
                sessionName = privateSessionName;
            }
            
            // Start or join (depends on gamemode) a session with a specific name
            await _runner.StartGame(new StartGameArgs()
            {
                GameMode = mode,
                SessionName = sessionName,
                PlayerCount = _playerLimit,
                Scene = SceneManager.GetActiveScene().buildIndex,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            });
            
            _runner.SetActiveScene("Gameplay");
        }

        private void StartLevel()
        {
            if (_runner.IsServer)
            {
                _gameStart = true;
                UIGameplayManager.Instance.RPC_StartLevel();
            }
        }
        

        #endregion

        #region Player

        public void SetPlayer(PlayerRef playerRef, Player player)
        {
            PlayerDictionary.Add(playerRef, player);
        }
        
        public Player GetLocalPlayer(PlayerRef playerRef)
        {
            if (!_runner)
            {
                return null;
            }
            
            foreach (KeyValuePair<PlayerRef, Player> playerDic in MainApp.Instance.PlayerDictionary)
            {
                if (playerDic.Key == _runner.LocalPlayer)
                {
                    return playerDic.Value;
                }
            }
            
            return null;
        }

        public Player GetPlayer(PlayerRef playerRef )
        {
            if (!_runner)
            {
                return null;
            }
            
            PlayerDictionary.TryGetValue(playerRef, out Player player);
            return player;
        }

        #endregion
        
        #region Fusion General NetworkCallBacks
        

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer)
            {
                // Create a unique position for the player
                Vector3 spawnPosition = new Vector3(Random.Range(-15, 15), Random.Range(20, 25), Random.Range(-15, 15));
                NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.Euler(0,90,0), player);
                networkPlayerObject.transform.parent = runner.transform;
                _spawnedCharacters.Add(player, networkPlayerObject);

                networkPlayerObject.GetComponent<Player>().PlayerNumb = _spawnedCharacters.Count - 1;
            }
        }
        
        public void SetPlayerParent(Transform player)
        {
            player.transform.SetParent(_runner.transform);
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            // Find and remove the players avatar
            if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
            {
                runner.Despawn(networkObject);
                _spawnedCharacters.Remove(player);
            }
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            if (UIPlayerControllersManager.IsActive && _playerControllers == null)
            {
                _playerControllers = UIPlayerControllersManager.Instance;
            }
            else if(_playerControllers != null)
            {
                NetworkInputData inputData = _playerControllers.GetNetworkInputs();
                input.Set(inputData);
            }
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            if (_runner != null && _runner.gameObject)
                Destroy(_runner.gameObject);

            _runner = null;

            SceneManager.LoadSceneAsync("MainMenu");
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
        }

        public void OnDisconnectedFromServer(NetworkRunner runner)
        {
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token)
        {
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
        {
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            if (_runner.IsServer && SceneManager.GetActiveScene().name == "Gameplay")
            {
                OnGamePlaySceneStart?.Invoke();
            }
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
            
        }
        
        #endregion
    }
}