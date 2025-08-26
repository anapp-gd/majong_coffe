using UnityEngine;

public class UpgradeLayout : SourceLayout
{
    public override void CloseIt()
    {
        gameObject.SetActive(false);
    }  
}
