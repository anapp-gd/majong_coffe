public class MainMenuCanvas : SourceCanvas
{
    public override void InvokeCanvas()
    {
        base.InvokeCanvas();

        OpenPanel<MainMenuPanel>();
    }
}
