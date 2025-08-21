using UnityEngine;

public class InfoLayout : SourceLayout
{ 
    public override void CloseIt()
    {
        gameObject.SetActive(false);
    }
}
