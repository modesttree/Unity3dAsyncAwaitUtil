using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityAsyncAwaitUtil
{
    public class PlayModeCoroutineRunner : MonoBehaviour, ICoroutineRunner
    {
        static PlayModeCoroutineRunner _instance;

        public static PlayModeCoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("PlayModeCoroutineRunner")
                        .AddComponent<PlayModeCoroutineRunner>();
                }

                return _instance;
            }
        }

        public new void StartCoroutine(IEnumerator coroutine)
        {
            base.StartCoroutine(coroutine);
        }

        void Awake()
        {
            // Don't show in scene hierarchy
            gameObject.hideFlags = HideFlags.HideAndDontSave;

            DontDestroyOnLoad(gameObject);
        }
    }
}
