using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitState : State
{
    [SerializeField] private AppsFlyerInitializer appsflyerRef;
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
        await Instantiate(appsflyerRef).InitializeAsync();

        // 3. AppMetrica
        await AppMetricaInitializer.InitializeAsync();

        await GameAnalyticsInitializer.InitializeAsync();

        await FacebookInitializer.InitializeAsync();
         
        AnalyticsHolder.Register(new AppsFlyerProvider());
        AnalyticsHolder.Register(new AppMetricaProvider());
        AnalyticsHolder.Register(new GameAnalyticsProvider());
        AnalyticsHolder.Register(new FacebookProvider());
    }

    private void LoadTargetScene()
    {
        AnalyticsHolder.GameStart();

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

