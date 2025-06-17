using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveable
{
    DataDefinition GetDataID();
    
    void RegisterSaveData()=>DataManager.instance.RegisterSaveData(this);
    
    void UnregisterSaveData()=>DataManager.instance.UnregisterSaveData(this);//simplified
    
    void GetSaveData(Data data);

    void LoadData(Data data);


}
