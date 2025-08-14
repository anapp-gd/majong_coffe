using UnityEngine;

public abstract class SourceWindow : MonoBehaviour
{
    protected SourcePanel _panel;
    protected object _data;

    public virtual SourceWindow Init(SourcePanel panel)
    {
        _panel = panel;
        gameObject.SetActive(false);
        return this;
    }

    public void OnInject()
    {

    }

    public virtual void OnOpen()
    {
        gameObject.SetActive(true);
    }

    public virtual void OnClose()
    {
        gameObject.SetActive(false);
    }

    public abstract void Dispose();
}
