using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[System.Serializable]
public struct BaseStat
{
    public StatType statType;
    public double offsetStat;
    public float statRatioPerLevel;
    public Dictionary<int, double> statByStep;
    public Dictionary<int, float> statRatioPerLevelByStep;

    private double currentStat;
    private bool update;

    #region "Initialize"

    public BaseStat(Dictionary<string, string> parsedData, string preFix = "")
    {
        currentStat = -1d;
        update = true;

        statType = (StatType)StringParser.ParseInt(parsedData, $"{preFix}StatType", 0);
        offsetStat = StringParser.ParseDouble(parsedData, $"{preFix}OffsetStat");
        statRatioPerLevel = StringParser.ParseFloat(parsedData, $"{preFix}StatRatioPerLevel");

        statByStep = parsedData.TryGetValue($"{preFix}StepStat", out var stepStatStr) ? StringParser.ParseDoubleDictionary(stepStatStr) : new Dictionary<int, double>();
        statRatioPerLevelByStep = parsedData.TryGetValue($"{preFix}StatRatioStep", out var statRatioStepStr) ? StringParser.ParseFloatDictionary(statRatioStepStr) : new Dictionary<int, float>();
    }

    #endregion

    #region "Get Stat"

    public bool CanStatByStep()
    {
        return statByStep != null && statByStep.Count > 0;
    }

    public double GetStat(int level, bool allowZeroLevel = true)
    {
        if(!allowZeroLevel && level <= 0)
        {
            return 0f;
        }

        if(!update)
        {
            return currentStat;
        }

        update = false;

        if(statByStep != null && statByStep.TryGetValue(level, out var stat))
        {
            currentStat = stat;
            return currentStat;
        }

        currentStat = offsetStat * Math.Pow(statRatioPerLevel, allowZeroLevel ? level : level - 1);
        return currentStat;
    }

    public double GetStatByTier(int level, int tier, bool allowZeroLevel = true)
    {
        if(level <= 0 && !allowZeroLevel)
        {
            return 0f;
        }

        if(!update)
        {
            return currentStat;
        }

        update = false;

        if(statByStep != null && statByStep.TryGetValue(tier, out var stat))
        {
            if(statRatioPerLevelByStep != null && statRatioPerLevelByStep.TryGetValue(tier, out var ratioPerLevel))
            {
                currentStat = stat * Mathf.Pow(ratioPerLevel, allowZeroLevel ? level : level - 1);
                return currentStat;
            }
            else
            {
                currentStat = stat;
                return currentStat;
            }
        }
        else
        {
            Debug.LogError("Stat per tier is not exist");
            return 0;
        }
    }

    #endregion

    #region "Get Stat By Level"

    public double GetLevelStat(int level, bool allowZeroLevel = true)
    {
        if(!allowZeroLevel && level <= 0)
        {
            return 0f;
        }

        if (statByStep != null && statByStep.TryGetValue(level, out var stat))
        {
            return stat;
        }

        return offsetStat * Math.Pow(statRatioPerLevel, allowZeroLevel ? level : level - 1);
    }

    public double GetLevelStatByTier(int level, int tier, bool allowZeroLevel = true)
    {
        if(!allowZeroLevel && level <= 0)
        {
            return 0f;
        }

        if (statByStep != null && statByStep.TryGetValue(tier, out var stat))
        {
            if (statRatioPerLevelByStep != null && statRatioPerLevelByStep.TryGetValue(tier, out var ratioPerLevel))
            {
                return stat * Mathf.Pow(ratioPerLevel, allowZeroLevel ? level : level - 1);
            }
            else
            {
                return stat;
            }
        }
        else
        {
            Debug.LogError("Stat per tier is not exist");
            return -1d;
        }
    }

    #endregion

    public void UpdateStat()
    {
        update = true;
    }
}