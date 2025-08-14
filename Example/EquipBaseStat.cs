using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EquipBaseStat
{
    public BaseStat baseStat;
    public BaseStat beyondStat;
    public StatPrice baseStatPrice;
    public StatPrice beyondStatPrice;
    public EquipServerStat serverParsingStat;

    public Tier tier;
    public int grade;

    #region "Initialize"

    public EquipBaseStat(Dictionary<string, string> parsedData)
    {
        baseStat = new BaseStat(parsedData);
        beyondStat = new BaseStat(parsedData, "Beyond");
        baseStatPrice = new StatPrice(parsedData);
        beyondStatPrice = new StatPrice(parsedData, "Beyond");

        tier = StringParser.ParseEnum(parsedData, "Tier", Tier.None);
        grade = StringParser.ParseInt(parsedData, "Grade", 1);

        serverParsingStat = new EquipServerStat();
    }


    public EquipBaseStat(BaseStat _baseStat, BaseStat _beyondStat, StatPrice _baseStatPrice, StatPrice _beyondStatPrice, Tier _tier, int _grade)
    {
        baseStat = _baseStat;
        beyondStat = _beyondStat;
        baseStatPrice = _baseStatPrice;
        beyondStatPrice = _beyondStatPrice;
        tier = _tier;
        grade = _grade;

        serverParsingStat = new EquipServerStat();
    }

    #endregion

    #region "Boolean"

    public bool IsUnLocked() => true;//serverParsingStat.unLocked;
    public bool IsBeyond() => serverParsingStat.level >= baseStatPrice.GetAllPrice().Count;
    public bool CanBeyond() => beyondStatPrice.GetAllPrice() != null && beyondStatPrice.GetAllPrice().Count > 0;
    public bool IsMaxLevelBeyond()
    {
        return serverParsingStat.beyondLevel >= beyondStatPrice.GetAllPrice().Count;
    }

    public bool IsMaxLevel()
    {
        if (CanBeyond())
        {
            return IsMaxLevelBeyond();
        }

        return serverParsingStat.level >= baseStatPrice.GetAllPrice().Count;
    }

    #endregion

    #region "Stat"

    public double GetTotalStat(StatType targetStatType)
    {
        double result = 0.0f;
        var stat = IsBeyond() ? beyondStat : baseStat;

        if(stat.statType == targetStatType)
        {
            result += GetStat();
        }

        if(serverParsingStat.oneStats != null && serverParsingStat.oneStats.Count > 0)
        {
            result += serverParsingStat.GetOneStatTotalStat(targetStatType);
        }

        return result;
    }

    public double GetNextLevelStat()
    {
        var stat = IsBeyond() ? beyondStat : baseStat;
        var level = IsBeyond() ? serverParsingStat.beyondLevel + 1 : serverParsingStat.level + 1;
        return stat.GetStat(level);
    }

    public double GetStat()
    {
        var stat = CanBeyond() && IsBeyond() ? beyondStat : baseStat;
        var level = CanBeyond() && IsBeyond() ? serverParsingStat.beyondLevel : serverParsingStat.level;
        return stat.GetStat(level);
    }

    public List<OneStat> GetDiceStats() => serverParsingStat.oneStats;

    #endregion

    #region "Price"

    public double GetPrice()
    {
        var price = IsBeyond() ? beyondStatPrice : baseStatPrice;
        var level = IsBeyond() ? serverParsingStat.beyondLevel : serverParsingStat.level;
        return price.GetPrice(level + 1);
    }

    #endregion

    #region "Numerics"

    public int CurrentLevel() => CanBeyond() && IsBeyond() ? serverParsingStat.beyondLevel : serverParsingStat.level;

    #endregion
}