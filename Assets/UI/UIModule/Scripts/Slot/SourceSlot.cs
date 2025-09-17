using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


public abstract class SourceSlot : MonoBehaviour
{
    public bool IsButton;
    public bool EnableIcon = true;
    protected Image _icon;
    protected Image _background;
    protected SourceLayout _layout;
    protected Button _btnClick; 
    protected Animation _loading;

    public virtual SourceSlot Init(SourceLayout layout)
    {
        _layout = layout;

        if (IsButton)
        {
            if (TryGetComponent<Button>(out var button))
            {
                _btnClick = button;
            }
            else
            {
                _btnClick = gameObject.AddComponent<Button>();
            }

            _btnClick.onClick.AddListener(OnClick);
        }

        _icon = transform.GetChild(0).GetComponent<Image>();
        if(_icon && !EnableIcon) _icon.enabled = false;

        _loading = GetComponentInChildren<Animation>();

        _background = GetComponent<Image>();

        return this;
    } 
    public virtual void OnInject()
    {

    }
    public abstract void OnActive();
    public abstract void OnClick();
    public abstract void UpdateView();
    public virtual void Clear()
    {
        _btnClick.interactable = false;
    }
    public virtual void Dispose()
    {
        _btnClick?.onClick.RemoveListener(OnClick);
    } 
}
