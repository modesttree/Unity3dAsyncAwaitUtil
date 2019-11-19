using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityAsyncAwaitUtil
{
    public interface ICoroutineRunner
    {
        void StartCoroutine(IEnumerator coroutine);
    }

    public class AsyncCoroutineRunner : MonoBehaviour
    {
        public static ICoroutineRunner Instance
        {
            get
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                    return PlayModeCoroutineRunner.Instance;
                else
                    return EditModeCoroutineRunner.Instance;
#else
                return PlayModeCoroutineRunner.Instance;
#endif
            }
        }
    }
}
