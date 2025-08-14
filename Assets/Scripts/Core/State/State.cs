using UnityEngine;

public abstract class State : MonoBehaviour
{
    protected abstract void Awake();
    protected abstract void Start();
    protected virtual void Update()
    {
        
    }
}
