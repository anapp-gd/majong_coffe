using System.Threading.Tasks;
using UnityEngine;
using Io.AppMetrica;

public static class AppMetricaInitializer
{
    private static bool _initialized = false;

    /// <summary>
    /// ����������� ������������� AppMetrica ��� ��������� InitState
    /// </summary>
    public static Task InitializeAsync()
    {
        if (_initialized)
            return Task.CompletedTask;

        _initialized = true;

#if UNITY_EDITOR
        // � ��������� ������ ����� ���������� Task
        return Task.CompletedTask;
#else
        var tcs = new TaskCompletionSource<bool>();

        try
        {
#if UNITY_ANDROID || UNITY_IOS
            // ������� ������ SDK
            var config = new AppMetricaConfig("YOUR_API_KEY")
            {
                FirstActivationAsUpdate = !IsFirstLaunch()
            };

            AppMetrica.Activate(config);

            // ����� �������� ����������� �������, ���� �����
            // ��������, AppMetrica.OnRevenueEvent += ...
#endif
            // SDK �������������, ������� Task �����������
            tcs.SetResult(true);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AppMetrica] Initialization error: {e}");
            tcs.SetResult(true); // ���� ��� ������ ���������� ��������
        }

        return tcs.Task;
#endif
    }

    private static bool IsFirstLaunch()
    {
        // ��� ����� ������������ PlayerPrefs ��� �������� �����
        const string key = "AppMetrica_FirstLaunch";

        if (PlayerPrefs.HasKey(key))
            return false;

        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
        return true;
    }
}
