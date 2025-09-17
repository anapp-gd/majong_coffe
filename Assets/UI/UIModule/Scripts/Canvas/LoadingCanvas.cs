using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingCanvas : SourceCanvas
{
    [SerializeField] private Text text; // или Text, если без TMP
    [SerializeField] private float delay = 0.25f;

    Coroutine coroutine;

    private string[] loadingStates =
    {
        "Loading   ",
        "Loading.  ",
        "Loading.. ",
        "Loading..."
    };

    public override void InvokeCanvas()
    {
        base.InvokeCanvas();

        if(coroutine == null) coroutine = StartCoroutine(AnimateText());
    } 

    private IEnumerator AnimateText()
    {
        int index = 0;
        while (true)
        {
            text.text = loadingStates[index];
            index = (index + 1) % loadingStates.Length;
            yield return new WaitForSeconds(delay);
        }
    }

    public override void CloseCanvas()
    {
        base.CloseCanvas();

        if (coroutine != null)
        {
            StopCoroutine(coroutine);

            coroutine = null;
        }
    } 
}
