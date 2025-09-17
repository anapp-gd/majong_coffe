using System.Threading.Tasks;
using UnityEngine;
using GameAnalyticsSDK;

public static class GameAnalyticsInitializer
{
    private static bool _initialized = false;

    public static Task InitializeAsync()
    {
        if (_initialized)
            return Task.CompletedTask;

        _initialized = true;

#if UNITY_EDITOR
        return Task.CompletedTask;
#else
        var tcs = new TaskCompletionSource<bool>();

        try
        {
            // Инициализация GameAnalytics
            GameAnalytics.Initialize();

            // GameAnalytics инициализируется синхронно, можем сразу завершить Task
            tcs.SetResult(true);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameAnalytics] Initialization error: {e}");
            tcs.SetResult(true);
        }
        
        AnalyticsHolder.Register(new GameAnalyticsProvider());
        return tcs.Task;
#endif
    }
}
