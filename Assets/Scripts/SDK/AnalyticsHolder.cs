using System.Collections.Generic;
using UnityEngine;

public class AnalyticsHolder
{
    private static readonly List<IAnalyticProvider> _providers = new List<IAnalyticProvider>();

    /// <summary>
    /// ������������ ���������� ��������� (SDK).
    /// ��� ����� �������� � InitState.InitializeSDKs().
    /// </summary>
    public static void Register(IAnalyticProvider provider)
    {
        if (!_providers.Contains(provider))
            _providers.Add(provider);
    }

    /// <summary>
    /// ������������� ����� ��� ����������� ���������� �������.
    /// </summary>
    public static void LogEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        foreach (var provider in _providers)
        {
            provider.LogEvent(eventName, parameters);
        }
    }

    // =========================
    // ������� ������� �������
    // =========================
    public static void GameStart()
    {
        LogEvent("game_start");
    }
    
    public static void TutorDone()
    {
        LogEvent("tutor_done");
    }

    public static void GameEnd()
    {
        LogEvent("game_end");
    }

    public static void Victory()
    {
        LogEvent("victory");
    }

    public static void Defeat()
    {
        LogEvent("defeat");
    }
}
