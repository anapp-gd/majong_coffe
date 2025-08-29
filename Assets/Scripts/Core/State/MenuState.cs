using UnityEngine.SceneManagement;

public class MenuState : State
{ 
    public static new MenuState Instance
    {
        get
        {
            return (MenuState)State.Instance;
        }
    }


    protected override void Awake()
    {

    }

    protected override void Start()
    {
        if (UIModule.OpenCanvas<MainMenuCanvas>(out var menuCanvas))
        {
            menuCanvas.OpenPanel<MainMenuPanel>();
        }
    }

    public override void Close()
    {
        if (UIModule.TryGetCanvas<MainMenuCanvas>(out var menuCanvas))
        {
            menuCanvas.OpenPanel<MainMenuPanel>();
        }
    }

    public void Play()
    {
        SceneManager.LoadScene(2);
    }

    public void Upgrade()
    {
        //todo upgrade  
        PlayerEntity.Instance.Upgrade();
    }
}
