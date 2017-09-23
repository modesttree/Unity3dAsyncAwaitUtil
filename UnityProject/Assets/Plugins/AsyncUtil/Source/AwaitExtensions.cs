using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

public static class AwaitExtensions
{
    public static TaskAwaiter<int> GetAwaiter(this Process process)
    {
        var tcs = new TaskCompletionSource<int>();
        process.EnableRaisingEvents = true;

        process.Exited += (s, e) => tcs.TrySetResult(process.ExitCode);

        if (process.HasExited)
        {
            tcs.TrySetResult(process.ExitCode);
        }

        return tcs.Task.GetAwaiter();
    }

    // Any time you call an async method from sync code, you should call this method
    // as well, otherwise any exceptions that occur inside the async code will not be
    // received by Unity.  (Unity only observes errors for async methods that have return type void)
    public static async void WrapErrors(this Task task)
    {
        await task;
    }
}
