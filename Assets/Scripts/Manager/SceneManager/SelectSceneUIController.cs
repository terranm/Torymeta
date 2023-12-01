using System;
using System.Collections;
using System.Collections.Generic;
using Colyseus;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectSceneUIController : MonoBehaviour
{
    private GameObject controllerObject;
    private TouchController touchController;

    private CharctorMeshAndMaterialController meshMatCtrl;
    private void Awake()
    {
        NativeManager.Instance.SendUnityLifecycleState(NativeManager.LifeCycle.Awake);
    }

    private void Start()
    {
        Debug.Log("Start : SelectScene");
        //GameEvents.Instance.OnRequestSceneChange += SwitchScene;
        GameEvents.Instance.OnRequestCharacterSet += SelectView_Select;
        GameEvents.Instance.OnRequestAvatarZoom += View_ZoomIn;
        GameEvents.Instance.OnRequestAvatarRotate += View_Rotation;
        //GameEvents.Instance.OnRequestCamMove += View_UpDown;
        GameEvents.Instance.OnRequestSetAvatarInfo += SetAvatar;


        controllerObject = GameObject.FindGameObjectWithTag("AvatarContainer");
        touchController = controllerObject.GetComponent<TouchController>();
        meshMatCtrl = touchController.GetAvatar().GetComponent<CharctorMeshAndMaterialController>();

        StartCoroutine(WaitContainer());
    }

    private IEnumerator WaitContainer()
    {
        while (GameObject.Find("MeshMatContainer(Clone)") == null)
        {
            yield return new WaitForUpdate();
        }

        SetAvatar();

        NativeManager.Instance.SendUnityLifecycleState(NativeManager.LifeCycle.Start);
        touchController.GetAvatar().transform.position = Vector3.zero;
    }

    private void OnDestroy()
    {
        if (GameEvents.Instance != null)
        {
            //GameEvents.Instance.OnRequestSceneChange -= SwitchScene;
            GameEvents.Instance.OnRequestCharacterSet -= SelectView_Select;
            GameEvents.Instance.OnRequestAvatarZoom -= View_ZoomIn;
            GameEvents.Instance.OnRequestAvatarRotate -= View_Rotation;
            //GameEvents.Instance.OnRequestCamMove -= View_UpDown;
            GameEvents.Instance.OnRequestSetAvatarInfo -= SetAvatar;
        }
        
        if(NativeManager.Instance != null)NativeManager.Instance.SendUnityLifecycleState(NativeManager.LifeCycle.OnDestroy);
    }


    //=============================================Region Divider======================================

    #region Game Event handler Region

    private void SetAvatar()
    {
        touchController.AvatarReset();
        touchController.ResetZoom();
        touchController.ResetRotation();
        touchController.ResetPosition();

        meshMatCtrl.CharacterSetting();
        //touchController.
    }


    //public void SwitchScene()
    //{
    //    LoadScene();

    //    if (PlayerData.myPlayerinfo.universityCode.Equals("SelectView") || PlayerData.myPlayerinfo.universityCode.Equals("AvatarView"))
    //    {
    //        Screen.fullScreen = false;
    //    }
    //}
    
    public void SelectView_Select(string part)
    {
        /*
        if (string.IsNullOrEmpty(PlayerData.myPlayerinfo.avatar))
            touchController.Select("100_11");
        else
            touchController.Select(PlayerData.myPlayerinfo.avatar);
        */
        Debug.Log(PlayerData.myPlayerinfo.avatar);

        string code = "";
        string color = "";
        float zoom = 10f;
        float hori = 0f;
        switch (part)
        {
            case "HAIR":// 헤어 코드 2001 ~ 2008, 헤어 컬러 8개
                code = PlayerData.myPlayerinfo.state.hairCode;
                color = PlayerData.myPlayerinfo.state.hairColorCode;
                hori = 3.5f;
                break;
            case "SKIN":// 스킨 코드 7001, 스킨 컬러 16개
                code = PlayerData.myPlayerinfo.state.skinCode;
                color = PlayerData.myPlayerinfo.state.skinColorCode;
                zoom = 30;
                hori = 2.12f;
                break;
            case "FACE":// 페이스 코드 3001
                code = PlayerData.myPlayerinfo.state.faceCode;
                hori = 3.5f;
                break;
            case "TOP":// 탑 코드 4001~4008
                code = PlayerData.myPlayerinfo.state.topCode;
                hori = 2.5f;
                break;
            case "BOTTOM":// 바텀 코드 5001~5008
                code = PlayerData.myPlayerinfo.state.bottomCode;
                hori = 1.5f;
                zoom = 20f;
                break;
            case "SHOES":// 슈즈 코드 6001~6004
                code = PlayerData.myPlayerinfo.state.shoesCode;
                hori = 0.4f;
                break;
            default:
                Debug.Log("partially Okay");
                break;
        }
        touchController.ResetRotation();
        touchController.ResetPositionNoneY();

        touchController.Zoom(zoom);
        touchController.MoveHorizontal(hori);

        meshMatCtrl.CharactorPartsChange(code, color);

        //touchController.ResetZoom();
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


    public void View_UpDown(bool isUp)
    {
        if (!isUp)
            touchController.MoveUpHorizontal();
        else
            touchController.MoveDownHorizontal();
    }

    //private void LoadScene()
    //{
    //    StartCoroutine(LoadSceneAsync());
    //}

    //private IEnumerator LoadSceneAsync()
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
        
        // AvatarView
        if (Input.GetKeyDown(KeyCode.Q))
        {
            NativeManager.Instance.SendToUnity(
                "{\"type\":\"SwitchScene\",\"value\":{\"url\":\"\",\"roomType\":\"\",\"roomId\":\"\",\"roomTitle\":\"\",\"timeDiff\":0,\"isAppRelease\":false,\"scene\":\"AvatarView\",}}"
            );
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            NativeManager.Instance.SendToUnity(
                "{\"type\":\"AvatarInfo\",\"value\":{\"avatarState\":{\"bodyType\":\"BODY_AA_001\",\"bottomCode\":\"BOTTOM_AA_001\",\"bottomColorCode\":\"#000000\",\"faceCode\":\"FACE_AA_001\",\"faceColorCode\":\"#fae7d6\",\"hairCode\":\"HAIR_AA_001\",\"hairColorCode\":\"#434343\",\"shoesCode\":\"SHOES_AA_001\",\"shoesColorCode\":\"#000000\",\"skinCode\":\"SKIN_AA_001\",\"skinColorCode\":\"#fae7d6\",\"topCode\":\"TOP_AA_001\",\"topColorCode\":\"#000000\"},\"memberId\":14,\"userName\":\"토리맨\"}}"
            );
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            NativeManager.Instance.SendToUnity(
                "{\"type\":\"CharacterSet\",\"value\":{\"characterId\":\"BODY_AA_001\",\"color\":\"#d13319\",\"item\":\"HAIR_AA_001\"}}"
            );
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            NativeManager.Instance.SendToUnity(
                "{\"type\":\"CharacterSet\",\"value\":{\"characterId\":\"BODY_AA_001\",\"color\":\"#fae7d6\",\"item\":\"HAIR_AA_001\"}}"
            );
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            NativeManager.Instance.SendToUnity(
                "{\"type\":\"CharacterSet\",\"value\":{\"characterId\":\"BODY_AA_001\",\"color\":\"#fae7d6\",\"item\":\"BOTTOM_AC_001\"}}"
            );
        }

        else if (Input.GetKeyDown(KeyCode.L))
        {
            NativeManager.Instance.SendToUnity(
                "{\"type\":\"CharacterSet\",\"value\":{\"characterId\":\"BODY_AA_001\",\"color\":\"#fae7d6\",\"item\":\"BOTTOM_AC_002\"}}"
            );
        }
        //else if (Input.GetKeyDown(KeyCode.K))
        //{
        //    PlayerData.myPlayerinfo.avatar = "100_18";
        //    SelectView_Select();
        //}
    }
#endif
    #endregion
}
