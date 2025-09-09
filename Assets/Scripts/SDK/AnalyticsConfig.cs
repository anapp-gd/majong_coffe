using UnityEngine;

[CreateAssetMenu(fileName = "AnalyticsConfig", menuName = "Config/AnalyticsConfig")]
public class AnalyticsConfig : ScriptableObject
{
    [Header("AppsFlyer")]
    public string AppsFlyerDevKey;
    public string AppsFlyerDevKeyiOS;
    public string AppsFlyerAppStoreId;

    [Header("AppMetrica")]
    public string AppMetricaApiKey;

    [Header("GameAnalytics")]
    public string AndroidgameAnalyticsGameKey;
    public string AndroidgameAnalyticsSecretKey;
    public string IOSgameAnalyticsGameKey;
    public string IOSgameAnalyticsSecretKey;

    [Header("Facebook")]
    public string FacebookAppId;
    public string FacebookSecretKey; 
}
