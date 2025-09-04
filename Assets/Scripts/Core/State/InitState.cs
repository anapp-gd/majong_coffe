using UnityEngine;
using UnityEngine.SceneManagement;

public class InitState : State
{ 
    public static new InitState Instance
    {
        get
        {
            return (InitState)State.Instance;
        }
    }
#if UNITY_EDITOR
    [UnityEngine.SerializeField] UnityEditor.SceneAsset TargetScene;
#endif
    [SerializeField] private string targetSceneName;
    protected override void Awake()
    {

    }

    protected override void Start()
    { 
        EntityModule.Initialize();

        UIModule.Initialize();

        ConfigModule.Initialize(this, onConfigLoaded);
    }

    protected override void Update()
    {

    } 

    void onConfigLoaded()
    {
        if (PlayerEntity.Instance.TutorDone)
        { 
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            SceneManager.LoadScene(3);
        }
    } 

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (TargetScene)
        {
            targetSceneName = TargetScene.name;
        }
    }
#endif
}
