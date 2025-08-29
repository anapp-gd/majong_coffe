using System;
using System.Collections;
using UnityEngine;

public abstract class State : MonoBehaviour
{
    protected static State _instance;
    public static State Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<State>();
            }
            return _instance;
        }
    }
    protected abstract void Awake();
    protected abstract void Start();
    public virtual void Close()
    {

    }
    protected virtual void Update()
    {

    }
    public virtual Coroutine RunCoroutine(IEnumerator coroutine, Action callback = null)
    {
        if (Instance != null)
        {
            return StartCoroutine(Instance.CoroutineWrapper(coroutine, callback));
        }
        else
        {
            return null;
        }
    }
    public virtual Coroutine RunCoroutine(IEnumerator coroutine, params Action[] callback)
    {
        if (Instance != null)
        {
            return StartCoroutine(Instance.CoroutineWrapper(coroutine, callback));
        }
        else
        {
            return null;
        }
    }
    protected virtual IEnumerator CoroutineWrapper(IEnumerator coroutine, Action callback = null)
    {
        yield return StartCoroutine(coroutine);

        callback?.Invoke();
    }
    protected virtual IEnumerator CoroutineWrapper(IEnumerator coroutine, params Action[] callback)
    {
        yield return StartCoroutine(coroutine);

        for (int i = 0; i < callback.Length; i++)
        {
            callback[i]?.Invoke();
        }
    }

}
