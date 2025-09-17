using System.Threading.Tasks;
using UnityEngine;
using AppsFlyerSDK;
 
public class AppsFlyerInitializer : MonoBehaviour, IAppsFlyerConversionData
{
    private TaskCompletionSource<bool> _tcs;

    string devKey = "FPvLwYMS72KXycgppD6iTm";
    string appId = "6751897265";
    public async Task InitializeAsync()
    {
        _tcs = new TaskCompletionSource<bool>();

#if UNITY_EDITOR
        _tcs.SetResult(true);
        await _tcs.Task;
        return;
#endif
        AppsFlyer.initSDK(devKey, appId, this);
        AppsFlyer.startSDK(); 
#if UNITY_ANDROID
#elif UNITY_IOS
        AppsFlyer.initSDK(devKey,appId, this);
        AppsFlyer.enableTCFDataCollection(true);
        AppsFlyer.startSDK();
#endif
        AnalyticsHolder.Register(new AppsFlyerProvider());
        await _tcs.Task; // ждём callback от AppsFlyer
    }

    // --- IAppsFlyerConversionData callbacks ---
    public void onConversionDataSuccess(string conversionData)
    {
        Debug.Log("[AppsFlyer] Conversion Data: " + conversionData);
        _tcs?.TrySetResult(true);
    }

    public void onConversionDataFail(string error)
    {
        Debug.LogError("[AppsFlyer] Conversion Failed: " + error);
        _tcs?.TrySetResult(false);
    }

    public void onAppOpenAttribution(string attributionData)
    {
        Debug.Log("[AppsFlyer] App Open Attribution: " + attributionData);
    }

    public void onAppOpenAttributionFailure(string error)
    {
        Debug.LogError("[AppsFlyer] Attribution Failure: " + error);
    }
}