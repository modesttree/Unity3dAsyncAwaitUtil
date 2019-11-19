#if UNITY_EDITOR
using System;
using Debug = UnityEngine.Debug;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityAsyncAwaitUtil
{
    /// <summary>
    /// This class runs coroutines in Edit Mode.
    ///
    /// In Edit Mode, Unity does *not* automatically automatically advance
    /// coroutines in each frame by calling `MoveNext`, like it does in Play
    /// Mode.  Instead, we keep track of the active coroutines ourselves and
    /// call `MoveNext` on them in response to the `EditorApplication.update`
    /// event.
    /// </summary>
    public class EditModeCoroutineRunner : ICoroutineRunner
    {
        private static EditModeCoroutineRunner _instance;

        /// <summary>
        /// Return the singleton instance of EditModeCoroutineRunner.
        /// </summary>
        public static EditModeCoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new EditModeCoroutineRunner();
                return _instance;
            }
        }

        /// <summary>
        /// Timer class used to emulate `WaitForSeconds` and
        /// `WaitForSecondsRealtime` in Edit Mode.
        /// </summary>
        private class WaitTimer
        {
            private readonly Stopwatch _stopwatch;
            private readonly float _secondsToWait;

            public WaitTimer(float secondsToWait)
            {
                _secondsToWait = secondsToWait;
                _stopwatch = new Stopwatch();
                _stopwatch.Start();
            }

            public bool IsDone
            {
                get
                {
                    _stopwatch.Stop();
                    if (_stopwatch.Elapsed.Seconds >= _secondsToWait)
                        return true;
                    _stopwatch.Start();
                    return false;
                }
            }
        }

        /// <summary>
        /// The list of currently running coroutines.
        /// </summary>
        private List<IEnumerator> _coroutines;

        /// <summary>
        /// A list of WaitTimer objects with a one-to-one
        /// correspondence to the elements of `_coroutines`.
        /// A coroutine's WaitTimer entry will be null
        /// unless that coroutine is currently waiting on a
        /// `WaitForSeconds` or `WaitForSecondsRealtime`
        /// instruction.
        /// </summary>
        private List<WaitTimer> _waitTimers;

        private EditModeCoroutineRunner()
        {
            _coroutines = new List<IEnumerator>();
            _waitTimers = new List<WaitTimer>();

            EditorApplication.update += Update;
        }

        public void StartCoroutine(IEnumerator coroutine)
        {
            // Make first call to `coroutine.MoveNext`, so that
            // we can safely use `coroutine.Current` in `Update`.
            if (coroutine.MoveNext())
            {
                _coroutines.Add(coroutine);
                _waitTimers.Add(null);
            }
        }

        /// <summary>
        /// Return the time in seconds of a `WaitForSecondsRealtime` object.
        /// </summary>
        private float GetSeconds(WaitForSecondsRealtime waitObj)
        {
            // In Unity 2018.3, the `waitTime` field was changed from protected
            // to public and the value was changed from
            // `Time.realtimeSinceStartup + seconds` to `seconds`. The best way
            // to understand the code below is to clone the `UnityCsReference`
            // repo and view the source file history with
            // `git log --follow --patch -- Runtimetime/Export/Scripting/WaitForSecondsRealtime.cs`

#if UNITY_2018_3_OR_NEWER
            return waitObj.waitTime;
#else
            FieldInfo field = waitObj.GetType()
                .GetField("waitTime", BindingFlags.Instance
                    | BindingFlags.NonPublic);

            return (float)field.GetValue(waitObj)
                - Time.realtimeSinceStartup;
#endif
        }

        /// <summary>
        /// Return the time in seconds of a `WaitForSeconds` object.
        /// </summary>
        private float GetSeconds(WaitForSeconds waitObj)
        {
            FieldInfo field = waitObj
                .GetType().GetField("m_Seconds",
                    BindingFlags.Instance
                    | BindingFlags.NonPublic);

            return (float)field.GetValue(waitObj);
        }

        /// <summary>
        /// Return the time in seconds for a `WaitForSeconds`
        /// or `WaitForSecondsRealtime` object.
        /// </summary>
        private float GetSecondsFromWaitObject(object waitObj)
        {
            WaitForSeconds waitForSeconds = waitObj as WaitForSeconds;
            if (waitForSeconds != null)
                return GetSeconds(waitForSeconds);

            WaitForSecondsRealtime waitForSecondsRealtime
                = waitObj as WaitForSecondsRealtime;
            if (waitForSecondsRealtime != null)
                return GetSeconds(waitForSecondsRealtime);

            Debug.LogError("GetSecondsFromWaitObject returning 0f: "
                + "unrecognized argument type");
            return 0.0f;
        }

        private void Update()
        {
            // Loop in reverse order so that we can remove
            // completed coroutines/timers while iterating.

            for (int i = _coroutines.Count - 1; i >= 0; i--)
            {
                // object returned by `yield return`

                var yieldInstruction = _coroutines[i].Current;

                // Emulate `WaitForSeconds` and `WaitForSecondsRealtime`
                // behaviour in Edit Mode

                if (yieldInstruction is WaitForSeconds
                    || yieldInstruction is WaitForSecondsRealtime)
                {
                    if (_waitTimers[i] == null)
                    {
                        float secondsToWait
                            = GetSecondsFromWaitObject(yieldInstruction);
                        _waitTimers[i] = new WaitTimer(secondsToWait);
                    }

                    if (_waitTimers[i].IsDone
                        && !_coroutines[i].MoveNext())
                    {
                        _coroutines.RemoveAt(i);
                        _waitTimers.RemoveAt(i);
                    }

                    continue;
                }

                // `CustomYieldInstruction`s provide an
                // `IEnumerator` interface.
                //  
                // Examples of `CustomYieldInstruction`:
                // `WaitUntil`, `WaitWhile`, `WWW`.

                IEnumerator enumerator = yieldInstruction as IEnumerator;
                if (enumerator != null)
                {
                    if (!enumerator.MoveNext() && !_coroutines[i].MoveNext())
                    {
                        _coroutines.RemoveAt(i);
                        _waitTimers.RemoveAt(i);
                    }

                    continue;
                }

                // Examples of `AsyncOperation`:
                // `ResourceRequest`, `AssetBundleRequest`
                // and `AssetBundleCreateRequest`.

                AsyncOperation asyncOperation
                    = yieldInstruction as AsyncOperation;
                if (asyncOperation != null)
                {
                    if (asyncOperation.isDone && !_coroutines[i].MoveNext())
                    {
                        _coroutines.RemoveAt(i);
                        _waitTimers.RemoveAt(i);
                    }

                    continue;
                }

                // Default case: `yield return` is `null` or is a type with no
                // special Unity-defined behaviour.

                if (!_coroutines[i].MoveNext())
                {
                    _coroutines.RemoveAt(i);
                    _waitTimers.RemoveAt(i);
                }
            }
        }
    }
}

#endif
