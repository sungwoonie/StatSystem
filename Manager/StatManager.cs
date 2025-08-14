using StarCloudgamesLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatManager : SingleTon<StatManager>
{
    private Dictionary<ScriptableStatType, List<IStat>> stats;
    private readonly List<ScriptableStatType> equippableStatTypes = new List<ScriptableStatType>
    {
        ScriptableStatType.EquipmentStat,
        ScriptableStatType.RelicStat,
        ScriptableStatType.SkillStat,
        ScriptableStatType.TitleStat,
        ScriptableStatType.PetStat
    };

    #region "Unity"

    protected override void Awake()
    {
        base.Awake();
        InitializeStats();
    }

    #endregion

    #region "Initialize"

    private void InitializeStats()
    {
        stats = new Dictionary<ScriptableStatType, List<IStat>>();

        var currentStats = ScriptableStatManager.instance.GetAllStats();
        foreach(var stat in currentStats)
        {
            if(equippableStatTypes.Contains(stat.Key))
            {
                continue;
            }

            stats[stat.Key] = stat.Value;
        }

        InitializeEquippableStats();
    }

    private void InitializeEquippableStats()
    {
        foreach(var scriptableStatType in equippableStatTypes)
        {
            stats[scriptableStatType] = UserDatabaseController.instance.GetEquippedEquippableStats(scriptableStatType);
        }
    }

    #endregion

    #region "Get Calculated Stat"

    public double GetFinalNormalAttackDamage(out bool isCritical)
    {
        var strikingStat = GetStat(ScriptableStatType.TrainingStat, StatType.strikingPower);
        var normalAttackDamage = GetStat(ScriptableStatType.None, StatType.normalAttackDamage);
        var finalDamage = GetStat(ScriptableStatType.None, StatType.finalDamage);
        var result = strikingStat * (1 + normalAttackDamage / 100.0d) * (1 + finalDamage / 100.0d);
        return IsCriticalHitDamage(result, out isCritical);
    }

    public double GetFinalSkillAttackDamage(out bool isCritical)
    {
        var strikingStat = GetStat(ScriptableStatType.TrainingStat, StatType.strikingPower);
        var skillAttackDamage = GetStat(ScriptableStatType.None, StatType.skillDamage);
        var finalDamage = GetStat(ScriptableStatType.None, StatType.finalDamage);
        var result = strikingStat * (1 + skillAttackDamage / 100.0d) * (1 + finalDamage / 100.0d);
        return IsCriticalHitDamage(result, out isCritical);
    }

    #endregion

    #region "Critical"

    private double IsCriticalHitDamage(double damage, out bool isCritical)
    {
        isCritical = IsCriticalHit();

        if(isCritical)
        {
            var criticalDamage = GetStat(ScriptableStatType.TrainingStat, StatType.criticalDamage);
            return damage * (1 + criticalDamage / 100.0d);
        }

        return damage;
    }

    private bool IsCriticalHit()
    {
        var criticalChance = GetStat(ScriptableStatType.TrainingStat, StatType.criticalChance);
        return UnityEngine.Random.Range(0.0f, 100.0f) < criticalChance;
    }

    #endregion

    #region "Get Stat"

    public double GetStat(ScriptableStatType baseStatType, StatType targetStatType)
    {
        var percentageStat = 0.0d;
        foreach (var scriptableStatType in ScriptableStatManager.AllStatTypes)
        {
            if (scriptableStatType == baseStatType ||scriptableStatType == ScriptableStatType.None) continue;

            percentageStat += GetStatInScriptableStatType(scriptableStatType, targetStatType);
        }

        percentageStat += NotScriptableStatManager.instance.GetRingElementalEffectStat(targetStatType);

        if(baseStatType != ScriptableStatType.None)
        {
            return GetStatInScriptableStatType(baseStatType, targetStatType) * (1 + percentageStat / 100.0d);
        }
        else
        {
            return percentageStat;
        }
    }

    private double GetStatInScriptableStatType(ScriptableStatType scriptableStatType, StatType statType)
    {
        var result = 0.0d;

        if(stats.TryGetValue(scriptableStatType, out var statList) && statList != null && statList.Count > 0)
        {
            foreach(var stat in statList)
            {
                result += stat.GetStat(statType);
            }
        }

        return result;
    }

    #endregion

    #region "Set Equippable Stat"

    public void SetEquippedRelicStats(List<IStat> relicStats)
    {
        stats[ScriptableStatType.RelicStat] = relicStats;
    }

    public void SetEquippedPassiveSkills(List<IStat> passiveSkills)
    {
        stats[ScriptableStatType.SkillStat] = passiveSkills;
    }

    public void SetEquippedEquipments(List<IStat> equipped)
    {
        stats[ScriptableStatType.EquipmentStat] = equipped;
    }

    #endregion
}