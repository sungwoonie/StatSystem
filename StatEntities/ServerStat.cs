using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class ServerStat : IServerStat
{
    public string statName;

    [JsonIgnore] public bool initialized;

    public virtual string GetStatToJson()
    {
        return JsonConvert.SerializeObject(this, Formatting.None);
    }

    public bool Initialized() => initialized;

    public virtual void Reset()
    {
    }

    public string StatName() => statName;
}