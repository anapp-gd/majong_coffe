using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UnityEngine.UI.Button))]
public class ButtonHoldListener : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Tooltip("����������� ��� ������� (PointerDown)")]
    public UnityEvent OnPress;

    [Tooltip("����������� ��� ���������� (PointerUp ��� ����� �������)")]
    public UnityEvent OnRelease;

    [System.Serializable]
    public class HoldEvent : UnityEvent<float> { }

    [Tooltip("���������� ������ ����� ���������, ������� ����� ��������� � ��������")]
    public HoldEvent OnHold;

    [Header("��������� ���������")]
    [Tooltip("����� ������� ���������, ����� �������� ��������� OnHoldComplete (� ��������)")]
    public float holdThreshold = 2f;

    [Tooltip("����������� ���� ���, ����� ��������� �������� holdThreshold")]
    public UnityEvent OnHoldComplete;

    private bool isInteractable;

    private bool isPressed;
    private bool thresholdTriggered;
    private float holdTime;

    public void SetInteractable(bool value)
    {
        isInteractable = value;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isInteractable) return;

        if (!isPressed)
        {
            isPressed = true;
            thresholdTriggered = false;
            holdTime = 0f;
            OnPress?.Invoke();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isInteractable) return;

        Release();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // ���� ������ ���� � ������, ������� ��� �����������
        if (isPressed && eventData.pointerPress == gameObject)
        {
            Release();
        }
    }

    private void Release()
    {
        if (isPressed)
        {
            isPressed = false;
            OnRelease?.Invoke();
        }
    }

    private void Update()
    {
        if (isPressed && isInteractable)
        {
            holdTime += Time.unscaledDeltaTime; // ��� ������� Time.timeScale
            OnHold?.Invoke(holdTime);

            if (!thresholdTriggered && holdTime >= holdThreshold)
            {
                thresholdTriggered = true;
                OnHoldComplete?.Invoke();
            }
        }
    }

    public bool IsPressed => isPressed;
    public float HoldTime => holdTime;

    public void RemoveListeners()
    {
        OnPress?.RemoveAllListeners();
        OnRelease?.RemoveAllListeners();
        OnHold?.RemoveAllListeners();
        OnHoldComplete?.RemoveAllListeners();
    }
}
