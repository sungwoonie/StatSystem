using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StatLoader<T> where T : Stat, new()
{
    private readonly string dataTablePath = "Assets/DataTables/{0}/CSV/{1}Table.csv";

    private string rootPath;
    private string dataName;

    private List<T> loadedDatas;

    public StatLoader(string _dataName, string dataTableRoot = "Stat") 
    {
        dataName = _dataName;
        rootPath = dataTableRoot;
        InitializeData();
    }

    private void InitializeData()
    {
        loadedDatas = new List<T>();

        string path = string.Format(dataTablePath, rootPath, dataName);

        if(!File.Exists(path))
        {
            Debug.LogWarning($"CSV 파일 없음 → 스킵: {dataTablePath}");
            return;
        }

        string csvText = File.ReadAllText(path);
        TextAsset csvAsset = new TextAsset(csvText);
        var parsedData = CSVReader.Read(csvAsset);

        foreach(var data in parsedData)
        {
            var newData = new T();
            newData.SetStat(data.Value);
            loadedDatas.Add(newData);
        }
    }

    public List<T> GetData() => loadedDatas;
}