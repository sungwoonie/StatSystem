using System.Collections;
using System.Collections.Generic;
using StarCloudgamesLibrary;
using UnityEngine;

public class NotScriptableStatManager : SingleTon<NotScriptableStatManager>
{
    private StatLoader<DiceStat> diceStatLoader;
    private StatLoader<RingElementalEffectStat> ringElementalEffectStatLoader;

    #region "Unity"

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    #endregion

    #region "Initialize"

    private void Initialize()
    {
        diceStatLoader = new StatLoader<DiceStat>("DiceStat");
        ringElementalEffectStatLoader = new StatLoader<RingElementalEffectStat>("RingElementalEffectStat");
    }

    #endregion

    #region "Dice Stat"

    public List<OneStat> GetNewRandomDiceStats(int count)
    {
        var result = new List<OneStat>(count);
        var loadedDiceStats = diceStatLoader.GetData();

        for(int i = 0; i < count; i++)
        {
            var newDiceStat = loadedDiceStats[Random.Range(0, loadedDiceStats.Count)];
            var randomTier = GachaTableManager.instance.GetDiceTier();
            var resultTier = newDiceStat.GetDiceStat(randomTier);

            result.Add(resultTier);
        }

        return result;
    }

    #endregion

    #region "Ring Elemental Effect"

    public List<RingElementalEffectStat> GetAllRingElementalEffectStat() => ringElementalEffectStatLoader.GetData();

    public double GetRingElementalEffectStat(StatType statType)
    {
        var statDatas = GetAllRingElementalEffectStat();
        var targetStat = statDatas.Find(x => x.statType == statType);
        if(targetStat == null || !UserDatabaseController.instance.CanActivate(targetStat.elementalType, out var equippedCount)) return 0d;
        return targetStat.GetStat(equippedCount).stat;
    }

    public OneStat GetRingElementalEffectStat(ElementalType elementalType, int equippedCount)
    {
        var effectList = GetAllRingElementalEffectStat();
        foreach(var effect in effectList)
        {
            if(effect.elementalType == elementalType)
            {
                return effect.GetStat(equippedCount);
            }
        }

        return default;
    }

    #endregion
}