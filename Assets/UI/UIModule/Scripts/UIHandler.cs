using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class UIHandler : MonoBehaviour
{
    [SerializeField] private AudioClip _ambient;
    private AudioSource _audioSource;
    private List<SourceCanvas> _canvases = new();
    private List<SourcePanel> _panels = new();
    private List<SourceLayout> _layouts = new();
    private List<SourceWindow> _windows = new();
    private List<SourceSlot> _slots = new();

    private void Awake()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.clip = _ambient;
        _audioSource.loop = true;
        _audioSource.volume = 1f;
        _audioSource.playOnAwake = false;
    }

    public void Play()
    {
        if (!_audioSource.isPlaying && PlayerEntity.Instance.IsMusic)
            _audioSource.Play();
        else
            _audioSource.Stop();
    } 

    public void Init()
    {
        _canvases = GetComponentsInChildren<SourceCanvas>(true).ToList();
        _panels = GetComponentsInChildren<SourcePanel>(true).ToList();
        _layouts = GetComponentsInChildren<SourceLayout>(true).ToList();
        _windows = GetComponentsInChildren<SourceWindow>(true).ToList();
        _slots = GetComponentsInChildren<SourceSlot>(true).ToList();
        _canvases = new List<SourceCanvas>();

        var canveses = GetComponentsInChildren<SourceCanvas>(true);

        for (int i = 0; i < canveses.Length; i++)
        {
            _canvases.Add(canveses[i]);
        }

        _canvases.ForEach(canvas => canvas.Init());

        Play();
    }

    public void Inject(params object[] data) 
    {
        InjectList(_canvases, data);
        InjectList(_panels, data);
        InjectList(_layouts, data);
        InjectList(_windows, data);
        InjectList(_slots, data);

        foreach (var canvas in _canvases)
            canvas.OnInject();
        foreach (var panel in _panels) 
            panel.OnInject();
        foreach (var layout in _layouts)
            layout.OnInject();
        foreach (var window in _windows) 
            window.OnInject();
        foreach (var slot in _slots) 
            slot.OnInject();
    }

    public void Dispose()
    {
        _canvases.ForEach(_canvas => _canvas.Dispose());
    }

    public virtual bool InvokeCanvas<T>(out T canvas) where T : SourceCanvas
    {
        canvas = null;

        foreach (var c in _canvases)
        {
            if (c is T returned)
            {
                canvas = returned;
            }
            else
            {
                c.CloseCanvas();
            }
        }

        canvas?.InvokeCanvas();

        return canvas != null;
    }

    public virtual bool CloseCanvas<T>(out T canvas) where T : SourceCanvas
    {
        canvas = null;
         
        foreach (var c in _canvases)
        {
            if (c is T closed)
            {
                canvas = closed;
            } 
        }

        canvas.CloseCanvas();

        return canvas != null;
    }

    public virtual bool TryGetCanvas<T>(out T canvas) where T : SourceCanvas
    {
        canvas = null;

        foreach (var c in _canvases)
        {
            if (c is T returned)
            {
                canvas = returned; 
                break;
            }
        }

        return canvas != null;
    }
    static void InjectList(IEnumerable<object> targets, params object[] sources)
    {
        foreach (var target in targets)
        {
            var fields = target.GetType()
                               .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            { 
                if (field.GetCustomAttribute<UIInjectAttribute>() == null)
                    continue;

                // Попробовать найти подходящий source
                object match = null;
                foreach (var source in sources)
                {
                    if (source == null) continue;

                    if (field.FieldType.IsAssignableFrom(source.GetType()))
                    {
                        match = source;
                        break;
                    }
                }

                if (match == null) continue;

                try
                {
                    field.SetValue(target, match);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UIInject] Ошибка инъекции в {target.GetType().Name}.{field.Name}: {e.Message}");
                }
            }
        }
    }
}
