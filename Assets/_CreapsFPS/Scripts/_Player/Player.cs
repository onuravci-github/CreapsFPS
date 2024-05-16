using System;
using System.Collections.Generic;
using CreapsFPS.Managers;
using CreapsFPS.NetworkSystem;
using CreapsFPS.UI;
using Fusion;
using MEC;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace CreapsFPS._Player
{
    public class Player : NetworkBehaviour
    {
        #region Network Variables
        
        [Networked] public NetworkString<_32> PlayerName { get; set; }
        [Networked] public int PlayerNumb { get; set; }

        [Networked(OnChanged = nameof(ScoreBoardUpdate))] public int PlayerScore{ get; set; }
        
        [Networked] public int PlayerAvatarIndex { get; set; }
        [Networked] public int BulletSizeIndex { get; set; }
        [Networked] public int BulletColorIndex { get; set; }
        [Networked] public NetworkBool GameStart { get; set; }
        
        #endregion
        
        #region Variables

        public static Action<PlayerRef,int> GainScore;
        
        #endregion

        #region Initialize

        public override void Spawned()
        {
            if (Object.HasStateAuthority)
            {
                GainScore += RPC_PlayerAddScore;
                MainApp.OnGameStart += SetSpawnPosition;
            }
            MainApp.OnGamePlaySceneStart += SetBulletTypeActions;
            
            MainApp.Instance.SetPlayer(Object.InputAuthority, this);

            if (MainApp.Instance.GetLocalPlayer(Runner.LocalPlayer) == this)
            {
                RPC_SetPlayerPrefs(MainApp.Instance.GetLocalPlayer(Object.InputAuthority),PlayerPrefsManager.PlayerName,PlayerPrefsManager.PlayerAvatarIndex);
            }
            
            if (!Object.HasStateAuthority && SceneManager.GetActiveScene().name == "Gameplay")
            {
                MainApp.OnGamePlaySceneStart?.Invoke();
                if (Runner.LocalPlayer == Object.InputAuthority)
                {
                    Timing.RunCoroutine(AlreadyGameStarted());
                }
            }
            
            MainApp.Instance.SetPlayerParent(this.transform);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            GainScore -= RPC_PlayerAddScore;
            MainApp.OnGameStart -= SetSpawnPosition;
        }

        #endregion
        
        #region AlreadyGameStarted Control
        
        IEnumerator<float> AlreadyGameStarted()
        {
            yield return Timing.WaitForSeconds(0.25f);
            if (IsGameAlreadyStart())
            {
                UIGameplayManager.Instance.OnGameStart();
                UIGameplayManager.Instance.RPC_ScoreBoardUpdate();
                MainApp.Instance.GetLocalPlayer(Object.InputAuthority).GetComponent<PlayerCameraControl>().Initialize();
            }
        }
        private bool IsGameAlreadyStart()
        {
            foreach (KeyValuePair<PlayerRef, Player> playerDic in MainApp.Instance.PlayerDictionary)
            {
                if (playerDic.Value.GameStart)
                {
                    return true;
                }
            }
            return false;
        }


        #endregion
        
        #region RPC Methods

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_SetPlayerPrefs(Player player,string name, int avatarIndex)
        { 
            player.PlayerName = name;
            player.PlayerAvatarIndex = avatarIndex;
        }
        
        [Rpc(RpcSources.All, RpcTargets.InputAuthority)]
        private void RPC_PlayerAddScore(PlayerRef playerRef,int score)
        {
            if (Object.HasStateAuthority)
            {
                MainApp.Instance.GetPlayer(playerRef).PlayerScore += score;
            }
        }
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        private void RPC_SetPlayerBulletColor(int bulletColorIndex)
        {
            BulletColorIndex = bulletColorIndex;
        }
        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        private void RPC_SetPlayerBulletSize(int bulletSizeIndex)
        {
            BulletSizeIndex = bulletSizeIndex;
        }
        
        #endregion

        #region Action Functions

        private static void ScoreBoardUpdate(Changed<Player> playerInfo)
        {
            UIGameplayManager.Instance.ScoreBoardUpdate();
        }
        
        private void SetBulletTypeActions()
        {
            if (Runner.LocalPlayer == Object.InputAuthority)
            {
                UIPlayerControllersManager.Instance.BulletColorSelect.OnChangedSelect += RPC_SetPlayerBulletColor;
                UIPlayerControllersManager.Instance.BulletSizeSelect.OnChangedSelect += RPC_SetPlayerBulletSize;
            }
        }
        
        private void SetSpawnPosition()
        {
            GameStart = true;
            int randomIndex = Random.Range(0, GamePlayDataManager.Instance.SpawnPositions.Length);
            
            while (GamePlayDataManager.Instance.SpawnPositions[randomIndex].isSpawned)
            {
                randomIndex = Random.Range(0, GamePlayDataManager.Instance.SpawnPositions.Length);
            }
            
            Vector3 spawnPos = GamePlayDataManager.Instance.SpawnPositions[randomIndex].SpawnTransform.position;
            GamePlayDataManager.Instance.SpawnPositions[randomIndex].isSpawned = true;
            transform.position = spawnPos;
        }

        #endregion
    }
}