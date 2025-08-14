using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using StarCloudgamesLibrary;
using UnityEngine;

public class ScriptableStatManager : SingleTon<ScriptableStatManager>
{
    private readonly Dictionary<ScriptableStatType, List<IStat>> stats = new();
    private const string resourceFolderPath = "ScriptableStats";

    public static readonly ScriptableStatType[] AllStatTypes = (ScriptableStatType[])Enum.GetValues(typeof(ScriptableStatType));

    #region "Unity"

    protected override void Awake()
    {
        base.Awake();

        InitializeScriptableStats();
    }

    #endregion

    #region "Server Stat"

    public void SetServerStats(Dictionary<ScriptableStatType, List<string>> serverDatas)
    {
        if(serverDatas == null || serverDatas.Count == 0)
        {
            Debug.LogWarning("서버 데이터가 비어있습니다.");
            return;
        }

        foreach(var stat in serverDatas)
        {
            foreach(var statJson in stat.Value)
            {
                IServerStat parsed = stat.Key switch
                {
                    ScriptableStatType.ADBuffStat => JsonConvert.DeserializeObject<ADBuffServerStat>(statJson),
                    ScriptableStatType.AdvancementStat => JsonConvert.DeserializeObject<AdvancementServerStat>(statJson),
                    ScriptableStatType.CollectionStat => JsonConvert.DeserializeObject<CollectionServerStat>(statJson),
                    ScriptableStatType.EquipmentStat => JsonConvert.DeserializeObject<EquipServerStat>(statJson),
                    ScriptableStatType.RelicStat => JsonConvert.DeserializeObject<EquipServerStat>(statJson),
                    ScriptableStatType.SkillStat => JsonConvert.DeserializeObject<EquipServerStat>(statJson),
                    ScriptableStatType.GrimoireStat => JsonConvert.DeserializeObject<GrimoireServerStat>(statJson),
                    ScriptableStatType.TrainingStat => JsonConvert.DeserializeObject<TrainingServerStat>(statJson),
                    ScriptableStatType.AbilityStat => JsonConvert.DeserializeObject<TrainingServerStat>(statJson),
                    ScriptableStatType.AwakeningStat => JsonConvert.DeserializeObject<TrainingServerStat>(statJson),
                    ScriptableStatType.GuildSkillStat => JsonConvert.DeserializeObject<GuildSkillServerStat>(statJson),
                    ScriptableStatType.PetStat => JsonConvert.DeserializeObject<PetServerStat>(statJson),
                    ScriptableStatType.TitleStat => JsonConvert.DeserializeObject<TitleServerStat>(statJson),
                    ScriptableStatType.VIPStat => JsonConvert.DeserializeObject<VIPServerStat>(statJson),
                    ScriptableStatType.AwakeningTierStat => JsonConvert.DeserializeObject<AwakeningTierServerStat>(statJson),
                    _ => null
                };

                if(parsed != null)
                {
                    var targetStat = stats[stat.Key].Find(x => x.StatName().Equals(parsed.StatName()));
                    targetStat?.SetServerStat(parsed);
                }
            }
        }
    }

    public Dictionary<ScriptableStatType, List<string>> GetServerStats()
    {
        var serverStats = new Dictionary<ScriptableStatType, List<string>>();

        foreach(var statList in stats)
        {
            foreach(var stat in statList.Value)
            {
                var serverStat = stat.GetServerStat();
                if(serverStat == null || !serverStat.Initialized())
                {
                    continue;
                }

                if(!serverStats.ContainsKey(statList.Key))
                {
                    serverStats[statList.Key] = new List<string>();
                }

                serverStats[statList.Key].Add(serverStat.GetStatToJson());
            }
        }

        return serverStats;
    }

    #endregion

    #region "Scriptable Stat"

    private bool IsNotScriptable(ScriptableStatType statType)
    {
        if(statType == ScriptableStatType.DiceStat || statType == ScriptableStatType.NecklaceEffectStat || statType == ScriptableStatType.RingElementalEffectStat)
        {
            return true;
        }
        
        return false;
    }

