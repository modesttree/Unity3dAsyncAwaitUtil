using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace UnityAsyncAwaitUtil
{
    public class UniRxTests : MonoBehaviour
    {
        Subject<string> _signal = new Subject<string>();

        public void OnGUI()
        {
            if (GUI.Button(new Rect(100, 100, 500, 100), "Test UniRx observable"))
            {
                RunUniRxTestAsync().WrapErrors();
            }

            if (GUI.Button(new Rect(100, 300, 500, 100), "Trigger UniRx observable"))
            {
                _signal.OnNext("zcvd");
            }
        }

        async Task RunUniRxTestAsync()
        {
            Debug.Log("Waiting for UniRx trigger...");
            var result = await _signal;
            Debug.Log("Received UniRx trigger with value: " + result);
        }
    }
}
