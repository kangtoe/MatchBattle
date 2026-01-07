using UnityEngine;

namespace MatchBattle
{
    /// <summary>
    /// 제네릭 싱글톤 기반 클래스
    /// MonoBehaviour를 상속받는 싱글톤 패턴 구현
    /// 인스턴스가 없으면 자동 생성
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;

        /// <summary>
        /// 싱글톤 인스턴스 (없으면 자동 생성)
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance of {typeof(T)} is already destroyed. Returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // 씬에서 기존 인스턴스 찾기
                        _instance = FindAnyObjectByType<T>();

                        if (_instance == null)
                        {
                            // 없으면 새로 생성
                            GameObject singleton = new GameObject($"[Singleton] {typeof(T).Name}");
                            _instance = singleton.AddComponent<T>();
                            DontDestroyOnLoad(singleton);

                            Debug.Log($"[Singleton] Created new instance of {typeof(T).Name}");
                        }
                        else
                        {
                            Debug.Log($"[Singleton] Found existing instance of {typeof(T).Name}");
                        }
                    }

                    return _instance;
                }
            }
        }

        /// <summary>
        /// 씬에 이미 인스턴스가 있는지 확인 (생성하지 않음)
        /// </summary>
        public static bool HasInstance => _instance != null;

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[Singleton] Duplicate instance of {typeof(T).Name} detected. Destroying new instance.");
                Destroy(gameObject);
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
