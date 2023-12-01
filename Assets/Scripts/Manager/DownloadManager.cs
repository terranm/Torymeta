using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DownloadManager : Singleton<DownloadManager> // : UnitySingleton<DownloadManager>
{
    private Text MapTitle;
    private AsyncOperationHandle<GameObject> handle;
    private AsyncOperationHandle<GameObject> characterMeshMatContainerHandle;
    private GameObject lookingforobj;
    private Canvas joystic;
    private Canvas touch;
    private Canvas loading;
    private Camera loadingCamera;
    private Coroutine mapDownloadMonitoringTask;
    public bool isDownComplete;

    //public void InitializeAddressableAssets()
    //{
    //    Addressables
    //}

    public void CharacterContainerDownload()
    {
        characterMeshMatContainerHandle = Addressables.InstantiateAsync("MeshMatContainer");
        characterMeshMatContainerHandle.Completed += _ =>
        {
            Debug.Log("characterMeshMatContainerHandle.Result.name " + characterMeshMatContainerHandle.Result.name);
            DontDestroyOnLoad(characterMeshMatContainerHandle.Result);
            Debug.Log("MeshMatContainer Downloaded");
        };
    }

    public void Download()
    {
        isDownComplete = false;
        string sceneName = PlayerData.myPlayerinfo.universityCode;

        string key = "";

        if (String.Equals(PlayerData.myPlayerinfo.universityCode, "seminar"))
        {
            switch (PlayerData.myPlayerinfo.seminarType)
            {
                case "20": key = "SeminarL"; break;
                case "21": key = "SeminarM"; break;
                case "22": key = "SeminarS"; break;
            }
        }
        else if (String.Equals(PlayerData.myPlayerinfo.universityCode, "lobby"))
        {
            key = "Lounge";
        }
        else
        {
            key = PlayerData.myPlayerinfo.universityCode;
        }
        
        Debug.Log(PlayerData.myPlayerinfo.universityCode + " : " + "Download size is " + Addressables.GetDownloadSizeAsync(key).Result);

        
        
        handle = Addressables.InstantiateAsync(key);

        mapDownloadMonitoringTask = StartCoroutine(MonitoringMapDownload());
        
        handle.Completed += _ =>
        {
            StopCoroutine(mapDownloadMonitoringTask);
            StartCoroutine(SetEnvironment());
            Debug.Log("Bundle " + key + " Downloaded");
        };
    }

    private IEnumerator MonitoringMapDownload()
    {
        int timeOutTimer = 0;
        Debug.Log("map down monitoring start");
        while (handle.IsValid())
        {
            yield return new WaitForSeconds(1f);
            timeOutTimer++;
            Debug.Log("map down " + (handle.PercentComplete*100f).ToString()+"% took " + timeOutTimer+"sec");
            if (timeOutTimer > 30)
            {
                //시간 초과
                NativeManager.Instance.SendNetworkTimeOutConfirm();
                break;
            }
        }
    }

    private IEnumerator SetEnvironment()
    {
        new WaitForEndOfFrame();
        Debug.Log("SetEnvironment called at " + SceneManager.GetActiveScene().name);
        while (lookingforobj == null)
        {
            yield return new WaitForEndOfFrame();
            lookingforobj = GameObject.FindGameObjectWithTag("Joystick");
        }
        
        joystic = GameObject.FindGameObjectWithTag("Joystick").GetComponent<Canvas>();
        touch = GameObject.FindGameObjectWithTag("TouchZone").GetComponent<Canvas>();
        GameObject objloading = GameObject.FindGameObjectWithTag("Loading");
        loading = objloading.GetComponentInChildren<Canvas>();
        loadingCamera = objloading.GetComponentInChildren<Camera>();


        if (MapTitle == null)
            MapTitle = GameObject.FindWithTag("WorldTitle").GetComponent<Text>();

        MapTitle.text = GetRoomName(PlayerData.myPlayerinfo.universityCode);

        /*
        if (true)//(!PlayerData.myPlayerinfo.universityCode.Equals("seminar"))
        {
            joystic.enabled = true;
            touch.enabled = true;
            loading.enabled = false;
            loadingCamera.enabled = false;
        }
        */

        isDownComplete = true;
    }

    public void StartLoadingProcess()
    {
        if (joystic != null)
            joystic.enabled = false;
        if (touch != null)
            touch.enabled = false;
        if (loading != null)
            loading.enabled = true;
        if (loadingCamera != null)
            loadingCamera.enabled = true;
    }

    public IEnumerator KillLoadingProcess()
    {
        while (!isDownComplete)
        {
            yield return new WaitForEndOfFrame();
        }
        if(joystic != null)
            joystic.enabled = true;
        if(touch != null)
            touch.enabled = true;
        if(loading != null)
            loading.enabled = false;
        if(loadingCamera != null)
            loadingCamera.enabled = false;
        yield return null;
    }

    private string GetRoomName(string universeCode)
    {
        string roomname = "\n";
        if (!string.IsNullOrEmpty(PlayerData.myPlayerinfo.seminarTitle))
        {
            if (PlayerData.myPlayerinfo.seminarTitle.Length < 9)
                roomname += PlayerData.myPlayerinfo.seminarTitle;
            else
                roomname += PlayerData.myPlayerinfo.seminarTitle.Substring(0, 8) + "...";
        }
        string code = "";
        LocalizationController.Instance.WaitLocaleText((localeText) => { code = localeText; }, "DownloadManager_" + universeCode);

        return code;

        //return universeCode switch
        //{
        //    "CAU" => "중앙대학교",
        //    "HONAU" => "호남대학교",
        //    "SMOONU" => "선문대학교",
        //    "KKU" => "건국대학교",
        //    "seminar" => "세미나실" + roomname,
        //    "lobby" => "로비",
        //    _ => "로비"
        //};
    }

    private void OnDestroy()
    {
        if (!handle.IsDone)
            Addressables.Release(handle);
    }

    //private static string _activeCatalogPath;
    //private static IResourceLocator _activeCatalogResourceLocator;

    //static async UniTask<IResourceLocator> SetupAndInitAddressables(string loadPath, CancellationToken ct)
    //{
    //    using var exeTimer = new ExecutionTimer($"{nameof(PackDownloader)}.{nameof(SetupAndInitAddressables)}");

    //    Debug.Log("Setting up Addressable Properties");
    //    AddressableVariables.LoadPath = loadPath;
    //    UnityEngine.AddressableAssets.Initialization.AddressablesRuntimeProperties.ClearCachedPropertyValues();

    //    Debug.Log("Initializing Addressables");
    //    await Addressables.InitializeAsync().WithCancellation(ct);


    //    var catalogPath = $"{loadPath}/catalog_root.json";
    //    if (_activeCatalogPath != catalogPath)
    //    {
    //        using var exeTimer2 = new ExecutionTimer($"{nameof(PackDownloader)}.{nameof(Addressables.LoadContentCatalogAsync)}");

    //        Addressables.ClearResourceLocators();
    //        _activeCatalogPath = catalogPath;

    //        if (_activeCatalogResourceLocator != null) Addressables.RemoveResourceLocator(_activeCatalogResourceLocator);
    //        _activeCatalogResourceLocator = await Addressables.LoadContentCatalogAsync(catalogPath, true).WithCancellation(ct);
    //    }

    //    return _activeCatalogResourceLocator;
    //}

    public void AddressbleChange()
    {
        // AddressablesRuntimeProperties.ClearCachedPropertyValues();
        // Addressables.UpdateCatalogs(new List<string> { ResourceManagerRuntimeData.kCatalogAddress });
    }

    // https://test-metabus-unity.s3.ap-northeast-2.amazonaws.com/address/ios
}
