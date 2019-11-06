using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityAsyncAwaitUtil
{
    public class PlayModeTests : MonoBehaviour
    {
        [SerializeField] TestButtonHandler.Settings _buttonSettings = null;

        TestButtonHandler _buttonHandler;

        public void Awake()
        {
            _buttonHandler = new TestButtonHandler(_buttonSettings);
        }

        public void OnGUI()
        {
            _buttonHandler.Restart();

            if (_buttonHandler.Display("Test await seconds"))
            {
                AsyncUtilTests.RunAwaitSecondsTestAsync().WrapErrors();
            }

            if (_buttonHandler.Display("Test return value"))
            {
                AsyncUtilTests.RunReturnValueTestAsync().WrapErrors();
            }

            if (_buttonHandler.Display("Test try-catch exception"))
            {
                AsyncUtilTests.RunTryCatchExceptionTestAsync().WrapErrors();
            }

            if (_buttonHandler.Display("Test unhandled exception"))
            {
                // Note: Without WrapErrors here this wouldn't log anything
                AsyncUtilTests.RunUnhandledExceptionTestAsync().WrapErrors();
            }

            if (_buttonHandler.Display("Test IEnumerator"))
            {
                AsyncUtilTests.RunIEnumeratorTestAsync().WrapErrors();
            }

            if (_buttonHandler.Display("Test IEnumerator with return value (untyped)"))
            {
                AsyncUtilTests.RunIEnumeratorUntypedStringTestAsync().WrapErrors();
            }

            if (_buttonHandler.Display("Test IEnumerator with return value (typed)"))
            {
                AsyncUtilTests.RunIEnumeratorStringTestAsync().WrapErrors();
            }

            if (_buttonHandler.Display("Test IEnumerator unhandled exception"))
            {
                AsyncUtilTests.RunIEnumeratorUnhandledExceptionAsync().WrapErrors();
            }

            if (_buttonHandler.Display("Test IEnumerator try-catch exception"))
            {
                AsyncUtilTests.RunIEnumeratorTryCatchExceptionAsync().WrapErrors();
            }

            if (_buttonHandler.Display("Load assetbundle"))
            {
                AsyncUtilTests.RunAsyncOperationAsync().WrapErrors();
            }

            if (_buttonHandler.Display("Test opening notepad"))
            {
                AsyncUtilTests.RunOpenNotepadTestAsync().WrapErrors();
            }

            if (_buttonHandler.Display("Test www download"))
            {
                AsyncUtilTests.RunWwwAsync().WrapErrors();
            }

            if (_buttonHandler.Display("Test Call Async from coroutine"))
            {
                StartCoroutine(AsyncUtilTests.RunAsyncFromCoroutineTest());
            }

            if (_buttonHandler.Display("Test multiple threads"))
            {
                AsyncUtilTests.RunMultipleThreadsTestAsync().WrapErrors();
            }
        }
    }
}
