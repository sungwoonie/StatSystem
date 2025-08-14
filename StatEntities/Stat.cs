using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat
{
    public string statName;
    public virtual double GetStat(StatType statType) => 0d;
    public virtual void SetStat(Dictionary<string, string> parsedData) { }
}