using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct OneStat
{
    public StatType statType;
    public Tier tier;
    public double stat;

    public OneStat(Dictionary<string, string> data, string preFix = "")
    {
        statType = StringParser.ParseEnum(data, $"{preFix}StatType", StatType.none);
        tier = Tier.None;
        stat = StringParser.ParseDouble(data, $"{preFix}Stat", 0);
    }

    public OneStat(string _tier, Dictionary<string, string> data, string preFix = "")
    {
        statType = StringParser.ParseEnum(data, $"{preFix}StatType", StatType.none);
        tier = tier = (Tier)int.Parse(_tier);
        stat = StringParser.ParseDouble(data, $"{preFix}Stat", 0);
    }
}