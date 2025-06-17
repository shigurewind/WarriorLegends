using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataDefinition : MonoBehaviour
{
    public PersistentType persistentType;
    
    public string ID;

    private void OnValidate()
    {
        //the only ID of object
        if (persistentType == PersistentType.ReadWrite)
        {
            if (ID == string.Empty)
            {
                ID = System.Guid.NewGuid().ToString();
            }
            
        }
        else
        {
            ID = string.Empty;
        }
    }
}
