using DG.Tweening;
using System;
using System.Linq;
using UnityEngine;
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

    [SerializeField] private Transform _particleBuild;
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

    public void BuyItem(Enums.ItemType type, Action finish)
    {
        var item = _items.First(_ => _.Type == type);

        if (item != null)
        {
            PlayAnimationBuild(item, finish);
        }
    }

    void PlayAnimationBuild(ItemView item, Action finish)
    {
        item.Invoke();
        item.transform.localScale = Vector3.zero;

        // Небольшой случайный поворот по Z (только для 2D)
        item.transform.localRotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-10f, 10f));

        var seq = DOTween.Sequence();

        // Этап 1: задержка перед появлением
        seq.AppendInterval(0.15f);

        // Этап 2: быстрый "поп" (scale 0 → 1.2)
        seq.Append(item.transform.DOScale(1.2f, 0.1f).SetEase(Ease.OutBack));

        // Этап 3: колебание масштаба (немного меньше → немного больше)
        seq.Append(item.transform.DOScale(0.95f, 0.1f).SetEase(Ease.InOutSine));
        seq.Append(item.transform.DOScale(1.05f, 0.1f).SetEase(Ease.InOutSine));

        // Этап 4: плавное выравнивание масштаба (1.05 → 1.0)
        seq.Append(item.transform.DOScale(1f, 0.1f).SetEase(Ease.OutCubic));

        // Этап 5: поворот в нормальное положение (0 по Z)
        seq.Join(item.transform.DOLocalRotate(Vector3.zero, 0.2f).SetEase(Ease.OutCubic));

        // Этап 6: финальный "пульс"
        seq.Append(item.transform.DOPunchScale(Vector3.one * 0.08f, 0.3f, 4, 0.12f));

        seq.OnComplete(() =>
        {
            finish();
        });
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
