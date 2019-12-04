using System.Collections;
using System.Collections.Generic;

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityAsyncAwaitUtil
{
    class TestMisc
    {
        [UnityTest]
        public IEnumerator TestAsyncCoroutineRunnerInEditor()
        {
            List<bool> result = new List<bool>();
            AsyncCoroutineRunner.Instance.StartCoroutine(AppendCoroutine(result));
            for (int i = 0; i < 10; ++i)
            {
                if (result.Count > 0) { yield break; }
                yield return null;
            }
            Assert.Fail("Coroutine did not complete");
        }

        static IEnumerator AppendCoroutine(List<bool> result)
        {
            yield return null;
            result.Add(true);
        }
    }
}
