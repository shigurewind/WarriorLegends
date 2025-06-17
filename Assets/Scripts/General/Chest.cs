using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour,IInteractable
{

    private SpriteRenderer spriteRenderer;
    
    public Sprite openSprite;
    public Sprite closedSprite;
    public bool isDone;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        spriteRenderer.sprite = isDone ? openSprite : closedSprite;
    }

    public void TriggerAction()
    {
        
        if (!isDone)
        {
            OpenChest();
            
        }
    }

    private void OpenChest()
    {
        //Debug.Log("Chest Opened");
        spriteRenderer.sprite = openSprite;
        isDone = true;
        this.gameObject.tag = "Untagged";
        //
    }
}