    private void InitializeScriptableStats()
    {
        foreach(ScriptableStatType statType in Enum.GetValues(typeof(ScriptableStatType)))
        {
            if(IsNotScriptable(statType))
            {
                continue;
            }

            string dataTablePath = $"Assets/DataTables/Stat/CSV/{statType}Table.csv";

            if(!File.Exists(dataTablePath))
            {
                Debug.LogWarning($"CSV 파일 없음 → 스킵: {dataTablePath}");
                continue;
            }

            string csvText = File.ReadAllText(dataTablePath);
            TextAsset csvAsset = new TextAsset(csvText);
            var parsedData = CSVReader.Read(csvAsset);

            var loadedStats = LoadStatsForType(statType);
            stats[statType] = loadedStats;

            ApplyStatData(loadedStats, parsedData);
        }
    }

    private List<IStat> LoadStatsForType(ScriptableStatType statType)
    {
        var statList = new List<IStat>();
        var loaded = Resources.LoadAll<ScriptableObject>($"{resourceFolderPath}/{statType}");

        foreach(var obj in loaded)
        {
            if(obj is IStat stat)
            {
                statList.Add(stat);
            }
            else
            {
                Debug.LogWarning($"{obj.name}은 IStat을 구현하지 않았습니다.");
            }
        }

        return statList;
    }

    private void ApplyStatData(List<IStat> statList, Dictionary<string, Dictionary<string, string>> tableData)
    {
        if(statList.Count == 1)
        {
            statList[0].SetStat(tableData);
            return;
        }

        foreach(var entry in tableData)
        {
            var stat = statList.Find(x => x.StatName() == entry.Key);
            if(stat != null)
            {
                stat.SetStat(entry.Value);
            }
            else
            {
                Debug.LogWarning($"'{entry.Key}'에 해당하는 Stat을 찾을 수 없습니다.");
            }
        }
    }

    #endregion

    #region "Get"

    public ScriptableStatType[] GetAllStatTypes() => AllStatTypes;

    public Dictionary<ScriptableStatType, List<IStat>> GetAllStats() => stats;

    public List<IStat> GetStats(ScriptableStatType type)
    {
        return stats.TryGetValue(type, out var list) ? list : new List<IStat>();
    }

    public IStat GetStat(ScriptableStatType type, string name)
    {
        return stats.TryGetValue(type, out var list) ? list.Find(x => x.StatName().Equals(name)) : null;
    }

    public IStat GetStat(ScriptableStatType type)
    {
        return stats.TryGetValue(type, out var list) ? list[0] : null;
    }

    public IStat GetStatByReward(SCReward reward)
    {
        if(reward.rewardType != RewardType.Equippable) return null;

        string key = reward.GetRewardTitle();

        foreach(var stat in stats)
        {
            var currentStat = GetStat(stat.Key, key);
            if(currentStat != null)
            {
                return currentStat;
            }
        }

        return null;
    }

    #endregion
}

[System.Serializable]
public struct ScriptableStatTable
{
    public TextAsset textAsset;
    public ScriptableStatType statType;
}

[System.Serializable]
public enum ScriptableStatType
{
    None,
    ADBuffStat = 1, //not developed
    AdvancementStat, //not developed
    CollectionStat, //not developed
    EquipmentStat, //statmanager intergrate done
    RelicStat, //statmanager intergrate done
    SkillStat, //statmanager intergrate done
    GrimoireStat, //not developed
    TrainingStat, //statmanager intergrate done
    AbilityStat, //statmanager intergrate done
    AwakeningStat, //statmanager intergrate done
    GuildSkillStat, //not developed
    PetStat, //not developed
    TitleStat, //not developed
    VIPStat, //not developed
    AwakeningTierStat, //statmanager intergrate done
    DiceStat,
    NecklaceEffectStat,
    RingElementalEffectStat
}