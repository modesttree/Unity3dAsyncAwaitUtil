#if UNITY_EDITOR
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;
using Task = System.Threading.Tasks.Task;

using UnityEditor;
using UnityEditor.VersionControl;

namespace UnityAsyncAwaitUtil
{
    /// <summary>
    /// Tests async/await/IEnumerator integration in Edit Mode.
    ///
    /// In Edit Mode, Unity does not call `MoveNext` on
    /// coroutines at regular intervals. Instead, we have implemented
    /// `EditModeCoroutineRunner` to track running coroutines and to call
    /// `MoveNext` on them during each Editor update
    /// (`EditorApplication.update` event).
    ///
    /// It would be much nicer if these tests were implemented as standard unit
    /// tests that could be run with the Unity Test Runner. Unfortunately, as
    /// of Unity 2017.1.1f1, the version of NUnit shipped with Unity does not
    /// support using `async`/`await` in tests.  See the following link for a
    /// discussion of this issue:
    /// https://forum.unity.com/threads/async-await-in-unittests.513857/
    /// </summary>
    public class EditModeTestsWindow : EditorWindow
    {
        private TestButtonHandler _buttonHandler;

        [MenuItem("Window/AsyncAwaitUtil/Edit Mode Tests")]
        public static void ShowWindow()
        {
            GetWindow<EditModeTestsWindow>();
        }

        public void Awake()
        {
            TestButtonHandler.Settings settings
                = new TestButtonHandler.Settings();

            settings.NumPerColumn = 4;
            settings.VerticalMargin = 10;
            settings.VerticalSpacing = 10;
            settings.HorizontalSpacing = 10;
            settings.HorizontalMargin = 10;
            settings.ButtonWidth = 300;
            settings.ButtonHeight = 30;

            _buttonHandler = new TestButtonHandler(settings);
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
                EditModeCoroutineRunner.Instance.StartCoroutine(
                    AsyncUtilTests.RunAsyncFromCoroutineTest());
            }

            if (_buttonHandler.Display("Test multiple threads"))
            {
                AsyncUtilTests.RunMultipleThreadsTestAsync().WrapErrors();
            }
        }

    }

}
#endif
