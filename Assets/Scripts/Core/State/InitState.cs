using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitState : State
{
    public static new InitState Instance => (InitState)State.Instance;

#if UNITY_EDITOR
    [SerializeField] private UnityEditor.SceneAsset TargetScene;
#endif
    [SerializeField] private string targetSceneName;

    protected override void Awake()
    { 

    }

    protected override void Start()
    {
        // Запускаем централизованную инициализацию
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        // 1. Локальные модули
        EntityModule.Initialize();

        UIModule.Initialize();

        // 2. Конфиг (ждём завершения)
        var configLoaded = new TaskCompletionSource<bool>();
        ConfigModule.Initialize(this, () => configLoaded.SetResult(true));
        await configLoaded.Task;

        // 3. SDK (по порядку)
        await InitializeSDKs();

        // 4. После полной инициализации грузим сцену
        LoadTargetScene();
    }

    private async Task InitializeSDKs()
    { 
        // 1. Запрашиваем трекинг (iOS ATT)
        await iOSAdvertisementSupport.RequestTracking(); 
        
        // 2. AppsFlyer
        var afInitializer = gameObject.AddComponent<AppsFlyerInitializer>();
#if UNITY_ANDROID
        afInitializer.devKeyAndroid = "YOUR_ANDROID_DEV_KEY";
#elif UNITY_IOS
    afInitializer.devKeyiOS = "YOUR_IOS_DEV_KEY";
    afInitializer.appStoreAppID = "YOUR_APPSTORE_APP_ID";
#endif
        await afInitializer.InitializeAsync();
    }

    private void LoadTargetScene()
    {
        if (PlayerEntity.Instance.TutorDone)
            SceneManager.LoadScene(targetSceneName);
        else
            SceneManager.LoadScene(3);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (TargetScene)
            targetSceneName = TargetScene.name;
    }
#endif
}

