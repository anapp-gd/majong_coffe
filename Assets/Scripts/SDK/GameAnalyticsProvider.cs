using System.Collections.Generic;
using GameAnalyticsSDK;
using UnityEngine;

public class GameAnalyticsProvider : IAnalyticProvider
{
    public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        // GameAnalytics умеет в "дизайн-событи€"
        if (parameters == null || parameters.Count == 0)
            GameAnalytics.NewDesignEvent(eventName);
        else
            GameAnalytics.NewDesignEvent(eventName, 0); // ћожно прокинуть value

        Debug.Log($"[GA] Event: {eventName}");
    }
}
