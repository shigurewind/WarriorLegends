using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public PlayerStatBar playerStatBar;
    
    [Header("Events Listeners")]
    public CharacterEventSO healthEvent;
    
    public SceneLoadEventSO unloadedSceneEvent;

    public VoidEventSO loadDataEvent;
    public VoidEventSO gameOverEvent;
    public VoidEventSO backToMenuEvent;
    public FloatEventSO syncVolumeEvent;
    
    [Header("Announcements")]
    public VoidEventSO pauseEvent;
    

    [Header("Component")]
    public GameObject gameOverPanel;

    public GameObject restartButton;

    public GameObject mobileTouch;
    public Button settingsBtn;
    public GameObject pausePanel;
    public Slider volumeSlider;


    private void Awake()
    {
#if UNITY_STANDALONE
        mobileTouch.SetActive(false);
#endif
        
        settingsBtn.onClick.AddListener(TogglePausePanel);
    }
    

    private void OnEnable()
    {
        healthEvent.OnEventRaised += OnHealthEvent;
        unloadedSceneEvent.LoadRequestEvent += OnUnLoadedSceneEvent;
        loadDataEvent.OnEventRaised += OnLoadDataEvent;
        gameOverEvent.OnEventRaised += OnGameOverEvent;
        backToMenuEvent.OnEventRaised += OnLoadDataEvent;
        syncVolumeEvent.OnEventRaised += OnSyncVolumeEvent;
    }

    


    private void OnDisable()
    {
        healthEvent.OnEventRaised -= OnHealthEvent;
        unloadedSceneEvent.LoadRequestEvent -= OnUnLoadedSceneEvent;
        loadDataEvent.OnEventRaised -= OnLoadDataEvent;
        gameOverEvent.OnEventRaised -= OnGameOverEvent;
        backToMenuEvent.OnEventRaised -= OnLoadDataEvent;
        syncVolumeEvent.OnEventRaised -= OnSyncVolumeEvent;
    }

    private void OnSyncVolumeEvent(float amount)
    {
        volumeSlider.value = (amount + 80f) / 100f;
    }

    //pause game
    private void TogglePausePanel()
    {
        if (pausePanel.activeInHierarchy)
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1;
        }
        else
        {
            pauseEvent.RaiseEvent();
            pausePanel.SetActive(true);
            Time.timeScale = 0;
        }
    }
    
    private void OnGameOverEvent()
    {
        gameOverPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(restartButton);
    }

    private void OnLoadDataEvent()
    {
        gameOverPanel.SetActive(false);
    }

    private void OnUnLoadedSceneEvent(GameSceneSO sceneToLoad, Vector3 arg1, bool arg2)
    {
        if (sceneToLoad.sceneType == SceneType.Menu)
        {
            playerStatBar.gameObject.SetActive(false);
        }
        else
        {
            playerStatBar.gameObject.SetActive(true);
        }
    }

    private void OnHealthEvent(Character character)
    {
        var percentage = character.currentHp / character.maxHp;
        playerStatBar.OnHealthChange(percentage);
        
        playerStatBar.OnPowerChange(character);
    }
    
    
}
