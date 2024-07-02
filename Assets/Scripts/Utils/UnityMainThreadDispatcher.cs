// Author: Nikki Asteinza (2024-06-22)

//This Unity script is designed to execute actions on Unity's main thread from other threads.
//This is particularly useful for operations that need to interact with Unity's API, which must be called from the main thread.

//Functionality:
//Singleton Pattern: Ensures only one instance of UnityMainThreadDispatcher exists throughout the application's lifecycle.
//Action Queue: Maintains a queue of actions to be executed on the main thread.
//Thread-Safe Enqueueing: Allows other threads to enqueue actions safely for execution on the main thread.
//Main Thread Execution: Executes all queued actions on the main thread in the Update method.

//Key Features:
//Singleton Instance: Ensures a single instance using DontDestroyOnLoad to persist across scene loads.
//Thread Safety: Uses a lock mechanism to ensure that actions are enqueued and dequeued safely across multiple threads.
//Main Thread Execution: Dequeues and executes actions in the Update method, ensuring they run on the main thread.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher instance;
    private Queue<Action> actionQueue = new Queue<Action>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        lock (actionQueue)
        {
            while (actionQueue.Count > 0)
            {
                Action action = actionQueue.Dequeue();
                action();
            }
        }
    }

    public void Enqueue(Action action)
    {
        lock (actionQueue)
        {
            actionQueue.Enqueue(action);
        }
    }

    public static UnityMainThreadDispatcher Instance()
    {
        if (instance == null)
        {
            Debug.LogError("UnityMainThreadDispatcher instance is null!");
        }
        return instance;
    }
}
