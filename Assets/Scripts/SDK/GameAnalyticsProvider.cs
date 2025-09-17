using System.Collections.Generic;
using GameAnalyticsSDK;
using UnityEngine;

public class GameAnalyticsProvider : IAnalyticProvider
{
    public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        // GameAnalytics ����� � "������-�������"
        if (parameters == null || parameters.Count == 0)
            GameAnalytics.NewDesignEvent(eventName);
        else
            GameAnalytics.NewDesignEvent(eventName, 0); // ����� ��������� value

        Debug.Log($"[GA] Event: {eventName}");
    }
}
