using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : MonoBehaviour, IInteractable
{
    [Header("Announcement")]
    public VoidEventSO saveDataEvent;
    
    [Header("variable")]
    public SpriteRenderer spriteRenderer;
    public GameObject lightObj;
    
    public Sprite darkSprite;
    public Sprite lightSprite;

    public bool isDone;
    

    private void OnEnable()
    {
        spriteRenderer.sprite = isDone? lightSprite : darkSprite;
        lightObj.SetActive(isDone);
    }

    public void TriggerAction()
    {
        if (!isDone)
        {
            isDone = true;
            spriteRenderer.sprite = lightSprite;
            lightObj.SetActive(true);
            
            //TODO:save data
            saveDataEvent.RaiseEvent();
            
            this.gameObject.tag = "Untagged";
            
        }
    }
    
    
}
