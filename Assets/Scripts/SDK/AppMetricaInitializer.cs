using System.Threading.Tasks;
using UnityEngine;
using Io.AppMetrica;

public static class AppMetricaInitializer
{
    private static bool _initialized = false;

    /// <summary>
    /// Асинхронная инициализация AppMetrica для пайплайна InitState
    /// </summary>
    public static Task InitializeAsync()
    {
        if (_initialized)
            return Task.CompletedTask;

        _initialized = true;

#if UNITY_EDITOR
        // В редакторе сразу возвращаем завершённый Task
        return Task.CompletedTask;
#else
        var tcs = new TaskCompletionSource<bool>();

        try
        {
#if UNITY_ANDROID
            InitializeAndroid();
#elif UNITY_IOS
            InitializeiOS();
#endif
            tcs.SetResult(true);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AppMetrica] Initialization error: {e}");
            tcs.SetResult(true); // даже при ошибке продолжаем пайплайн
        }
        
        AnalyticsHolder.Register(new AppMetricaProvider());
        return tcs.Task;
#endif
    }

#if UNITY_ANDROID
    private static void InitializeAndroid()
    {
        var androidApiKey = "4e6b8e7f-3e44-474f-b853-03a08334540d";

        var config = new AppMetricaConfig(androidApiKey)
        {
            FirstActivationAsUpdate = !IsFirstLaunch()
            // Можно добавить специфичные для Android настройки
        };

        AppMetrica.Activate(config);
        Debug.Log("[AppMetrica] Android initialized");
    }
#endif

#if UNITY_IOS
    private static void InitializeiOS()
    {
        var iosApiKey = "c4c789a2-37ba-4970-a93d-1cab2f77c2a7";

        var config = new AppMetricaConfig(iosApiKey)
        {
            FirstActivationAsUpdate = !IsFirstLaunch()
            // Можно добавить специфичные для iOS настройки
        };

        AppMetrica.Activate(config);
        Debug.Log("[AppMetrica] iOS initialized");
    }
#endif

    private static bool IsFirstLaunch()
    {
        const string key = "AppMetrica_FirstLaunch";

        if (PlayerPrefs.HasKey(key))
            return false;

        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
        return true;
    }
}
