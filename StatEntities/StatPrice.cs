using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public struct StatPrice
{
    public double offsetPrice;
    public float priceRatioPerLevel;
    public Dictionary<int, double> priceByStep;
    public Dictionary<int, float> priceRatioPerLevelByStep;

    public StatPrice(Dictionary<string, string> parsedData, string preFix = "")
    {
        offsetPrice = StringParser.ParseDouble(parsedData, $"{preFix}OffsetPrice");
        priceRatioPerLevel = StringParser.ParseFloat(parsedData, $"{preFix}PriceRatioPerLevel");
        priceByStep = parsedData.TryGetValue($"{preFix}StepPrice", out var stepPriceStr) ? StringParser.ParseDoubleDictionary(stepPriceStr) : new Dictionary<int, double>();
        priceRatioPerLevelByStep = parsedData.TryGetValue($"{preFix}PriceRatioStep", out var ratioStepStr) ? StringParser.ParseFloatDictionary(ratioStepStr) : new Dictionary<int, float>();
    }

    public double GetPrice(int level, bool allowZeroLevel = true)
    {
        if(!allowZeroLevel && level <= 0)
            return -1.0f;

        if(priceByStep != null && priceByStep.TryGetValue(level, out var stat))
            return stat;

        return offsetPrice * Mathf.Pow(priceRatioPerLevel, allowZeroLevel ? level : level - 1);
    }

    public double GetPriceByTier(int level, int tier)
    {
        if(priceByStep != null && priceByStep.TryGetValue(tier, out var stat))
        {
            if(priceRatioPerLevelByStep != null && priceRatioPerLevelByStep.TryGetValue(tier, out var ratioPerLevel))
            {
                return stat * Mathf.Pow(ratioPerLevel, level);
            }
            else
            {
                return stat;
            }
        }
        else
        {
            return -1.0f;
        }
    }

    public Dictionary<int, double> GetAllPrice()
    {
        return priceByStep;
    }
}
