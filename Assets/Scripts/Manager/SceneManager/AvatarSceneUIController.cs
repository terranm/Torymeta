using System;
using System.Collections;
using System.Collections.Generic;
using Colyseus;
using Newtonsoft.Json;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class AvatarSceneUIController : MonoBehaviour
{
    public string version;// = "2.0.4";
    private GameObject controllerObject;
    private TouchController touchController;

    private CharctorMeshAndMaterialController meshMatCtrl;

    [SerializeField] private GameObject _objLoading;

    [SerializeField] private GameObject _objLoadingCanvas;
    [SerializeField] private GameObject _objLoadingCam;

    private void Awake()
    {
        //Debug.Log("Download  " + UnityEngine.AddressableAssets.Addressables.BuildPath + ", " + UnityEngine.AddressableAssets.Addressables.LibraryPath + ", " + UnityEngine.AddressableAssets.Addressables.PlayerBuildDataPath + ", " + UnityEngine.AddressableAssets.Addressables.RuntimePath);
        Debug.Log("Unity Torymeta Version " + version);
        StartCoroutine(Init());
        NativeManager.Instance.SendUnityLifecycleState(NativeManager.LifeCycle.Awake);
    }

    private void Start()
    {
        Debug.Log("Start : AvatarScene");
#if UNITY_ANDROID && !UNITY_EDITOR
        Screen.orientation = ScreenOrientation.Portrait;
#endif
        GameEvents.Instance.OnRequestSceneChange += LoadingCanvasController;
        GameEvents.Instance.OnRequestAvatarZoom += View_ZoomIn;
        GameEvents.Instance.OnRequestAvatarRotate += View_Rotation;
        //GameEvents.Instance.OnRequestCamMove += View_UpDown;
        GameEvents.Instance.OnRequestSetAvatarInfo += SetAvatar;
        
        controllerObject = GameObject.FindGameObjectWithTag("AvatarContainer");
        touchController = controllerObject.GetComponent<TouchController>();
        meshMatCtrl = touchController.GetAvatar().GetComponent<CharctorMeshAndMaterialController>();

        _objLoading = GameObject.FindGameObjectWithTag("Loading");
        _objLoadingCanvas = _objLoading.GetComponentInChildren<Canvas>().gameObject;
        _objLoadingCam = _objLoading.GetComponentInChildren<Camera>().gameObject;

        StartCoroutine(WaitContainer());

        if (PlayerData.myPlayerinfo.backSign)
        {
            Screen.fullScreen = false;
            NativeManager.Instance.SendNativeBack();
            PlayerData.myPlayerinfo.backSign = false;
        }
    }

    private IEnumerator WaitContainer()
    {
        Debug.Log("WaitContainer");
        while (GameObject.Find("MeshMatContainer(Clone)") == null)
        {
            yield return new WaitForUpdate();
        }
        NativeManager.Instance.SendUnityLifecycleState(NativeManager.LifeCycle.Start);
        NativeManager.Instance.SendRequestMemberInfo();

        if (PlayerData.myPlayerinfo.state != null)
        {
//#if UNITY_EDITOR
            meshMatCtrl.CharacterSetting();
//#endif
        }

        touchController.GetAvatar().transform.position = Vector3.zero;

        _objLoading.SetActive(false);
        
        yield return null;
    }

    private void OnDestroy()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnRequestSceneChange -= LoadingCanvasController;
            GameEvents.Instance.OnRequestAvatarZoom -= View_ZoomIn;
            GameEvents.Instance.OnRequestAvatarRotate -= View_Rotation;
            //GameEvents.Instance.OnRequestCamMove -= View_UpDown;
            GameEvents.Instance.OnRequestSetAvatarInfo -= SetAvatar;
        }
        
        if(NativeManager.Instance != null)NativeManager.Instance.SendUnityLifecycleState(NativeManager.LifeCycle.OnDestroy);
    }

    private IEnumerator Init()
    {
        while (ServerManager.Instance == null)
        {
            yield return new WaitForEndOfFrame();
        }
        
        while (BasicSceneManager.Instance == null)
        {
            yield return new WaitForEndOfFrame();
        }

        //Debug.Log("AddressableWrapper.isTest : " + Torymeta.Addressable.AddressableWrapper.isTest + " PlayerData.myPlayerinfo.isAppRelease : " + PlayerData.myPlayerinfo.isAppRelease);
        //Torymeta.Addressable.AddressableWrapper.isTest = PlayerData.myPlayerinfo.isAppRelease;
        //Debug.Log("AddressableWrapper.isTest : " + Torymeta.Addressable.AddressableWrapper.isTest + " PlayerData.myPlayerinfo.isAppRelease : " + PlayerData.myPlayerinfo.isAppRelease);

#if UNITY_EDITOR
        string id = Random.Range(100, 999).ToString();
        NativeManager.Instance.SendToUnity(
            "{\"type\":\"AvatarInfo\",\"value\":{\"avatarState\":{\"bodyType\":\"BODY_AA_001\",\"bottomCode\":\"BOTTOM_AA_001\",\"bottomColorCode\":\"#000000\",\"faceCode\":\"FACE_AA_001\",\"faceColorCode\":\"#fae7d6\",\"hairCode\":\"HAIR_AA_001\",\"hairColorCode\":\"#434343\",\"shoesCode\":\"SHOES_AA_001\",\"shoesColorCode\":\"#000000\",\"skinCode\":\"SKIN_AA_001\",\"skinColorCode\":\"#fae7d6\",\"topCode\":\"TOP_AA_001\",\"topColorCode\":\"#000000\"},\"memberId\":"+ id + ",\"userName\":\"유니티테스트"+ id + "\"}}"
        );
#endif
        
        yield return null;
    }
    
    //=============================================Region Divider======================================
    
    #region Game Event handler Region

    private void SetAvatar()
    {
        touchController.CreateAvatar();
        touchController.ResetZoom();
        touchController.ResetRotation();
        touchController.ResetPosition();

        meshMatCtrl.CharacterSetting();
        //touchController.
    }
    
    public void LoadingCanvasController()
    {
        //LoadScene();

        if (PlayerData.myPlayerinfo.universityCode.Equals("SelectView") || PlayerData.myPlayerinfo.universityCode.Equals("AvatarView"))
        {
            //Screen.fullScreen = false;
            _objLoading.SetActive(false);
            //_objLoadingCanvas.SetActive(false);
            //_objLoadingCam.SetActive(false);
        }
        else
        {
            //if (string.IsNullOrEmpty(PlayerData.myPlayerinfo.avatar))
            //    PlayerData.myPlayerinfo.avatar = "100_11";
            //if (PlayerData.myPlayerinfo.memberId.Equals(0))
            //    PlayerData.myPlayerinfo.memberId = Random.Range(100, 999);
            //if (string.IsNullOrEmpty(PlayerData.myPlayerinfo.userName))
            //    PlayerData.myPlayerinfo.userName = "unityTest" + PlayerData.myPlayerinfo.memberId;
            //ServerManager.Instance.TryConnect(PlayerData.myPlayerinfo.universityCode,
            //    (error) => { Debug.LogError($"Login 에러 l {error} l"); });
            _objLoading.SetActive(true);
            //_objLoadingCanvas.SetActive(true);
            //_objLoadingCam.SetActive(true);
            //Screen.fullScreen = true;
        }
    }

    public void AvatarView_Select(string msg)
    {
        Debug.Log("AvatarSelect_Select");
        CharacterSelect value = JsonConvert.DeserializeObject<CharacterSelect>(msg);

        if (string.IsNullOrEmpty(value.characterId))
        {
            if (string.IsNullOrEmpty(PlayerData.myPlayerinfo.avatar))
            {
                //touchController.Select("100_11");
                return;
            }
        }
        else
            PlayerData.myPlayerinfo.avatar = value.characterId;
        //touchController.Select(PlayerData.myPlayerinfo.avatar);

        Debug.Log(value.characterId);

        touchController.ResetZoom();
        touchController.ResetRotation();
        touchController.ResetPosition();
    }

    public void View_ZoomIn(bool Zoom)
    {
        if(!Zoom)
            touchController.ZoomOut();
        else
            touchController.ZoomIn();
    }
    
    public void View_Rotation()
    {
        touchController.Rotation();
    }

    //public void View_UpDown(bool isUp)
    //{
    //    if (!isUp)
    //        touchController.MoveUpHorizontal();
    //    else
    //        touchController.MoveDownHorizontal();
    //}

    //private void LoadScene()
    //{
    //    StartCoroutine(LoadSceneAsync());
    //}

    //private IEnumerator LoadSceneAsync(Action onComplete = null)
    //{
    //    // Scene currScene = SceneManager.GetActiveScene();

    //    string SceneName = PlayerData.myPlayerinfo.universityCode;

    //    if (!SceneName.Equals("lobby") && !SceneName.Equals("seminar") && !SceneName.Equals("AvatarView") && !SceneName.Equals("SelectView"))
    //        // if (!SceneName.Equals("lobby") && !SceneName.Equals("AvatarView") && !SceneName.Equals("SelectView"))
    //        SceneName = "world";

    //    AsyncOperation op = SceneManager.LoadSceneAsync(SceneName);

    //    while (op.progress <= 0.9f)
    //    {
    //        //Wait until the scene is loaded
    //        yield return new WaitForEndOfFrame();
    //    }

    //    op.allowSceneActivation = true;

    //    if (onComplete != null)
    //        onComplete?.Invoke();
    //}
    #endregion


    //=============================================Region Divider======================================

    #region Script for test on UnityEditor Env region
