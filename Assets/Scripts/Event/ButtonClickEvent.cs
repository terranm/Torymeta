using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Agora_RTC_Plugin.API_Example.Examples.Basic.JoinChannelVideo;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

public class ButtonClickEvent : MonoBehaviour
{
    private bool flag;
    private bool mikeflag;
    private bool videoflag;
    
    public UnityEngine.UI.Image mikeBtn = null;
    public UnityEngine.UI.Image videoBtn = null;
    [SerializeField]
    private Sprite camoff = null;
    [SerializeField]
    private Sprite camon = null;
    [SerializeField]
    private Sprite micoff = null;
    [SerializeField]
    private Sprite micon = null;

    private void Start()
    {
        if (mikeBtn != null)
        {
            mikeflag = PlayerData.myPlayerinfo.universityCode.Equals("lobby");
            videoflag = mikeflag;
            if (mikeflag)
                mikeBtn.sprite = micon;
            else
                mikeBtn.sprite = micoff;

            if (videoflag)
                videoBtn.sprite = camon;
            else
                videoBtn.sprite = camoff;
        }

        GameEvents.Instance.OnLeaveRoom += LeaveRoomEvent;
        GameEvents.Instance.OnRequestNormalizeChatUI += NormalizeChatUI;
    }
    
    private void OnDestroy()
    {
        if (!flag)
            LeaveRoomEvent();
        //LeaveRoomEvent();

        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnLeaveRoom -= LeaveRoomEvent;
            GameEvents.Instance.OnRequestNormalizeChatUI -= NormalizeChatUI;
        }
    }

    private void NormalizeChatUI()
    {
        mikeBtn.sprite = micon;
        videoBtn.sprite = camon;
    }

    public void OnMessageBtn(GameObject chatPanel)
    {
        bool isActive = chatPanel.gameObject.activeSelf;
        chatPanel.gameObject.SetActive(!isActive);
    }

    public void OnMikeBtn()
    {
        ServerManager.Instance.VideoChatAudioMute(mikeflag);

        mikeflag = !mikeflag;
        if (mikeflag)
            mikeBtn.sprite = micon;
        else
            mikeBtn.sprite = micoff;
    }

    public void OnVideoBtn()
    {
        ServerManager.Instance.VideoChatVideoMute(videoflag);

        videoflag = !videoflag;
        if (videoflag)
            videoBtn.sprite = camon;
        else
            videoBtn.sprite = camoff;
    }

    public void OnFixedMinimapBtn()
    {
        GameEvents.Instance.RequestFixedMapActiveConvert();
    }
    
    public void OnChatListBtn()
    {
        NativeManager.Instance.SendChatLoungeListMessage();
    }

    public void OnParticipantListBtn()
    {
        ServerManager.Instance.ChatLoungeRoom.Send("findParticipant");
    }
    
    public void OnWorldBtn(GameObject universityPanel)
    {
        bool isActive = universityPanel.gameObject.activeSelf;
        universityPanel.gameObject.SetActive(!isActive);
    }

    public void OnEnterWorldBtn()
    {
        GameObject clickedButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        string universityCode = clickedButton.name;

        if (PlayerData.myPlayerinfo.universityCode == universityCode)
        {
            clickedButton.GetComponent<Image>().color = Color.gray;
            clickedButton.GetComponent<Button>().enabled = false;
            return;
        }

        PlayerData.myPlayerinfo.universityCode = universityCode;
        ServerManager.Instance.LeaveRooms();
        //SceneManager.LoadSceneAsync("world");
        GameEvents.Instance.RequestSceneChange();
        OnWorldBtn(clickedButton.transform.parent.parent.gameObject);
        // Debug.Log("Clicked");
    }

    public void OnExitBtn()
    {
        if (NetworkedEntityFactory.Instance.GetMine() != null)
        {
            if (NetworkedEntityFactory.Instance.GetMine().Table != "0")
            {
#if !UNITY_EDITOR
                NativeManager.Instance.SendChatLeaveConfirm();
#elif UNITY_EDITOR
                GameEvents.Instance.LeaveChatRoom();
#endif
                return;
            }
        }

        string sceneName = SceneManager.GetActiveScene().name;
        Debug.Log("sceneName " +sceneName);
        string description = "";
        switch (sceneName)
        {
            case "seminar":
                GameObject mediaPlayer = GameObject.Find("MediaPlayer");
                AVProMediaPlayerController _mediaPlayerController = mediaPlayer.GetComponent<AVProMediaPlayerController>();
                if (_mediaPlayerController.isMediaPlayerEnded)
                {
                    //description = "세미나실을 나가시겠습니까?";
                    LocalizationController.Instance.WaitLocaleText((localeText) => { description = localeText; }, "ButtonClickEvent_SeminarExit");
                }
                else
                {
                    //description = "세미나가 진행 중입니다. 세미나실을 나가시겠습니까?";
                    LocalizationController.Instance.WaitLocaleText((localeText) => { description = localeText; }, "ButtonClickEvent_SeminarProgressExit");
                }
                break;
            case "world":
                //description = "토리라운지로 이동하시겠습니까?";
                LocalizationController.Instance.WaitLocaleText((localeText) => { description = localeText; }, "ButtonClickEvent_WorldExit");
                break;
            default:
                //description = "토리메타 메인화면으로 이동하시겠습니까?";
                LocalizationController.Instance.WaitLocaleText((localeText) => { description = localeText; }, "ButtonClickEvent_DefaultExit");
                break;
        }
        Debug.Log("sceneName " + sceneName + " description " + description);
#if !UNITY_EDITOR
        NativeManager.Instance.SendLeaveRoomConfirm(description);
#elif UNITY_EDITOR
        LeaveRoomEvent();
#endif   
        Debug.Log("OnExitBtn");
    }

    private void LeaveRoomEvent()
    {
        flag = true;
        DownloadManager.Instance.StartLoadingProcess();
        ServerManager.Instance.LeaveRooms();
        string code = GameEvents.Instance.CheckUniversityCode();

        switch (code)
        {
            case "seminar":
                ServerManager.Instance.LeaveVideoChat();

//                 Debug.Log("LeaveRoomEvent " + code + " //" + PlayerData.myPlayerinfo.universityCode);
//                 PlayerData.myPlayerinfo.seminarUrl = "";
//                 PlayerData.myPlayerinfo.goToLoungeLocation = (int)MinimapController.TranportLocationPoint.none;
// #if UNITY_EDITOR
//                 NativeManager.Instance.SendToUnity(
//                     "{\"type\":\"LoungeVideoUrl\",\"value\":{\"url\":\"https://contents.ttceducation.net/2023%20%EC%A4%8C%20%EA%B0%95%EC%9D%98/2023_%EC%97%AC%EB%A6%84%ED%95%99%EA%B8%B0/%EB%B3%B8%EC%82%AC/%5B%EC%97%AC%EB%A6%84%EB%B0%A9%ED%95%99%5D%20ETS%20%ED%86%A0%EC%9D%B5%20%EB%8B%A8%EA%B8%B0%EA%B3%B5%EB%9E%B5%20850+%20RC/%EC%98%A4%ED%9B%84%EB%B0%98/3%EC%B0%A8%EC%8B%9C(7.5).mp4\"}}");
// #elif UNITY_IOS || UNITY_ANDROID
//                 NativeManager.Instance.SendRequireLoungeVideoUrl();
// #endif
//                 StartCoroutine(WaitForLoungeVideoUrl(code));
                ServerManager.Instance.LeaveChatRoom();
                StartCoroutine(nameof(SendBackMsg));
                break;

            case "lobby":
                ServerManager.Instance.LeaveChatRoom();

                //flag = true;
                StartCoroutine(nameof(SendBackMsg));
                break;

            case "world":
                Debug.Log("LeaveRoomEvent " + code + " //" + PlayerData.myPlayerinfo.universityCode);
                PlayerData.myPlayerinfo.seminarUrl = "";
                PlayerData.myPlayerinfo.goToLoungeLocation = (int)MinimapController.TranportLocationPoint.none;
#if UNITY_EDITOR
                NativeManager.Instance.SendToUnity(
                    "{\"type\":\"LoungeVideoUrl\",\"value\":{\"url\":\"https://contents.ttceducation.net/2023%20%EC%A4%8C%20%EA%B0%95%EC%9D%98/2023_%EC%97%AC%EB%A6%84%ED%95%99%EA%B8%B0/%EB%B3%B8%EC%82%AC/%5B%EC%97%AC%EB%A6%84%EB%B0%A9%ED%95%99%5D%20ETS%20%ED%86%A0%EC%9D%B5%20%EB%8B%A8%EA%B8%B0%EA%B3%B5%EB%9E%B5%20850+%20RC/%EC%98%A4%ED%9B%84%EB%B0%98/3%EC%B0%A8%EC%8B%9C(7.5).mp4\"}}");
#elif UNITY_IOS || UNITY_ANDROID
                NativeManager.Instance.SendRequireLoungeVideoUrl();
#endif
                StartCoroutine(WaitForLoungeVideoUrl(code));
                break;
        }
    }

    private IEnumerator WaitForLoungeVideoUrl(string currentSceneName)
    {
        if (GameEvents.Instance.CheckUniversityCode().Equals("seminar"))
        {
            while (AgoraManager.Instance.RtcEngine != null)
            {
                new WaitForEndOfFrame();
            }
        }

        while (PlayerData.myPlayerinfo.seminarUrl == "")
        {
            yield return new WaitForEndOfFrame();
        }
        PlayerData.myPlayerinfo.universityCode = "lobby";
        switch (currentSceneName)
        {
            case "seminar":
                PlayerData.myPlayerinfo.goToLoungeLocation = (int)MinimapController.TranportLocationPoint.seminarLobbyIn;
                break;
            case "world":
            default:
                PlayerData.myPlayerinfo.goToLoungeLocation = (int)MinimapController.TranportLocationPoint.lounge;
                break;
        }
        GameEvents.Instance.RequestSceneChange();
        yield return null;
    }

    /// <summary>
    /// 리펙토링 이전 버전으로 안쓰이는 상태
    /// </summary>
    public void OnExitBtn_nonstatic()
    {
        MMOManager.Instance.LeaveRooms();
        // Application.Unload();
        //AOSNativeAPI.SendMessage_nonStatic(CallMessageType.onExitToryMeta);
    }

    public void ScreenSizeBtn()
    {
        Debug.Log("ScreenSizeBtn");
    }
    
    IEnumerator SendBackMsg()
    {
        while(!ServerManager.Instance.endflag)
        {
            yield return new WaitForFixedUpdate();
        }

        PlayerData.myPlayerinfo.backSign = true;
        PlayerData.myPlayerinfo.universityCode = "AvatarView";
#if UNITY_ANDROID && !UNITY_EDITOR
        Screen.orientation = ScreenOrientation.Portrait;
#endif
        yield return SceneManager.LoadSceneAsync("AvatarView");
    }

    //private void Update()
    //{
    //    if (flag)
    //    {
    //        if (ServerManager.Instance.endflag)
    //        {
    //            StartCoroutine(nameof(SendBackMsg));
    //            flag = false;
    //        }
    //    }
    //}
    
    private void OnApplicationQuit()
    {
        if (PlayerData.myPlayerinfo.universityCode.Equals("seminar"))
        {
            ServerManager.Instance.LeaveRooms();
            ServerManager.Instance.LeaveVideoChat();
        }
    }
}