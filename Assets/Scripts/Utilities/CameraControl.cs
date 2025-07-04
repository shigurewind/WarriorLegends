using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraControl : MonoBehaviour
{
    [Header("event listener")]
    public VoidEventSO afterSceneLoadedEvent;
    
    private CinemachineConfiner2D confiner2D;
    
    public CinemachineImpulseSource impulseSource;
    
    public VoidEventSO cameraShakeEvent;

    private void Awake()
    {
        confiner2D = GetComponent<CinemachineConfiner2D>();
    }

    private void OnEnable()
    {
        cameraShakeEvent.OnEventRaised += OnCameraShakeEvent;
        afterSceneLoadedEvent.OnEventRaised += OnAfterSceneLoadedEvent;
    }


    private void OnDisable()
    {
        cameraShakeEvent.OnEventRaised -= OnCameraShakeEvent;
        afterSceneLoadedEvent.OnEventRaised -= OnAfterSceneLoadedEvent;
    }

    private void OnCameraShakeEvent()
    {
        impulseSource.GenerateImpulse();
    }
    private void OnAfterSceneLoadedEvent()
    {
        GetNewCameraBounds();
    }

    

    private void GetNewCameraBounds()
    {
        var obj = GameObject.FindGameObjectWithTag("Bounds");
        if (obj == null)return;
        
        confiner2D.m_BoundingShape2D = obj.gameObject.GetComponent<Collider2D>();
        
        confiner2D.InvalidateCache();
        
    }
}
