using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.CustomUtils
{
    public abstract class Singleton<T> : MonoBehaviour where T : notnull, Singleton<T>
    {
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject();
                    go.name = typeof(T).Name;
                    _instance = go.AddComponent<T>();
                }
                return _instance;
            }
            private set => _instance = value;
        }
        [SerializeField, ReadOnly] private static T _instance = null;

        private bool _destroying = false;


        protected void ForceSetInstance()
        {
            _instance = this as T;
        }

        protected void SetInstance()
        {
            if (_instance == null)
            {
                ForceSetInstance();
            }

            if (_instance != null && _instance != this && _instance.gameObject.activeInHierarchy)
            {
                Destroy(gameObject);
            }
        }

        protected static void ForceUnsetInstance()
        {
            if (_instance != null && !_instance._destroying)
            {
                Destroy(_instance.gameObject);
            }
            _instance = null;
        }

        protected void UnsetInstance()
        {
            if (_instance == this)
            {
                ForceUnsetInstance();
            }
        }



        protected virtual void Awake()
        {
            SetInstance();
        }

        protected virtual void OnDestroy()
        {
            _destroying = true;
            if (_instance == this)
            {
                UnsetInstance();
            }
        }
	}
	public abstract class GlobalSingleton<T> : Singleton<T> where T : GlobalSingleton<T>
	{
		protected override void Awake()
		{
			base.Awake();
			DontDestroyOnLoad(gameObject);
		}
	}
}