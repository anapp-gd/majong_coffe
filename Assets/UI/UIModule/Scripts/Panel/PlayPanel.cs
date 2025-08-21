using System;

public class PlayPanel : SourcePanel
{
    public override void OnOpen(params Action[] onComplete)
    {
        base.OnOpen(onComplete);

        OpenLayout<ServingLayout>(); 
    }
}
