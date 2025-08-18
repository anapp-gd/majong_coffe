using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Config/LevelConfig")]
public class LevelConfig : Config
{
    public List<LevelData> _levels;

    private Dictionary<int, LevelData> _dictionary;
    public override IEnumerator Init()
    {
        _dictionary = new Dictionary<int, LevelData>();

        for (int i = 0; i < _levels.Count; i++)
        {
            _dictionary.Add(i, _levels[i]);
        } 

        yield return null;
    }

    public bool TryGetLevelData(int id, out LevelData data)
    {
        data = null;

        if (_dictionary.TryGetValue(id, out data))
        {
            return true;
        }

        return false;
    }
}
