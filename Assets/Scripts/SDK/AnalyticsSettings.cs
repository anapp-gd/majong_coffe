using UnityEngine;

public static class AnalyticsSettings
{
    private static AnalyticsConfig _config;

    public static AnalyticsConfig Config
    {
        get
        {
            if (_config == null)
            {
                // ��������� �� Resources
                _config = Resources.Load<AnalyticsConfig>("AnalyticsConfig");
                if (_config == null)
                {
                    Debug.LogError("[AnalyticsSettings] �� ������ AnalyticsConfig � Resources!");
                }
            }
            return _config;
        }
    }
}
