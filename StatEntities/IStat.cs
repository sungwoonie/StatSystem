using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStat
{
    public string StatName();
    public void SetStat(Dictionary<string, string> parsedStat);
    public void SetStat(Dictionary<string, Dictionary<string, string>> parsedStat);
    public void SetStatName(string statName);
    public abstract void SetServerStat(IServerStat serverStatJson);
    public abstract IServerStat GetServerStat();
    public abstract void InitializeServerStat();
    public double GetStat(StatType targetStat);
    public void GetNewStat();
    public Sprite Icon();
}