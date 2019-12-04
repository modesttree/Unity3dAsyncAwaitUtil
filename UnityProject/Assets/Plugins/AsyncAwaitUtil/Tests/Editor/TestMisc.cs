using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityAsyncAwaitUtil
{
    class TestMisc
    {
        // Helper for writing async editor tests.
        static IEnumerator AsUnityTest(Func<Task> body)
        {
            var task = Task.Run(body);
            while (!task.IsCompleted)
            {
                yield return null;
            }
            if (task.IsFaulted)
            {
                ExceptionDispatchInfo.Capture(task.Exception.InnerExceptions[0]).Throw();
            }
        }

        [UnityTest]
        public IEnumerator TestAsyncCoroutineRunnerInEditor()
        {
            List<bool> result = new List<bool>();
            IEnumerator AppendResult()
            {
                yield return null;
                result.Add(true);
            }
            AsyncCoroutineRunner.Instance.StartCoroutine(AppendResult());
            for (int i = 0; i < 10; ++i)
            {
                if (result.Count > 0) { yield break; }
                yield return null;
            }
            Assert.Fail("Coroutine did not complete in the allotted time");
        }


        [UnityTest]
        public IEnumerator TestAwaitSecondsRealtime() => AsUnityTest(async () =>
        {
            const float delay = .25f;
            var start = DateTime.Now;
            await new WaitForSecondsRealtime(delay);
            Assert.Greater(DateTime.Now - start, TimeSpan.FromSeconds(delay - .01));
        });
    }
}
