using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json;
using System.IO;



[DefaultExecutionOrder(-100)]
public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    [Header("Event Listeners")] 
    public VoidEventSO saveDataEvent;
    public VoidEventSO loadDataEvent;
    
    private List<ISaveable> saveableList = new List<ISaveable>();
    private Data saveData;

    private string jsonFolder;//path to save data

    private void Awake()
    {
        //confirm that only one instance in the scene
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
        
        saveData = new Data();
        
        jsonFolder = Application.persistentDataPath + "/Save Data/";
        
        ReadSavedData();
        
    }

    private void OnEnable()
    {
        saveDataEvent.OnEventRaised += Save;
        loadDataEvent.OnEventRaised += Load;
    }

    private void OnDisable()
    {
        saveDataEvent.OnEventRaised -= Save;
        loadDataEvent.OnEventRaised -= Load;
    }

    private void Update()
    {
        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            Load();
        }
    }

    public void RegisterSaveData(ISaveable saveable)
    {
        if (!saveableList.Contains(saveable))
        {
            saveableList.Add(saveable);
        }
    }

    public void UnregisterSaveData(ISaveable saveable)
    {
        saveableList.Remove(saveable);
    }

    public void Save()
    {
        foreach (var saveable in saveableList)
        {
            saveable.GetSaveData(saveData);
        }
        
        var resultPath = jsonFolder + "SaveData.json";

        var jsonDate = JsonConvert.SerializeObject(saveData);

        if (!File.Exists(resultPath))
        {
            Directory.CreateDirectory(jsonFolder);
        }
        
        File.WriteAllText(resultPath, jsonDate);

        // foreach (var item in saveData.charcaterPosDict)
        // {
        //     Debug.Log(item.Key + " : " + item.Value);
        // }

    }

    public void Load()
    {
        foreach (var saveable in saveableList)
        {
            saveable.LoadData(saveData);
        }
    }

    private void ReadSavedData()
    {
        var resultPath = jsonFolder + "SaveData.json";
        if (File.Exists(resultPath))
        {
            var stringData = File.ReadAllText(resultPath);
            
            var jsonData = JsonConvert.DeserializeObject<Data>(stringData);
            
            saveData = jsonData;
        }
        
    }
}
