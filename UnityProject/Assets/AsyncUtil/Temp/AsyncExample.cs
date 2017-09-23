using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class AsyncExample : MonoBehaviour
{
    async void Start()
    {
        Debug.Log("Thread: " + Thread.CurrentThread.ManagedThreadId);

        await Task.Delay(TimeSpan.FromSeconds(1.0f)).ConfigureAwait(false);

        Debug.Log("Thread: " + Thread.CurrentThread.ManagedThreadId);
    }
}
