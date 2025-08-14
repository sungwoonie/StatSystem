using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDiceStat
{
    public List<OneStat> GetDiceStats();
    public void SetDiceStats(List<OneStat> diceStats);
}