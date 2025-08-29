using System.Collections;
using UnityEngine;


[CreateAssetMenu(fileName = "InterfaceConfig", menuName = "Config/InterfaceConfig")]
public class InterfaceConfig : Config
{
    public Sprite SoundOn;
    public Sprite SoundOff;
    public Sprite MusicOn;
    public Sprite MusicOff;
    public Sprite VibroOn;
    public Sprite VibroOff;
    public Sprite ButtonOn;
    public Sprite ButtonOff;
    public Sprite BuyButton;
    public Sprite BuyedButton;
    public Sprite CantBuyButton;
    public override IEnumerator Init()
    {
        yield return null;
    } 
}
