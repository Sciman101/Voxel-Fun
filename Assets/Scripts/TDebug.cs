using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TDebug : MonoBehaviour
{
    static TDebug instance;

    Queue<string> messages = new Queue<string>();

    private void Awake()
    {
        instance = this;
    }

    private void LateUpdate()
    {
        while (messages.Count > 0)
            Debug.Log(messages.Dequeue());
    }

    public static void Log(string msg)
    {
        instance.messages.Enqueue(msg);
    }
}
