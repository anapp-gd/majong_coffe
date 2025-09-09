using System.Collections.Generic;
using UnityEngine;

public interface IAnalyticProvider
{
    void LogEvent(string eventName, Dictionary<string, object> parameters = null);
}
