using System;
using System.Threading;
using UnityAsyncAwaitUtil;

public static class UnityScheduler
{
    public static void Run(Action action)
    {
        if (!SyncContextUtil.IsInstalled)
        {
            throw new InvalidOperationException(string.Format("Unity Synchronization Context has not yet been set. Please ensure the {0} class is only used during play-time.", typeof(UnityScheduler).Name));
        }

        if (SynchronizationContext.Current == SyncContextUtil.UnitySynchronizationContext)
        {
            action();
        }
        else
        {
            SyncContextUtil.UnitySynchronizationContext.Post(_ => action(), null);
        }
    }
}
