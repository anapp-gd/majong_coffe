using UnityEngine;

public abstract class SourceLayout : MonoBehaviour
{
    protected SourcePanel _panel;
    protected SourceSlot[] _slots; 

    public virtual SourceLayout Init(SourcePanel panel)
    {
        _panel = panel;

        var slots = GetComponentsInChildren<SourceSlot>(true);
        _slots = new SourceSlot[slots.Length];

        for (int i = 0; i < slots.Length; i++) _slots[i] = slots[i].Init(this);

        gameObject.SetActive(false);

        return this;
    } 

    public virtual void OnInject()
    {

    }

    public virtual void OnActive()
    {
        foreach (var slot in _slots)
        {
            slot.OnActive();
        }
    }

    public virtual T GetSourcePanel<T>() where T : SourcePanel
    {
        return _panel as T;
    }

    public abstract void CloseIt();

    public virtual void OnOpen()
    {
        gameObject.SetActive(true);
    }

    public virtual void OnClose()
    {
        gameObject.SetActive(false);
    }

    public virtual void Dispose()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            _slots[i].Dispose();
        }
    }
}
