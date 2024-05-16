using CreapsFPS.Utils;
using Cinemachine;
using UnityEngine;

namespace CreapsFPS.Managers
{

    public class GamePlayDataManager : SceneSingleton<GamePlayDataManager>
    {
        #region Variables
        
        public CinemachineVirtualCamera MainVirtualCamera;
        public Camera MainCamera;
        public SpawnPoint[] SpawnPositions;
        #endregion

        
        [System.Serializable]
        public struct SpawnPoint
        {
            public Transform SpawnTransform;
            public bool isSpawned;
        }
    }

}