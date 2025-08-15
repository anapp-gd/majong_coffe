using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigModule
{

    public static Action<string, float> OnLoaded;
    public static bool AllConfigInited;
    public static bool AllConfigLoaded;
    private static Dictionary<Type, Config> _configData = new Dictionary<Type, Config>();
    /// <summary>
    /// Инициализирует все Config из папки "Configs" и ожидает завершения их инициализации.
    /// </summary>
    public static void Initialize(MonoBehaviour caller, Action onComplete)
    {
        if (caller == null)
        {
            Debug.LogError("Caller is null. Config initialization requires a valid MonoBehaviour instance.");
            return;
        }

        caller.StartCoroutine(InitializeConfigsCoroutine(caller, onComplete));
    }

    /// <summary>
    /// Возвращает объект Config по его типу.
    /// </summary>
    public static T GetConfig<T>() where T : Config
    {
        if (_configData.TryGetValue(typeof(T), out var config))
        {
            return config as T;
        }

        Debug.LogError($"Config of type {typeof(T).Name} not found.");
        return null;
    }

    /// <summary>
    /// Загружает все Config из папки "Configs" и инициализирует их.
    /// </summary>
    private static IEnumerator InitializeConfigsCoroutine(MonoBehaviour caller, Action onComplete)
    {
        Debug.Log("Starting config initialization...");
        // Загружаем все Config из папки "Configs"
        var configs = Resources.LoadAll<Config>("Config");

        string loadedInfo = $"Загрузка {configs.Length} настроек";

        float loadedValue = 0f;

        float maxValue = configs.Length;

        OnLoaded?.Invoke(loadedInfo, loadedValue);

        int currentIndex = 0;

        foreach (var config in configs)
        {
            currentIndex++;

            var type = config.GetType();

            if (!_configData.ContainsKey(type))
            {
                _configData.Add(type, config);

                loadedValue = currentIndex / maxValue;

                loadedInfo = $"Загрузка {config.name}";

                OnLoaded?.Invoke(loadedInfo, loadedValue);

                yield return new WaitForSeconds(0.5f);
            }
        }

        AllConfigLoaded = true;
        currentIndex = 0;

        // Инициализируем каждый Config
        foreach (var config in _configData.Values)
        {
            currentIndex++;

            loadedValue = currentIndex / maxValue;

            Debug.Log($"Initializing {config.name}...");

            loadedInfo = $"Инициализация {config.name}";

            OnLoaded?.Invoke(loadedInfo, loadedValue);

            yield return caller.StartCoroutine(config.Init());
            Debug.Log($"Initialized {config.name}");

        }

        Debug.Log("All configs initialized successfully.");

        loadedInfo = $"Загрузка завершена";

        AllConfigInited = true;

        onComplete?.Invoke();
    }
}
