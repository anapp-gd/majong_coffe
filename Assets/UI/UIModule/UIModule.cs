using UnityEngine;

public static class UIModule
{
    private static UIHandler Handler;

    public static void Initialize()
    {
        GameObject[] allPrefabs = Resources.LoadAll<GameObject>("");

        GameObject uiHandlerPrefab = null;

        foreach (var prefab in allPrefabs)
        {
            if (prefab.GetComponent<UIHandler>() != null)
            {
                uiHandlerPrefab = prefab;
                break;
            }
        }

        if (uiHandlerPrefab == null)
        {
            Debug.LogError("Не найден префаб с компонентом UIHandler в папке Resources!");
            return;
        }

        GameObject instance = Object.Instantiate(uiHandlerPrefab);

        Handler = instance.GetComponent<UIHandler>();

        GameObject.DontDestroyOnLoad(instance);

        instance.name = "UIHandler";

        Handler.Init();

        if (Handler == null)
        {
            Debug.LogError("На инстансе нет UIHandler!");
            Object.Destroy(instance);
        }
    }
    public static void Inject(params object[] data) 
    {
        Handler?.Inject(data); 
    }
    public static void Play()
    {
        Handler.Play();
    } 
    public static bool OpenCanvas<T>(out T canvas) where T : SourceCanvas
    {
        canvas = null;
         
        bool result = Handler.InvokeCanvas(out canvas);

        return result;
    }

    public static bool CloseCanvas<T>(out T canvas) where T : SourceCanvas
    {
        canvas = null;

        bool result = Handler.CloseCanvas(out canvas);

        return result;
    }

    public static bool TryGetCanvas<T>(out T canvas) where T : SourceCanvas
    {
        if (Handler.TryGetCanvas(out canvas))
        {
            return true;
        }

        return false;
    }

    public static void DisposeCanvas()
    {
        Handler.Dispose();
    }
}  