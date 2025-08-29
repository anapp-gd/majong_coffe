using DG.Tweening;

using System;
using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public abstract class SourcePanel : MonoBehaviour
{
    public Ease EaseOpen = Ease.InBack;
    public Ease EaseClose = Ease.OutBack;

    public float DurationAnimate = 0.5f;

    protected List<SourceWindow> _windows;
    protected List<SourceLayout> _layouts;

    protected CanvasGroup _canvasGroup;
    protected RectTransform _rectTransform;
    protected SourceCanvas _sourceCanvas;
    protected Sequence _sequenceHide;
    protected Sequence _sequenceShow;

    public bool isOpenOnInit;
    public bool isAlwaysOpen;
    [HideInInspector] public bool isOpen;

    public virtual void Init(SourceCanvas canvasParent)
    {
        _sourceCanvas = canvasParent;
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _windows = new List<SourceWindow>();
        _layouts = new List<SourceLayout>();

        var windows = GetComponentsInChildren<SourceWindow>(true);
        var layouts = GetComponentsInChildren<SourceLayout>(true);

        for (int i = 0; i < windows.Length; i++)
        {
            _windows.Add(windows[i].Init(this));
        }

        for (int i = 0; i < layouts.Length; i++)
        {
            _layouts.Add(layouts[i].Init(this));
        }

        isOpen = false;

        if (!isOpenOnInit) gameObject.SetActive(false);
    } 

    public virtual void OnInject()
    {

    }

    public virtual T OpenWindow<T>() where T : SourceWindow
    {
        SourceWindow returnedWindow = null;

        foreach (var sourceWindow in _windows)
        {
            if (sourceWindow is T panel)
            {
                returnedWindow = panel;
            }
            else
            {
                sourceWindow.OnClose();
            }
        }

        returnedWindow.OnOpen();

        return returnedWindow as T;
    }
    public virtual T CloseWindow<T>() where T : SourceWindow
    {
        SourceWindow returnedWindow = null;

        foreach (var sourceWindow in _windows)
        {
            if (sourceWindow is T panel)
            {
                returnedWindow = panel;
            }
        }

        returnedWindow.OnClose();

        return returnedWindow as T;
    }

    public virtual T GetWindow<T>() where T : SourceWindow
    {
        SourceWindow returnedWindow = null;

        foreach (var sourceWindow in _windows)
        {
            if (sourceWindow is T window)
            {
                returnedWindow = window;
            }
        }

        return returnedWindow as T; 
    }


    public virtual T GetLayout<T>() where T : SourceLayout
    {
        SourceLayout returnedLayout = null;

        foreach (var sourceLayout in _layouts)
        {
            if (sourceLayout is T layout)
            {
                returnedLayout = layout;
            } 
        } 

        return returnedLayout as T;
    }
    public virtual T OpenLayout<T>() where T : SourceLayout
    {
        SourceLayout returnedLayout = null;

        foreach (var sourceLayout in _layouts)
        {
            if (sourceLayout is T panel)
            {
                returnedLayout = panel;
            }
            else
            {
                sourceLayout.OnClose();
            }
        }

        returnedLayout.OnOpen();

        return returnedLayout as T;
    }
    public virtual T CloseLayout<T>() where T : SourceLayout
    {
        SourceLayout returnedLayout = null;

        foreach (var sourceWindow in _layouts)
        {
            if (sourceWindow is T panel)
            {
                returnedLayout = panel;
            }
        }

        returnedLayout.OnClose();

        return returnedLayout as T;
    }
    public virtual void OnOpen(params Action[] onComplete)
    {
        gameObject.SetActive(true);

        if (isOpen) return;

        Show(onComplete);
    }
    protected virtual Action[] AddCallback(Action[] originalCallbacks, params Action[] additionalCallbacks)
    {
        if (additionalCallbacks == null || additionalCallbacks.Length == 0)
            return originalCallbacks;

        var combined = new Action[originalCallbacks.Length + additionalCallbacks.Length];
        originalCallbacks.CopyTo(combined, 0);
        additionalCallbacks.CopyTo(combined, originalCallbacks.Length);
        return combined;
    }
    protected virtual void Show(params Action[] onComplete)
    {
        gameObject.SetActive(true);
        _canvasGroup.alpha = 0f;
        _rectTransform.localScale = Vector3.one * 1.1f;

        // Анимация появления
        _sequenceShow = DOTween.Sequence();
        _sequenceShow.SetUpdate(UpdateType.Normal, true);
        _sequenceShow.Append(_canvasGroup.DOFade(1f, DurationAnimate));
        _sequenceShow.Join(_rectTransform.DOScale(1f, DurationAnimate).SetEase(EaseOpen)).OnComplete(() =>
        {
            foreach (var action in onComplete)
            {
                action?.Invoke(); // Вызываем колбэк
            }

            isOpen = true;
        });
    }
    public virtual void OnCLose(params Action[] onComplete)
    {
        if (isOpen) Hide(onComplete);
    }
    protected virtual void Hide(params Action[] onComplete)
    { 
        _sequenceHide = DOTween.Sequence();
        _sequenceHide.SetUpdate(UpdateType.Normal, true);
        _sequenceHide.Append(_canvasGroup.DOFade(0f, DurationAnimate));
        _sequenceHide.Join(_rectTransform.DOScale(1.1f, DurationAnimate).SetEase(EaseClose))
            .OnComplete(() =>
            {
                if (onComplete.Length > 0)
                {
                    foreach (var action in onComplete)
                    {
                        action?.Invoke();
                    }
                }

                foreach (var layout in _layouts)
                {
                    layout.OnClose();
                }

                foreach (var window in _windows)
                {
                    window.OnClose();
                }

                gameObject.SetActive(false);
                isOpen = false;
            });
    }
    public virtual void OnDispose()
    {
        for (int i = 0; i < _layouts.Count; i++)
        {
            _layouts[i].Dispose();
        }

        for (int i = 0; i < _windows.Count; i++)
        {
            _windows[i].Dispose();
        }

        _sequenceHide?.Kill();
        _sequenceShow?.Kill();
    }
}
