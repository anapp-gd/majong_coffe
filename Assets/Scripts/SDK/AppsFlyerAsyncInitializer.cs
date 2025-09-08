using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_IOS
using AppsFlyerSDK;
#endif

#if UNITY_ANDROID
using AppsFlyerSDK;
#endif
public class AppsFlyerInitializer : MonoBehaviour, IAppsFlyerConversionData
{
    private TaskCompletionSource<bool> _tcs;

    [SerializeField] private string devKeyAndroid;
    [SerializeField] private string devKeyiOS;
    [SerializeField] private string appStoreAppID;

    public async Task InitializeAsync()
    {
        _tcs = new TaskCompletionSource<bool>();

#if UNITY_EDITOR
        _tcs.SetResult(true);
        await _tcs.Task;
        return;
#endif

#if UNITY_ANDROID
        AppsFlyer.initSDK(devKeyAndroid, Application.identifier, this);
        AppsFlyer.startSDK();
#elif UNITY_IOS
        AppsFlyer.initSDK(devKeyiOS, appStoreAppID, this);
        AppsFlyer.enableTCFDataCollection(true);
        AppsFlyer.startSDK();
#endif

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