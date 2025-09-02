using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FlyIcon : MonoBehaviour
{
    Image _image; 

    private void Awake()
    {
        _image = GetComponent<Image>();    
    }

    public void InvokeFly(Sprite sprite)
    {
        _image.sprite = sprite;
    }

    public void Finish()
    {
        transform.DOPunchScale(Vector3.one * 0.2f, 0.35f, 6, 0.7f).OnComplete(()=> gameObject.SetActive(false));
    }
}
