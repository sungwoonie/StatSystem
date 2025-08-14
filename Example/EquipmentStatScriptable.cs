using System.Collections;
using System.Collections.Generic;
using StarCloudgamesLibrary;
using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentStatScriptable", menuName = "Scriptable Stat/EquipmentStatScriptable")]
public class EquipmentStatScriptable : StatScriptable, IDiceStat, IBaseStat
{
    [SerializeField] private EquipBaseStat equipBaseStat;

    public EquipBaseStat BaseStat() => equipBaseStat;
    public Sprite icon;
    public EquipmentType equipmentType;
    public BaseStat llTierEffect;

    private const int mergeCount = 4;
    private const int maxGrade = 4;

    #region "Set"

    public override void SetStat(Dictionary<string, string> parsedData)
    {
        equipmentType = StringParser.ParseEnum(parsedData, "EquipmentType", EquipmentType.Weapon);

        equipBaseStat = new EquipBaseStat(parsedData);
        llTierEffect = new BaseStat(parsedData, "LLTier");
    }

    public override void SetServerStat(IServerStat serverStatJson)
    {
        equipBaseStat.serverParsingStat = (EquipServerStat)serverStatJson;
        equipBaseStat.serverParsingStat.initialized = true;
    }

    #endregion

    #region "Server Stat"

    public override IServerStat GetServerStat() => equipBaseStat.serverParsingStat;

    public override void InitializeServerStat()
    {
        equipBaseStat.serverParsingStat = new EquipServerStat();
        equipBaseStat.serverParsingStat.statName = statName;
        equipBaseStat.serverParsingStat.initialized = true;
    }

    #endregion

    #region "Stat Method"

    public double GetLLTierEffect()
    {
        if (llTierEffect.statType != StatType.none && ((int)equipBaseStat.tier >= (int)Tier.LL))
        {
            return llTierEffect.GetStat(0);
        }

        return 0.0f;
    }

    public override void GetNewStat()
    {
        if(!GetServerStat().Initialized())
        {
            InitializeServerStat();
        }

        equipBaseStat.serverParsingStat.havingCount++;
        equipBaseStat.serverParsingStat.unLocked = true;
    }

    public override double GetStat(StatType targetStatType)
    {
        return BaseStat().GetTotalStat(targetStatType) + GetLLTierEffectStat(targetStatType);
    }

    public double GetLLTierEffectStat(StatType statType)
    {
        if(llTierEffect.statType == statType && ((int)equipBaseStat.tier >= (int)Tier.LL))
        {
            return llTierEffect.GetStat(0);
        }

        return 0.0d;
    }

    #endregion

    #region "Merge"

    public bool CanMerge() => true;//BaseStat().IsUnLocked() && BaseStat().tier < Tier.LL && BaseStat().serverParsingStat.havingCount >= mergeCount;

    public void Merge()
    {
        BaseStat().serverParsingStat.havingCount -= mergeCount;

        var baseStat = BaseStat();
        bool isGradeMerge = baseStat.grade < maxGrade;

        var targetGrade = isGradeMerge ? baseStat.grade + 1 : 1;
        var targetTier = isGradeMerge ? baseStat.tier : baseStat.tier + 1;

        string statName = $"{equipmentType.ToString().ToLower()}_{(int)targetTier}_{targetGrade}";
        var targetStat = (EquipmentStatScriptable)ScriptableStatManager.instance.GetStat(ScriptableStatType.EquipmentStat, statName);

        if(targetStat == null)
        {
            Debug.LogWarning($"[Merge Failed] Stat not found: {statName}");
            return;
        }

        targetStat.GetNewStat();
    }

    public void MergeAll()
    {
        var allMergeCount = BaseStat().serverParsingStat.havingCount / mergeCount;
        for(var i = 0; i < allMergeCount; i++)
        {
            Merge();
        }
    }

    #endregion

    #region "Enhancement"

    private RewardID CurrentCurrencyID() => equipBaseStat.IsBeyond() ? RewardID.beyondStone : RewardID.skillStone;

    public void Enhance()
    {
        UserDatabaseController.instance.UseCurrency(CurrentCurrencyID(), equipBaseStat.GetPrice());

        if(!GetServerStat().Initialized())
        {
            InitializeServerStat();
        }

        if(equipBaseStat.IsBeyond())
        {
            equipBaseStat.serverParsingStat.beyondLevel++;
            equipBaseStat.beyondStat.UpdateStat();
        }
        else
        {
            equipBaseStat.serverParsingStat.level++;
            equipBaseStat.baseStat.UpdateStat();
        }
    }

    public bool CanEnhance()
    {
        if(!equipBaseStat.IsUnLocked())
        {
            return false;
        }


        if(equipBaseStat.IsMaxLevelBeyond())
        {
            return false;
        }

        return UserDatabaseController.instance.CanUseCurrency(CurrentCurrencyID(), equipBaseStat.GetPrice());
    }

    #endregion

    #region "Dice Stat"

    public List<OneStat> GetDiceStats()
    {
        return BaseStat().GetDiceStats();
    }

    public void SetDiceStats(List<OneStat> diceStats)
    {
        if(!GetServerStat().Initialized())
        {
            InitializeServerStat();
        }

        BaseStat().serverParsingStat.oneStats = diceStats;
    }

    #endregion

    public override Sprite Icon() => icon;
}

[System.Serializable]
public enum EquipmentType
{
    None = 0,
    Weapon = 1,
    Cape = 2,
    Armor = 3
}