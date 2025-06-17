using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Events;

public class Character : MonoBehaviour, ISaveable
{
    [Header("Event Listeners")]
    public VoidEventSO newGameEvent; 
    
    [Header("Attributes")] 
    public float maxHp;
    public float currentHp;

    public float maxPower;
    public float currentPower;
    public float powerRecoverSpeed;
    
    [Header("Invulnerable")]
    public float invulnerableDuration;
    public float invulnerableCounter;
    public bool invulnerable;
    
    public UnityEvent<Transform> OnTakeDamage;
    public UnityEvent OnDie;

    public UnityEvent<Character> OnHealthChange;

    private void Start()
    {
        currentHp = maxHp;
    }

    private void OnEnable()
    {
        newGameEvent.OnEventRaised += NewGame;

        ISaveable saveable = this;
        saveable.RegisterSaveData();
    }

    private void OnDisable()
    {
        newGameEvent.OnEventRaised -= NewGame;

        ISaveable saveable = this;
        saveable.UnregisterSaveData();
    }

    private void Update()
    {
        if (invulnerable)
        {
            invulnerableCounter -= Time.deltaTime;
            if (invulnerableCounter <= 0) 
            {
                invulnerable = false;
            }
        }

        if (currentPower < maxPower)
        {
            currentPower += powerRecoverSpeed * Time.deltaTime;
        }
        else
        {
            currentPower = maxPower;
        }

    }
    
    private void NewGame()
    {
        currentHp = maxHp;
        currentPower = maxPower;
        
        OnHealthChange?.Invoke(this);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Water"))
        {
            //fall into water
            if (currentHp > 0)
            {
                currentHp = 0;
                OnHealthChange?.Invoke(this);
                OnDie?.Invoke();
            }
            
        }
    }

    public void TakeDamage(Attack attacker)
    {
        if (invulnerable)
            return;

        if (currentHp>attacker.damage)
        {
            currentHp -= attacker.damage;
             // Debug.Log(this.name+"受けた"+ attacker.damage+"from"+attacker.name);
            TriggerInvulnerable();
            //傷つけ動作実行
            OnTakeDamage?.Invoke(attacker.transform);
            
        }
        else
        {
            currentHp = 0;
            //Die
            OnDie?.Invoke();
        }
        
        OnHealthChange?.Invoke(this);
        
    }

    private void TriggerInvulnerable()
    {
        if (!invulnerable)
        {
            invulnerable = true;
            invulnerableCounter = invulnerableDuration;
        }
    }

    public void OnSlide(int cost)
    {
        currentPower -= cost;
        OnHealthChange?.Invoke(this);
    }

    public DataDefinition GetDataID()
    {
        return GetComponent<DataDefinition>();
    }

    public void GetSaveData(Data data)
    {
        if (data.charcaterPosDict.ContainsKey(GetDataID().ID))
        {
            data.charcaterPosDict[GetDataID().ID] = new SerializeVector3(transform.position);
            data.floatSaveData[GetDataID().ID + "health"] = this.currentHp;
            data.floatSaveData[GetDataID().ID + "power"] = this.currentPower;
        }
        else
        {
            data.charcaterPosDict.Add(GetDataID().ID, new SerializeVector3(transform.position));
            data.floatSaveData.Add(GetDataID().ID + "health", this.currentHp);
            data.floatSaveData.Add(GetDataID().ID + "power", this.currentPower);
        }
    }

    public void LoadData(Data data)
    {
        if (data.charcaterPosDict.ContainsKey(GetDataID().ID))
        {
            this.currentHp = data.floatSaveData[GetDataID().ID + "health"];
            this.currentPower = data.floatSaveData[GetDataID().ID + "power"];
            transform.position = data.charcaterPosDict[GetDataID().ID].ToVector3();
            
            //UI display update
            OnHealthChange?.Invoke(this);
        }
    }
}
