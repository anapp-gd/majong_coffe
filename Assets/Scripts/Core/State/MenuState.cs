using System.Linq;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.UI;
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

    private ItemView[] _items;

    protected override void Awake()
    {
        _items = FindObjectsByType<ItemView>(UnityEngine.FindObjectsSortMode.None);

        foreach (var item in _items)
        {
            item.gameObject.SetActive(false);
        }
    }

    protected override void Start()
    {
        if (UIModule.OpenCanvas<MainMenuCanvas>(out var menuCanvas))
        {
            menuCanvas.OpenPanel<MainMenuPanel>();
        }

        var playerEntity = PlayerEntity.Instance;

        var list = playerEntity.Data;

        for (int i = 0; i < list.Count; i++)
        {
            var item = _items.First(_ => _.Type == list[i].Type);

            if (item != null)
            {
                item.gameObject.SetActive(true);
            }
        } 
    }

    public void BuyItem(Enums.ItemType type)
    {
        var item = _items.First(_ => _.Type == type);

        if (item != null)
        {
            item.gameObject.SetActive(true);
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
}
