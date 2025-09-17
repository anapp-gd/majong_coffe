using System.Collections.Generic;
using AppsFlyerSDK;
using UnityEngine;

public class AppsFlyerProvider : IAnalyticProvider
{
    public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
    { 
        AppsFlyer.sendEvent(eventName, new Dictionary<string, string>());
        Debug.Log($"[AppsFlyer] Event: {eventName}");
    }
}
