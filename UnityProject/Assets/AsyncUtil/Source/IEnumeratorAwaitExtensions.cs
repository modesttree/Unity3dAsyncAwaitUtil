using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityAsyncAwaitUtil;

// We could just add a generic GetAwaiter to YieldInstruction and CustomYieldInstruction
// but instead we add specific methods to each derived class to allow for return values
// that make the most sense for the specific instruction type
public static class IEnumeratorAwaitExtensions
{
    public static TaskAwaiter<AsyncOperation> GetAwaiter(this AsyncOperation instruction)
    {
        return GetAwaiterReturnSelf(instruction);
    }

    public static TaskAwaiter<object> GetAwaiter(this WaitForSeconds instruction)
    {
        return GetAwaiterReturnNull(instruction);
    }

    public static TaskAwaiter<object> GetAwaiter(this WaitForUpdate instruction)
    {
        return GetAwaiterReturnNull(instruction);
    }

    public static TaskAwaiter<object> GetAwaiter(this WaitForEndOfFrame instruction)
    {
        return GetAwaiterReturnNull(instruction);
    }

    public static TaskAwaiter<object> GetAwaiter(this WaitForFixedUpdate instruction)
    {
        return GetAwaiterReturnNull(instruction);
    }

    public static TaskAwaiter<object> GetAwaiter(this WaitForSecondsRealtime instruction)
    {
        return GetAwaiterReturnNull(instruction);
    }

    public static TaskAwaiter<object> GetAwaiter(this WaitUntil instruction)
    {
        return GetAwaiterReturnNull(instruction);
    }

    public static TaskAwaiter<object> GetAwaiter(this WaitWhile instruction)
    {
        return GetAwaiterReturnNull(instruction);
    }

    public static TaskAwaiter<UnityEngine.Object> GetAwaiter(this ResourceRequest instruction)
    {
        var tcs = new TaskCompletionSource<UnityEngine.Object>();
        AsyncCoroutineRunner.Instance.StartCoroutine(
            InstructionWrappers.ResourceRequest(tcs, instruction));
        return tcs.Task.GetAwaiter();
    }

    public static TaskAwaiter<UnityEngine.iOS.OnDemandResourcesRequest> GetAwaiter(this UnityEngine.iOS.OnDemandResourcesRequest instruction)
    {
        return GetAwaiterReturnSelf(instruction);
    }

    // Return itself so you can do things like (await new WWW(url)).bytes
    public static TaskAwaiter<WWW> GetAwaiter(this WWW instruction)
    {
        return GetAwaiterReturnSelf(instruction);
    }

    public static TaskAwaiter<AssetBundle> GetAwaiter(this AssetBundleCreateRequest instruction)
    {
        var tcs = new TaskCompletionSource<AssetBundle>();
        AsyncCoroutineRunner.Instance.StartCoroutine(
            InstructionWrappers.AssetBundleCreateRequest(tcs, instruction));
        return tcs.Task.GetAwaiter();
    }

    public static TaskAwaiter<UnityEngine.Object> GetAwaiter(this AssetBundleRequest instruction)
    {
        var tcs = new TaskCompletionSource<UnityEngine.Object>();
        AsyncCoroutineRunner.Instance.StartCoroutine(
            InstructionWrappers.AssetBundleRequest(tcs, instruction));
        return tcs.Task.GetAwaiter();
    }

    public static TaskAwaiter<object> GetAwaiter(this IEnumerator coroutine)
    {
        var tcs = new TaskCompletionSource<object>();
        var wrapper = new CoroutineWrapper(coroutine, tcs);
        AsyncCoroutineRunner.Instance.StartCoroutine(wrapper.Run());
        return tcs.Task.GetAwaiter();
    }

    // We'd prefer to return TaskAwaiter here instead since there is never a return
    // value for yield instructions, but I'm not sure how to get that working here
    // since TaskAwaiter<> does not inherit from TaskAwaiter and there isn't a
    // non generic version of TaskCompletionSource<>
    static TaskAwaiter<object> GetAwaiterReturnNull(object instruction)
    {
        var tcs = new TaskCompletionSource<object>();
        AsyncCoroutineRunner.Instance.StartCoroutine(
            InstructionWrappers.ReturnNullValue(tcs, instruction));
        return tcs.Task.GetAwaiter();
    }

    static TaskAwaiter<T> GetAwaiterReturnSelf<T>(T instruction)
    {
        var tcs = new TaskCompletionSource<T>();
        AsyncCoroutineRunner.Instance.StartCoroutine(
            InstructionWrappers.ReturnSelf(tcs, instruction));
        return tcs.Task.GetAwaiter();
    }

    static class InstructionWrappers
    {
        public static IEnumerator ReturnNullValue(
            TaskCompletionSource<object> tcs, object instruction)
        {
            yield return instruction;
            tcs.SetResult(null);
        }

        public static IEnumerator AssetBundleCreateRequest(
            TaskCompletionSource<AssetBundle> tcs, AssetBundleCreateRequest instruction)
        {
            yield return instruction;
            tcs.SetResult(instruction.assetBundle);
        }

        public static IEnumerator AssetBundleRequest(
            TaskCompletionSource<UnityEngine.Object> tcs, AssetBundleRequest instruction)
        {
            yield return instruction;
            tcs.SetResult(instruction.asset);
        }

        public static IEnumerator ResourceRequest(
            TaskCompletionSource<UnityEngine.Object> tcs, ResourceRequest instruction)
        {
            yield return instruction;
            tcs.SetResult(instruction.asset);
        }

        public static IEnumerator ReturnSelf<T>(
            TaskCompletionSource<T> tcs, T instruction)
        {
            yield return instruction;
            tcs.SetResult(instruction);
        }
    }

    class CoroutineWrapper
    {
        readonly TaskCompletionSource<object> _tcs;
        readonly Stack<IEnumerator> _processStack;

        public CoroutineWrapper(
            IEnumerator coroutine, TaskCompletionSource<object> tcs)
        {
            _processStack = new Stack<IEnumerator>();
            _processStack.Push(coroutine);
            _tcs = tcs;
        }

        public IEnumerator Run()
        {
            while (true)
            {
                var topWorker = _processStack.Peek();

                bool isDone;

                try
                {
                    isDone = !topWorker.MoveNext();
                }
                catch (Exception e)
                {
                    _tcs.SetException(e);
                    yield break;
                }

                if (isDone)
                {
                    _processStack.Pop();

                    if (_processStack.Count == 0)
                    {
                        _tcs.SetResult(topWorker.Current);
                        yield break;
                    }
                }

                // We could just yield return nested IEnumerator's here but we choose to do
                // our own handling here so that we can catch exceptions in nested coroutines
                // instead of just top level coroutine
                if (topWorker.Current is IEnumerator)
                {
                    _processStack.Push((IEnumerator)topWorker.Current);
                }
                else
                {
                    // Return the current value to the unity engine so it can handle things like
                    // WaitForSeconds, WaitToEndOfFrame, etc.
                    yield return topWorker.Current;
                }
            }
        }
    }
}
