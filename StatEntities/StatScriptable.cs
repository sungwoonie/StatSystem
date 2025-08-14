using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatScriptable : ScriptableObject, IStat
{
    public string statName;
    public string StatName() => statName;
    public virtual void SetStatName(string statName)
    {
        this.statName = statName;
    }

    public virtual IServerStat GetServerStat() => null;

    public virtual void InitializeServerStat(){}

    public virtual void SetServerStat(IServerStat serverStatJson){}

    public virtual void SetStat(Dictionary<string, string> parsedStat){}

    public virtual void SetStat(Dictionary<string, Dictionary<string, string>> parsedStat){}

    public virtual double GetStat(StatType targetStat) => 0d;
    public virtual void GetNewStat() { }
    public virtual Sprite Icon() => null;
}