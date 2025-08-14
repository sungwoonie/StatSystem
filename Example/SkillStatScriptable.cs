using System.Collections;
using System.Collections.Generic;
using StarCloudgamesLibrary;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillStatScriptable", menuName = "Scriptable Stat/SkillStatScriptable")]
public class SkillStatScriptable : StatScriptable, IBaseStat
{
    public EquipBaseStat equipBaseStat;

    public SkillType skillType;
    public float skillCoolDown;
    public int attackCount;

    public string effectName;
    public Sprite icon;

    public EquipBaseStat BaseStat() => equipBaseStat;

    #region "Server Stat"

    public override IServerStat GetServerStat() => equipBaseStat.serverParsingStat;

    public override void InitializeServerStat()
    {
        equipBaseStat.serverParsingStat = new EquipServerStat();
        equipBaseStat.serverParsingStat.statName = statName;
        equipBaseStat.serverParsingStat.initialized = true;
    }

    #endregion

    #region "Set Stat"

    public override void SetServerStat(IServerStat serverStatJson)
    {
        equipBaseStat.serverParsingStat = (EquipServerStat)serverStatJson;
        equipBaseStat.serverParsingStat.initialized = true;
    }

    public override void SetStat(Dictionary<string, string> parsedStat)
    {
        skillType = StringParser.ParseEnum(parsedStat, "SkillType", SkillType.Active);
        equipBaseStat = new EquipBaseStat(parsedStat);
        skillCoolDown = StringParser.ParseFloat(parsedStat, "CoolDown", 0f);
        attackCount = StringParser.ParseInt(parsedStat, "AttackCount", 0);
        effectName = parsedStat.TryGetValue("EffectName", out var _effectName) ? _effectName : "";
    }

    #endregion

    #region "Stat"

    public override double GetStat(StatType targetStat) => equipBaseStat.GetTotalStat(targetStat);

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

    #region "Stat Method"

    public override void GetNewStat()
    {
        if(!GetServerStat().Initialized())
        {
            InitializeServerStat();
        }

        equipBaseStat.serverParsingStat.havingCount++;
    }

    #endregion

    public override Sprite Icon() => icon;
}

[System.Serializable]
public enum SkillType
{
    Active = 1,
    Passive = 2
}