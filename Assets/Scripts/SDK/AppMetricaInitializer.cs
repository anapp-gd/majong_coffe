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
        // В редакторе просто сразу возвращаем Task
        return Task.CompletedTask;
#else
        var tcs = new TaskCompletionSource<bool>();

        try
        {
#if UNITY_ANDROID || UNITY_IOS
            // Создаем конфиг SDK
            var config = new AppMetricaConfig("YOUR_API_KEY")
            {
                FirstActivationAsUpdate = !IsFirstLaunch()
            };

            AppMetrica.Activate(config);

            // Можно добавить обработчики событий, если нужно
            // Например, AppMetrica.OnRevenueEvent += ...
#endif
            // SDK активировался, считаем Task завершенным
            tcs.SetResult(true);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AppMetrica] Initialization error: {e}");
            tcs.SetResult(true); // даже при ошибке продолжаем пайплайн
        }

        return tcs.Task;
#endif
    }

    private static bool IsFirstLaunch()
    {
        // Тут можно использовать PlayerPrefs или проверку файла
        const string key = "AppMetrica_FirstLaunch";

        if (PlayerPrefs.HasKey(key))
            return false;

        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
        return true;
    }
}
