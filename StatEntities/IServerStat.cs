using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IServerStat
{
    public string StatName();
    public string GetStatToJson();
    public bool Initialized();
    public void Reset();
}