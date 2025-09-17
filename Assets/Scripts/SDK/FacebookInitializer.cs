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
        // � ��������� ������� �� ��������, ����� "�����"
        _tcs.SetResult(true);
        return _tcs.Task;
#endif

        if (!FB.IsInitialized)
        {
            // ������������� SDK
            FB.Init(OnInitComplete, OnHideUnity);
        }
        else
        {
            // ��� ��� ���������������
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
        // ���� ������� ����� SDK (��������, ��� ������������ ����)
        Debug.Log("[FB] App visibility: " + isGameShown);
    }
}
