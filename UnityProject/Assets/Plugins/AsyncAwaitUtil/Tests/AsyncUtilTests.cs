using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace UnityAsyncAwaitUtil
{
    public static class AsyncUtilTests
    {
        const string AssetBundleSampleUrl = "http://www.stevevermeulen.com/wp-content/uploads/2017/09/teapot.unity3d";
        const string AssetBundleSampleAssetName = "Teapot";

        public static IEnumerator RunAsyncFromCoroutineTest()
        {
            Debug.Log("Waiting 1 second...");
            yield return new WaitForSeconds(1.0f);
            Debug.Log("Waiting 1 second again...");
            yield return RunAsyncFromCoroutineTest2().AsIEnumerator();
            Debug.Log("Done");
        }

        public static async Task RunMultipleThreadsTestAsync()
        {
            PrintCurrentThreadContext("Start");
            await Task.Delay(TimeSpan.FromSeconds(1.0f));
            PrintCurrentThreadContext("After delay");
            await new WaitForBackgroundThread();
            PrintCurrentThreadContext("After WaitForBackgroundThread");
            Debug.Log("Waiting 1 second...");
            await Task.Delay(TimeSpan.FromSeconds(1.0f));
            // We will still running from the threadpool after the delay here
            PrintCurrentThreadContext("After Waiting");
            // We can place any unity yield instruction here instead and it will return to the unity thread
            await new WaitForUpdate();
            PrintCurrentThreadContext("After WaitForUpdate");
        }

        private static async Task RunMultipleThreadsTestAsyncWait()
        {
            PrintCurrentThreadContext("RunMultipleThreadsTestAsyncWait1");
            await new WaitForSeconds(1.0f);
            PrintCurrentThreadContext("RunMultipleThreadsTestAsyncWait2");
        }

        private static void PrintCurrentThreadContext(string prefix = null)
        {
            Debug.Log(string.Format("{0}Current Thread: {1}, Scheduler: {2}",
                prefix == null ? "" : prefix + ": ", Thread.CurrentThread.ManagedThreadId, SynchronizationContext.Current == null ? "null" : SynchronizationContext.Current.GetType().Name));
        }

        private static async Task RunAsyncFromCoroutineTest2()
        {
            await new WaitForSeconds(1.0f);
        }

        public static async Task RunWwwAsync()
        {
            Debug.Log("Downloading asset bundle using WWW");
            var bytes = (await new WWW(AssetBundleSampleUrl)).bytes;
            Debug.Log("Downloaded " + (bytes.Length / 1024) + " kb");
        }

        public static async Task RunOpenNotepadTestAsync()
        {
            Debug.Log("Waiting for user to close notepad...");
            await Process.Start("notepad.exe");
            Debug.Log("Closed notepad");
        }

        public static async Task RunUnhandledExceptionTestAsync()
        {
            // This should be logged when using WrapErrors
            await WaitThenThrowException();
        }

        public static async Task RunTryCatchExceptionTestAsync()
        {
            try
            {
                await NestedRunAsync();
            }
            catch (Exception e)
            {
                Debug.Log("Caught exception! " + e.Message);
            }
        }

        private static async Task NestedRunAsync()
        {
            await new WaitForSeconds(1);
            throw new Exception("foo");
        }

        private static async Task WaitThenThrowException()
        {
            await new WaitForSeconds(1.5f);
            throw new Exception("asdf");
        }

        public static async Task RunAsyncOperationAsync()
        {
            await InstantiateAssetBundleAsync(AssetBundleSampleUrl, AssetBundleSampleAssetName);
        }

        private static async Task InstantiateAssetBundleAsync(string abUrl, string assetName)
        {
            // We could use WWW here too which might be easier
            Debug.Log("Downloading asset bundle data...");
            var assetBundle = await AssetBundle.LoadFromMemoryAsync(
                await DownloadRawDataAsync(abUrl));

            var prefab = (GameObject)(await assetBundle.LoadAssetAsync<GameObject>(assetName));

            GameObject.Instantiate(prefab);
            assetBundle.Unload(false);
            Debug.Log("Asset bundle instantiated");
        }

        private static async Task<byte[]> DownloadRawDataAsync(string url)
        {
            var request = UnityWebRequest.Get(url);
            await request.Send();
            return request.downloadHandler.data;
        }

        public static async Task RunIEnumeratorTryCatchExceptionAsync()
        {
            try
            {
                await WaitThenThrow();
            }
            catch (Exception e)
            {
                Debug.Log("Caught exception! " + e.Message);
            }
        }

        public static async Task RunIEnumeratorUnhandledExceptionAsync()
        {
            await WaitThenThrow();
        }

        private static IEnumerator WaitThenThrow()
        {
            yield return WaitThenThrowNested();
        }

        private static IEnumerator WaitThenThrowNested()
        {
            Debug.Log("Waiting 1 second...");
            yield return new WaitForSeconds(1.0f);
            throw new Exception("zxcv");
        }

        public static async Task RunIEnumeratorStringTestAsync()
        {
            Debug.Log("Waiting for ienumerator...");
            Debug.Log("Done! Result: " + await WaitForString());
        }

        public static async Task RunIEnumeratorUntypedStringTestAsync()
        {
            Debug.Log("Waiting for ienumerator...");
            string result = (string)(await WaitForStringUntyped());
            Debug.Log("Done! Result: " + result);
        }

        public static async Task RunIEnumeratorTestAsync()
        {
            Debug.Log("Waiting for ienumerator...");
            await WaitABit();
            Debug.Log("Done!");
        }

        private static IEnumerator<string> WaitForString()
        {
            var startTime = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - startTime < 2)
            {
                yield return null;
            }
            yield return "bsdfgas";
        }

        private static IEnumerator WaitForStringUntyped()
        {
            yield return WaitABit();
            yield return "qwer";
        }

        private static IEnumerator WaitABit()
        {
            yield return new WaitForSeconds(1.5f);
        }

        public static async Task RunReturnValueTestAsync()
        {
            Debug.Log("Waiting to get value...");
            var result = await GetValueExampleAsync();
            Debug.Log("Got value: " + result);
        }

        private static async Task<string> GetValueExampleAsync()
        {
            await new WaitForSeconds(1.0f);
            return "asdf";
        }

        public static async Task RunAwaitSecondsTestAsync()
        {
            Debug.Log("Waiting 1 second...");
            await new WaitForSeconds(1.0f);
            Debug.Log("Done!");
        }
    }
}
