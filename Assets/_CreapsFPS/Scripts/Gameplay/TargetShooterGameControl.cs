using System.Collections.Generic;
using CreapsFPS.Enums;
using CreapsFPS.Managers;
using CreapsFPS.NetworkSystem;
using CreapsFPS.NetworkSystem.NetworkObjects;
using CreapsFPS.UI;
using CreapsFPS.Utils;
using Fusion;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

namespace CreapsFPS.Gameplay
{
    public class TargetShooterGameControl : NetworkSceneSingleton<TargetShooterGameControl>
    {
        #region Getter Setter

        public int ActiveBulletColorIndex
        {
            get => _activeBulletColorIndex;
        }

        public int ActiveBulletSizeIndex
        {
            get => _activeBulletSizeIndex;
        }

        #endregion
        
        #region Network Variables

        [Networked] private NetworkBool _isStartingGame { get; set; }
        [Networked] private NetworkBool _isEndingGame { get; set; }
        [Networked] private TickTimer _activeStageCountTimer { get; set; }
        [Networked] private TickTimer _newStageCountTimer { get; set; }

        [Networked] private int _activeBulletColorIndex { get; set; }
        [Networked] private int _activeBulletSizeIndex { get; set; }
        
        #endregion

        #region Variables

        [Header("Game Properties")]
        [SerializeField] private int _activeStageNumb;
        [SerializeField] private int _totalStageNumb = 5;

        [SerializeField] private int _nextBulletColorIndex;
        [SerializeField] private int _nextBulletSizeIndex;
        
        [Header("Stage Time Limits")]
        [SerializeField] private float _activeStageStartTime = 5f;
        [SerializeField] private float _activeStageChangeTime = 15f;
        [SerializeField] private float _nextStageChangeTime = 10f;

        [Header("Target Objects")] 
        [SerializeField] private List<NetworkObject> _targetObjects;
        [SerializeField] private string _addressableTargetsLabelsName = "Targets";
        
        [Header("Spawn Targets")]
        [SerializeField] public Transform SpawnParent;
        [SerializeField] private List<Transform> _spawnPositions;
        [SerializeField] public int SpawnCount = 0;
        [SerializeField] private int _spawnTargetLimit;
        
        #endregion

        #region Initialize

        public override void Spawned()
        {
            if (Object.HasStateAuthority)
            {
                SetTargetObjects();
                MainApp.OnGameStart += OnGameStart;
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (Object.HasStateAuthority)
            {
                MainApp.OnGameStart -= OnGameStart;
            }
        }

        private async void SetTargetObjects()
        {
            var locations = await Addressables.LoadResourceLocationsAsync(_addressableTargetsLabelsName).Task;

            for (int i = 0; i < locations.Count; i++)
            {
                var tempObject = await Addressables.LoadAssetAsync<GameObject>(locations[i]).Task as GameObject;
                _targetObjects.Add(tempObject.GetComponent<NetworkObject>());
            }
            
        } 

        #endregion
        
        #region Updates

        public override void FixedUpdateNetwork()
        {
            if (_isEndingGame)
            {
                return;
            }
            
            if (_activeStageNumb == _totalStageNumb)
            {
                if (Object.HasStateAuthority)
                {
                    RPC_EndingGame();
                }
                Debug.Log("Game Finish");
                return;
            }
            
            if (Object.HasStateAuthority && _isStartingGame)
            {
                if (_activeStageCountTimer.Expired(Runner))
                {
                    SetActiveStageCountTimer(_activeStageChangeTime);
                    SetNewStageCountTimer(_nextStageChangeTime);
                    
                    _activeBulletColorIndex = _nextBulletColorIndex;
                    _activeBulletSizeIndex = _nextBulletSizeIndex;
                    
                    RPC_TargetPropertiesUIUpdate(_activeBulletColorIndex, _activeBulletSizeIndex);

                    GenerateControlledBulletType();
                }
                
                if (_newStageCountTimer.Expired(Runner))
                {
                    _newStageCountTimer = TickTimer.None;
                    RPC_NextPropertiesUIUpdate(_nextBulletColorIndex, _nextBulletSizeIndex);
                }
            }

            if (Object.HasStateAuthority && _isStartingGame && SpawnCount < _spawnTargetLimit)
            {
                Vector3 spawnPos =_spawnPositions[Random.Range(0,_spawnPositions.Count)].position;
                
                NetworkObject createObject = Runner.Spawn(_targetObjects[Random.Range(0, _targetObjects.Count)]);
                createObject.transform.GetComponent<TargetObject>().Initialize(spawnPos);
                SpawnCount++;
            }
        }

        #endregion

        #region Time Control
        
        public void SetActiveStageCountTimer(float activeStageChangeTime)
        {
            _activeStageCountTimer = TickTimer.CreateFromSeconds(Runner, activeStageChangeTime);
        }
        
        public void SetNewStageCountTimer(float nextStageChangeTime)
        {
            _newStageCountTimer = TickTimer.CreateFromSeconds(Runner, nextStageChangeTime);
        }

        #endregion
        
        #region Control Functions

        private void GenerateControlledBulletType()
        {
            _nextBulletColorIndex = Random.Range(0, GameDataManager.Instance.BulletColors.Count);
            _nextBulletSizeIndex = Random.Range(0, GameDataManager.Instance.BulletSizes.Count);

            while (_nextBulletColorIndex == _activeBulletColorIndex && _nextBulletSizeIndex == _activeBulletSizeIndex)
            {
                _nextBulletColorIndex = Random.Range(0, GameDataManager.Instance.BulletColors.Count);
                _nextBulletSizeIndex = Random.Range(0, GameDataManager.Instance.BulletSizes.Count);
            }
        }

        #endregion
        
        #region Action Functions
        
        private void OnGameStart()
        {
            _nextBulletColorIndex = Random.Range(0, GameDataManager.Instance.BulletColors.Count);
            _nextBulletSizeIndex = Random.Range(0, GameDataManager.Instance.BulletSizes.Count);

            _activeBulletColorIndex = _nextBulletColorIndex;
            _activeBulletSizeIndex = _nextBulletSizeIndex;
                    
            SetActiveStageCountTimer(_activeStageStartTime);
            SetNewStageCountTimer(0);
            RPC_StartGame();
        }

        #endregion
        
        #region RPC Methods

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_StartGame()    
        {
            _isStartingGame = true;
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_EndingGame()
        {
            _isEndingGame = true;
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_TargetPropertiesUIUpdate(int bulletColorIndex, int bulletSizeIndex)
        {
            _activeStageNumb++;
            _activeBulletColorIndex = bulletColorIndex;
            _activeBulletSizeIndex = bulletSizeIndex;
            UIGameplayManager.Instance.TargetPropertiesUIUpdate(bulletColorIndex,bulletSizeIndex);
        }
        
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_NextPropertiesUIUpdate(int bulletColorIndex, int bulletSizeIndex)
        {
            _nextBulletColorIndex = bulletColorIndex;
            _nextBulletSizeIndex = bulletSizeIndex;
            UIGameplayManager.Instance.NextTargetPropertiesUIUpdate(bulletColorIndex,bulletSizeIndex);
        }

        #endregion
        
    }
}
