using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

public class WaitForBackgroundThread
{
    private readonly bool canFallbackToMainThread;
    private readonly bool platformSupportsMultithread;

    public WaitForBackgroundThread() : this(false) { }

    public WaitForBackgroundThread(bool canFallbackToMainThread)
    {
        this.canFallbackToMainThread = canFallbackToMainThread;
        this.platformSupportsMultithread = Application.platform != RuntimePlatform.WebGLPlayer;
    }

    public ConfiguredTaskAwaitable.ConfiguredTaskAwaiter GetAwaiter()
    {
        if (!platformSupportsMultithread && !canFallbackToMainThread)
        {
            throw new InvalidOperationException($"{Application.platform} does not support background threads.");
        }

        return Empty().ConfigureAwait(!platformSupportsMultithread).GetAwaiter();
    }

    private static async Task Empty()
    {
        // Task.Run does not run on WebGL, so we use this async method instead.
        await Task.Yield();
    }
}
