using System.Collections.Generic;
using UnityEngine;

public class WinConditions
{
    private Dictionary<WinCondition, bool> conditions;

    public WinConditions(IEnumerable<WinCondition> conditionsList)
    {
        conditions = new Dictionary<WinCondition, bool>();
        foreach (var c in conditionsList)
            conditions[c] = false;
    }

    public void SetCompleted(WinCondition condition, bool value = true)
    {
        if (conditions.ContainsKey(condition))
            conditions[condition] = value;

        if (IsVictory()) PlayState.Instance.Win(); 
    }

    bool IsVictory()
    {
        foreach (var kvp in conditions)
            if (!kvp.Value) return false;
        return true;
    }
}

public enum WinCondition
{
    RemoveAllTiles,
    TableClear
}
