using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AsyncMonoBehaviourEvents : MonoBehaviour
{
    static AsyncMonoBehaviourEvents _instance;

    public static AsyncMonoBehaviourEvents Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameObject("AsyncMonoBehaviourEvents")
                    .AddComponent<AsyncMonoBehaviourEvents>();
            }

            return _instance;
        }
    }

    void Awake()
    {
        // Don't show in scene hierarchy
        gameObject.hideFlags = HideFlags.HideAndDontSave;

        DontDestroyOnLoad(gameObject);
    }
}
