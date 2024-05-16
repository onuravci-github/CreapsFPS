using Fusion;
using UnityEngine;

namespace CreapsFPS.Utils
{
    public class NetworkSceneSingleton<T> : NetworkBehaviour where T : NetworkBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<T>();

                    if (_instance == null)
                    {
                        _instance = new GameObject(typeof(T).ToString()).AddComponent<T>();
                    }
                }

                return _instance;
            }
        }

        public static bool IsActive
        {
            get { return _instance != null; }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        protected virtual void OnDestroy()
        {
            if (_instance)
                Destroy(_instance);

            _instance = null;
        }
    }

}