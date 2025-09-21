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
        Application.targetFrameRate = 60;
    }
    protected override void Start()
    {
        // ��������� ���������������� �������������
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        Vibration.Init();

        // 1. ��������� ������
        EntityModule.Initialize();

        UIModule.Initialize();

        // 2. ������ (��� ����������)
        var configLoaded = new TaskCompletionSource<bool>();
        ConfigModule.Initialize(this, () => configLoaded.SetResult(true));
        await configLoaded.Task;

        // 3. SDK (�� �������)
        await InitializeSDKs();

        // 4. ����� ������ ������������� ������ �����
        LoadTargetScene();
    }

    private async Task InitializeSDKs()
    { 
        // 1. ����������� ������� (iOS ATT)
        await iOSAdvertisementSupport.RequestTracking();

        // 2. AppsFlyer  
        await Instantiate(appsflyerRef).InitializeAsync();

        // 3. AppMetrica
        await AppMetricaInitializer.InitializeAsync();

        await GameAnalyticsInitializer.InitializeAsync();

        await FacebookInitializer.InitializeAsync();
         
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

