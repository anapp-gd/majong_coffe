using System.Threading.Tasks;
using UnityEngine; 
using Facebook.Unity;

public static class FacebookInitializer
{
    private static TaskCompletionSource<bool> _tcs;

    public static Task InitializeAsync()
    {
        _tcs = new TaskCompletionSource<bool>();

#if UNITY_EDITOR
        // В редакторе фейсбук не работает, сразу "успех"
        _tcs.SetResult(true);
        return _tcs.Task;
#endif

        if (!FB.IsInitialized)
        {
            // Инициализация SDK
            FB.Init(OnInitComplete, OnHideUnity);
        }
        else
        {
            // Уже был инициализирован
            FB.ActivateApp();
            _tcs.SetResult(true);
        }

        return _tcs.Task;
    }

    private static void OnInitComplete()
    {
        Debug.Log("[FB] Initialization complete.");
        if (FB.IsInitialized)
        {
            FB.ActivateApp();
            AnalyticsHolder.Register(new FacebookProvider());
            _tcs.TrySetResult(true);
        }
        else
        {
            Debug.LogError("[FB] Failed to initialize the Facebook SDK.");
            _tcs.TrySetResult(false);
        }
    }

    private static void OnHideUnity(bool isGameShown)
    {
        // Этот коллбек нужен SDK (например, при сворачивании игры)
        Debug.Log("[FB] App visibility: " + isGameShown);
    }
}
