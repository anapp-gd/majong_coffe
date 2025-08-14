using System.Collections;
using UnityEngine;
public abstract class Config : ScriptableObject
{
    public abstract IEnumerator Init();
}
