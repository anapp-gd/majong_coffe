using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Config/GameConfig")]
public class GameConfig : Config
{
    public float ClientSpawnDelay;
    public float ClientTakeDelay;
    public int MaxClientCount;
    public int MaxDishCount;

    public override IEnumerator Init()
    {

        yield return null;
    }
}