#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            NativeManager.Instance.SendToUnity(
                "{\"type\":\"ZoomIn\",\"value\":{}}"
            );
            //AvatarView_ZoomIn("");
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            NativeManager.Instance.SendToUnity(
                "{\"type\":\"ZoomOut\",\"value\":{}}"
            );
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            NativeManager.Instance.SendToUnity(
                "{\"type\":\"AvatarRotate\",\"value\":{}}"
            );
        }
        
        // SelectView
        if (Input.GetKeyDown(KeyCode.W))
        {
            //SwitchScene("{\"scene\":\"SelectView\",\"characterId\":\"\"}");
            NativeManager.Instance.SendToUnity(
                "{\"type\":\"SwitchScene\",\"value\":{\"url\":\"\",\"roomType\":\"\",\"roomId\":\"\",\"roomTitle\":\"\",\"timeDiff\":0,\"isAppRelease\":false,\"scene\":\"SelectView\",}}"
            );
        }

        // Seminar
        if (Input.GetKeyDown(KeyCode.E))
        { 
            /*
            PlayerData.myPlayerinfo.userName = "UnityTest";
            PlayerData.myPlayerinfo.memberId = 123;
            PlayerData.myPlayerinfo.seminarUrl = "http://ec2-3-34-142-39.ap-northeast-2.compute.amazonaws.com/OT.mp4";
            PlayerData.myPlayerinfo.seminarType = "22";
            PlayerData.myPlayerinfo.seminarId = "000000022";
            PlayerData.myPlayerinfo.seminarTitle = "testname12312312312";
            //PlayerData.myPlayerinfo.seminarStartTime = "2023-07-03 20:41:40";
            PlayerData.myPlayerinfo.isAppRelease = false;
            PlayerData.myPlayerinfo.universityCode = "seminar";
            SwitchScene();
            */
            NativeManager.Instance.SendToUnity(
                "{\"type\":\"SwitchScene\",\"value\":{\"url\":\"https://contents.ttceducation.net/맛보기 강좌/리슨업/OT.mp4\",\"roomType\":\"22\",\"roomId\":\"000000022\",\"roomTitle\":\"testname12312312312\",\"videoStartTime\":\"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+"\",\"isAppRelease\":false,\"scene\":\"seminar\",}}"
            );
        }
        
        // world
        if (Input.GetKeyDown(KeyCode.T))
        {
            /*
            PlayerData.myPlayerinfo.userName = "UnityTest";
            PlayerData.myPlayerinfo.memberId = 123;
            PlayerData.myPlayerinfo.seminarUrl = "http://ec2-3-34-142-39.ap-northeast-2.compute.amazonaws.com/OT.mp4";
            PlayerData.myPlayerinfo.seminarType = "";
            PlayerData.myPlayerinfo.seminarId = "";
            PlayerData.myPlayerinfo.seminarTitle = "";
            //PlayerData.myPlayerinfo.seminarStartTime = "";
            PlayerData.myPlayerinfo.isAppRelease = false;
            PlayerData.myPlayerinfo.universityCode = "lobby";
            SwitchScene();
            */
            NativeManager.Instance.SendToUnity(
                "{\"type\":\"SwitchScene\",\"value\":{\"url\":\"https://test-torymeta-image.s3.ap-northeast-2.amazonaws.com/video/videoplayback.mp4\",\"roomType\":\"\",\"roomId\":\"\",\"roomTitle\":\"\",\"videoStartTime\":\"\",\"isAppRelease\":false,\"scene\":\"lobby\"}}"
            );
        }
        
        // world
        if (Input.GetKeyDown(KeyCode.Y))
        {
            /*
            PlayerData.myPlayerinfo.userName = "UnityTest";
            PlayerData.myPlayerinfo.memberId = 123;
            PlayerData.myPlayerinfo.seminarUrl = "http://ec2-3-34-142-39.ap-northeast-2.compute.amazonaws.com/OT.mp4";
            PlayerData.myPlayerinfo.seminarType = "";
            PlayerData.myPlayerinfo.seminarId = "";
            PlayerData.myPlayerinfo.seminarTitle = "";
            //PlayerData.myPlayerinfo.seminarStartTime = "";
            PlayerData.myPlayerinfo.isAppRelease = false;
            PlayerData.myPlayerinfo.universityCode = "lobby";
            SwitchScene();
            */
            NativeManager.Instance.SendToUnity(
                "{\"type\":\"SwitchScene\",\"value\":{\"url\":\"https://test-torymeta-image.s3.ap-northeast-2.amazonaws.com/video/videoplayback.mp4\",\"roomType\":\"festival\",\"roomId\":\"\",\"roomTitle\":\"\",\"videoStartTime\":\"\",\"isAppRelease\":false,\"scene\":\"lobby\"}}"
            );
        }
        
        //{"type":"SwitchScene","value":{"roomId":"","scene":"lobby","isAppRelease":false,"roomType":"","roomTitle":"","videoStartTime":"","url":"https:\/\/test-torymeta-image.s3.ap-northeast-2.amazonaws.com\/video\/videoplayback.mp4"}}
        if (Input.GetKeyDown(KeyCode.K))
        {
            /*
            PlayerData.myPlayerinfo.userName = "UnityTest";
            PlayerData.myPlayerinfo.memberId = 123;
            PlayerData.myPlayerinfo.seminarUrl = "http://ec2-3-34-142-39.ap-northeast-2.compute.amazonaws.com/OT.mp4";
            PlayerData.myPlayerinfo.seminarType = "";
            PlayerData.myPlayerinfo.seminarId = "";
            PlayerData.myPlayerinfo.seminarTitle = "";
            //PlayerData.myPlayerinfo.seminarStartTime = "";
            PlayerData.myPlayerinfo.isAppRelease = false;
            PlayerData.myPlayerinfo.universityCode = "lobby";
            SwitchScene();
            */
            NativeManager.Instance.SendToUnity(
                "{\"type\":\"SwitchScene\",\"value\":{\"roomId\":\"\",\"scene\":\"lobby\",\"tableId\":\"TC02\",\"isAppRelease\":false,\"roomType\":\"\",\"roomTitle\":\"\",\"videoStartTime\":\"\",\"url\":\"https://test-torymeta-image.s3.ap-northeast-2.amazonaws.com/video/videoplayback.mp4\"}}"
            );
        }
    }
#endif
#endregion

}
