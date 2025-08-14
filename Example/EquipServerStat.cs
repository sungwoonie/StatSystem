using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Serialization;

public class EquipServerStat : ServerStat
{
    public int level;
    public int beyondLevel;
    public int havingCount;
    public bool unLocked;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<OneStat> oneStats;

    public override string GetStatToJson()
    {
        if (oneStats != null && oneStats.Count > 0 == false)
        {
            oneStats = null;
        }

        return JsonConvert.SerializeObject(this, Formatting.None);
    }

    public double GetOneStatTotalStat(StatType statType)
    {
        double result = 0f;

        foreach (OneStat stat in oneStats)
        {
            if (stat.statType == statType)
            {
                result += stat.stat;
            }
        }

        return result;
    }
}