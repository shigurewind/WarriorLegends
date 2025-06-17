using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour, ISaveable
{
    public Transform playerTrans;

    public Vector3 firstPosition;
    
    public Vector3 menuPosition;
    
    [Header("Events Listeners")] 
    public SceneLoadEventSO loadEventSO;
    public VoidEventSO newGameEvent;
    public VoidEventSO backToMenuEvent;
    
    [Header("announcement")]
    public VoidEventSO afterSceneLoadedEvent;
    public FadeEventSO fadeEvent;
    public SceneLoadEventSO unloadedSceneEvent;
    
    [Header("Scene")]
    public GameSceneSO firstLoadScene;
    public GameSceneSO menuScene;
    
    private GameSceneSO sceneToLoad;
    [SerializeField] private GameSceneSO currentLoadedScene;

    [Header("parameters")]
    private Vector3 positionToGo;
    private bool fadeScreen;
    
    private bool isLoading;

    public float fadeDuration;

    private void Awake()
    {
        //Addressables.LoadSceneAsync(firstLoadScene.sceneReference, LoadSceneMode.Additive);
        // currentLoadedScene = firstLoadScene;
        // currentLoadedScene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive);
    }

    //TODO:change after main menu done
    private void Start()
    {
        loadEventSO.RaiseLoadRequestEvent(menuScene,menuPosition,true);
        // NewGame();
        
        
        
    }

    private void OnEnable()
    {
        loadEventSO.LoadRequestEvent += OnLoadRequestEvent;
        newGameEvent.OnEventRaised += NewGame;
        backToMenuEvent.OnEventRaised += OnBackToMenuEvent;
        
        ISaveable saveable = this;
        saveable.RegisterSaveData();
    }

    private void OnDisable()
    {
        loadEventSO.LoadRequestEvent -= OnLoadRequestEvent;
        newGameEvent.OnEventRaised -= NewGame;
        backToMenuEvent.OnEventRaised -= OnBackToMenuEvent;
        
        ISaveable saveable = this;
        saveable.UnregisterSaveData();
    }

    private void OnBackToMenuEvent()
    {
        sceneToLoad = menuScene;
        loadEventSO.RaiseLoadRequestEvent(sceneToLoad,menuPosition,true);
    }

    private void NewGame()
    {
        sceneToLoad = firstLoadScene;
        // OnLoadRequestEvent(sceneToLoad,firstPosition,true);
        loadEventSO.RaiseLoadRequestEvent(sceneToLoad,firstPosition,true);
    }

    /// <summary>
    /// scene load event request
    /// </summary>
    /// <param name="locationToLoad"></param>
    /// <param name="posToGo"></param>
    /// <param name="fadeScreen"></param>
    private void OnLoadRequestEvent(GameSceneSO locationToLoad, Vector3 posToGo, bool fadeScreen)
    {
        if (isLoading)
        {
            return;
        }
        
        isLoading = true;
        
        sceneToLoad = locationToLoad;
        positionToGo = posToGo;
        this.fadeScreen = fadeScreen;

        if (currentLoadedScene != null)
        {
            StartCoroutine(UnLoadPreviousScene());
        }
        else
        {
            LoadNewScene();
        }


    }

    private IEnumerator UnLoadPreviousScene()
    {
        if (fadeScreen)
        {
            //turn black
            fadeEvent.FadeIn(fadeDuration);
            
        }

        yield return new WaitForSeconds(fadeDuration);
        
        //health bar display
        unloadedSceneEvent.RaiseLoadRequestEvent(sceneToLoad,positionToGo,true);

        yield return currentLoadedScene.sceneReference.UnLoadScene();
        
        //close the player
        playerTrans.gameObject.SetActive(false);
        
        LoadNewScene();
    }

    private void LoadNewScene()
    { 
        var loadingOption =  sceneToLoad.sceneReference.LoadSceneAsync(LoadSceneMode.Additive,true);

        loadingOption.Completed += OnLoadCompleted;
    }

    /// <summary>
    /// scene loading over
    /// </summary>
    /// <param name="obj"></param>
    private void OnLoadCompleted(AsyncOperationHandle<SceneInstance> obj)
    {
        currentLoadedScene = sceneToLoad;
        
        playerTrans.position = positionToGo;
        playerTrans.gameObject.SetActive(true);

        if (fadeScreen)
        {
            //turn transparent
            fadeEvent.FadeOut(fadeDuration);
        }
        
        isLoading = false;
        
        //the event after scene load
        if (currentLoadedScene.sceneType != SceneType.Menu)
        {
            afterSceneLoadedEvent?.RaiseEvent();

        }
    }

    public DataDefinition GetDataID()
    {
        return GetComponent<DataDefinition>();
    }

    public void GetSaveData(Data data)
    {
        data.SaveGameScene(currentLoadedScene);
    }

    public void LoadData(Data data)
    {
        var playerID = playerTrans.GetComponent<DataDefinition>().ID;
        if (data.charcaterPosDict.ContainsKey(playerID))
        {
            positionToGo = data.charcaterPosDict[playerID].ToVector3();
            sceneToLoad = data.GetSavedScene();
            
            OnLoadRequestEvent(sceneToLoad, positionToGo,true);
        }
    }
}
