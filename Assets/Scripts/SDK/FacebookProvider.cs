using System.Collections.Generic;
using Facebook.Unity;
using UnityEngine; 

public class FacebookProvider : IAnalyticProvider
{
    public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        FB.LogAppEvent(eventName, parameters: parameters);
        Debug.Log($"[FB] Event: {eventName}");
    }
}
