using System.Collections.Generic;
using Io.AppMetrica;
using UnityEngine;

public class AppMetricaProvider : IAnalyticProvider
{
    public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        if (parameters != null)
            AppMetrica.ReportEvent(eventName, parameters.Values.ToString());
        else
            AppMetrica.ReportEvent(eventName);

        Debug.Log($"[AppMetrica] Event: {eventName}");
    }
}