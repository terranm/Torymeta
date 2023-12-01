
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Colyseus;
using Newtonsoft.Json;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbySceneUIController : MonoBehaviour
{
    [SerializeField] private GameObject mediaPlayer;
    [SerializeField] private GameObject mediaPlayerPanel;
    
    [SerializeField] private Canvas _canvasTouchZone;
    [SerializeField] private Canvas _canvasJoystick;
    [SerializeField] private Canvas _canvasVideoScreen;
    //[SerializeField] private GameObject _objZoomOutButton;
    
    [SerializeField] private GameObject _objJumpButton;
    [SerializeField] private GameObject _objSprintButton;
    [SerializeField] private GameObject _objJoystickButton;
    [SerializeField] private GameObject _objButtonFrame;
    [SerializeField] private GameObject _objButtonFrameForChat;
    [SerializeField] private GameObject _objChatLoungeInfoPanel;
    [SerializeField] private TMP_Text _txtChatLoungeMemberCount;
    [SerializeField] private GameObject _objChatLoungeInfoPanelLockImage;
    [SerializeField] private GameObject _objEmotion;
    
    [SerializeField] private GameObject _objEmotionSit;
    [SerializeField] private InputField _inputChat;
    //[SerializeField] private GameObject _objJump;
    
    [SerializeField] private Image _imgtarget;
    [SerializeField] private Camera _camera;    
    [SerializeField] private CinemachineVirtualCamera _cineCam;
    
    [SerializeField] private GameObject _RoomIcon;
    
    [SerializeField] private GameObject[] _objMute;
    [SerializeField] private GameObject[] _objUnmute;
    [SerializeField] private AVProMediaPlayerController _mediaPlayerController;

    [SerializeField] private MinimapController _minimapController;

    class ChatLoungeform
    {
        public GameObject table;
        public ChatLoungeController Controller;
    }
    
    private List<ChatLoungeform> ChatLoungeformList;
    private List<RoomInfo> CurrentRoomList;

    private int warningCount;
    
    private void Awake()
    {
        //_canvasVideoScreen.gameObject.SetActive(false);
        _canvasVideoScreen.enabled = false;
        mediaPlayer = GameObject.Find("MediaPlayer");
        mediaPlayerPanel = GameObject.Find("MediaPlayerPanel");
        //mediaPlayerPanel.SetActive(false);
        //mediaPlayer.SetActive(false);
        _mediaPlayerController = mediaPlayer.GetComponent<AVProMediaPlayerController>();
        
        NativeManager.Instance.SendUnityLifecycleState(NativeManager.LifeCycle.Awake);
    }

    private void Start()
    {
        GameEvents.Instance.OnRequestZooom += ZoomVideo;
        GameEvents.Instance.OnCompleteLoadScene += OnCompleteLoadScene;
        GameEvents.Instance.OnRequestSetVideoCanvas += SetCamCanvas;
        GameEvents.Instance.OnLeaveChatRoom += LeaveChatRoom;
        GameEvents.Instance.OnChangeUIBySit += ChangeUIBySit;
        GameEvents.Instance.OnClickTeleportObj += Teleport;
        GameEvents.Instance.OnRequestSilencePenalty += PerformSilencePenalty;
        GameEvents.Instance.OnRequestWarningPenalty += PerformWarningPenalty;
        GameEvents.Instance.OnClickMediaPlayerMuteBtn += MediaPlayerMute;
        //GameEvents.Instance.OnRequestSceneChange += SwitchScene;

        CurrentRoomList = new List<RoomInfo>();
        ServerManager.Instance.ChatLoungeProcessDone = true;
        warningCount = 0;

        NativeManager.Instance.SendUnityLifecycleState(NativeManager.LifeCycle.Start);
    }

    private void OnDestroy()
    {
        ServerManager.Instance.ChatLoungeProcessDone = true;
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnRequestZooom -= ZoomVideo;
            GameEvents.Instance.OnCompleteLoadScene -= OnCompleteLoadScene;
            GameEvents.Instance.OnRequestSetVideoCanvas -= SetCamCanvas;
            GameEvents.Instance.OnLeaveChatRoom -= LeaveChatRoom;
            GameEvents.Instance.OnChangeUIBySit -= ChangeUIBySit;
            GameEvents.Instance.OnClickTeleportObj -= Teleport;
            GameEvents.Instance.OnRequestSilencePenalty -= PerformSilencePenalty;
            GameEvents.Instance.OnRequestWarningPenalty -= PerformWarningPenalty;
            GameEvents.Instance.OnClickMediaPlayerMuteBtn -= MediaPlayerMute;
            //GameEvents.Instance.OnRequestSceneChange -= SwitchScene;
        }
        
        if(NativeManager.Instance != null)NativeManager.Instance.SendUnityLifecycleState(NativeManager.LifeCycle.OnDestroy);
    }


    //=============================================Region Divider======================================

    #region Game Event handler Region

    private void MediaPlayerMute()
    {
        //음소거 기능 수행 & 이미지 변환
        bool muteState = _mediaPlayerController.AudioMuteConvert();
        foreach (var obj in _objMute)
        {
            obj.SetActive(muteState);
        }

        foreach (var obj in _objUnmute)
        {
            obj.SetActive(!muteState);
        }
    }

    private void PerformWarningPenalty()
    {
        warningCount++;
        if (warningCount > 2)
        {
            ServerManager.Instance.Room.Send("sendChat", string.Format("#SYSMSG#{0}#RoomID#{1}#Entry#{2}",
                ServerManager.Instance.ChatLoungeRoom.Id, "LobbySceneUIController_ChatPenaltyWarningKick", PlayerData.myPlayerinfo.userName));
            //"님이 경고 3회로 채팅 방에서 강퇴되었습니다."

            GameEvents.Instance.LeaveChatRoom();
            LocalizationController.Instance.WaitLocaleText((localeText) => {
                NativeManager.Instance.SendBasicAlert(localeText);
            }, "LobbySceneUIController_AlertChatPenaltyWarningKick");
            //NativeManager.Instance.SendBasicAlert("경고 3회 누적으로 채팅방에서 강퇴되었습니다.");
            warningCount = 0;
        }
        else
        {
            // {0}님 경고{1}회 입니다.
            ServerManager.Instance.Room.Send("sendChat", string.Format("#SYSMSG#{0}#RoomID#{1}#Entry#{2}#Columns#{3}", 
                    ServerManager.Instance.ChatLoungeRoom.Id, "LobbySceneUIController_ChatPenaltyWarningCnt", PlayerData.myPlayerinfo.userName, warningCount.ToString()));
        }
    }

    private void PerformSilencePenalty()
    {
        //10초 동안 {0}님의 채팅입력이 제한됩니다.
        ServerManager.Instance.Room.Send("sendChat", string.Format("#SYSMSG#{0}#RoomID#{1}#Entry#{2}",
            ServerManager.Instance.ChatLoungeRoom.Id, "LobbySceneUIController_ChatPenaltySilence", PlayerData.myPlayerinfo.userName));

        StartCoroutine(SeizureChatInputField());
    }

    // 텔레포트 기능 구현 - 230713 유정균 // 텔레포트 관련 하여 캐릭터 이동 관련 기능을 끄고 이동시켜야하는 상황이므로 아바타 관련 코드가 정리된 후 진행필요 되도록 NetworkedEntityFactory에서 진행해야함 230802 유정균
    private void Teleport(GameObject hitObject)
    {
        if (!hitObject.layer.Equals(LayerMask.NameToLayer("Teleport"))) return;
        //GameEvents.Instance.RequestFixedMapActiveConvert();
        Transform targetTr = hitObject.transform.GetChild(0);
        _minimapController.MoveTo(targetTr.position,targetTr.rotation.eulerAngles);
    }
    
    private void LeaveChatRoom()
    {
        _objJumpButton.SetActive(true);
        _objSprintButton.SetActive(true);
        _objJoystickButton.SetActive(true);
        _objButtonFrame.SetActive(true);
        _objEmotion.SetActive(true);
        _objButtonFrameForChat.SetActive(false);
        _objChatLoungeInfoPanel.SetActive(false);
        _cineCam.m_Lens.FieldOfView = 40f;
        ServerManager.Instance.LeaveChatRoom();
        ServerManager.Instance.LeaveVideoChat();
        NetworkedEntityFactory.Instance.ClearChatCanvas();
        CheckChatLoungeList();
        NetworkedEntityFactory.Instance.GetMyEntity().SendRFC("Stand");
        warningCount = 0;
        _minimapController.gameObject.SetActive(true);
        StartCoroutine(SetMediaPlayerMute(false));
        GameEvents.Instance.RequstClearChatLog();
    }
        
    private void SetCamCanvas(int memberId, string channelId)
    {
        Debug.Log("SetCamCanvas call memberId : " + memberId + " at channel : " + channelId);
        NetworkedEntityFactory.Instance.SetEntityCanvasReadyByMemberId(memberId,channelId);
        if (memberId.Equals(PlayerData.myPlayerinfo.memberId))
        {
            _objJumpButton.SetActive(false);
            _objSprintButton.SetActive(false);
            _objJoystickButton.SetActive(false);
            _objButtonFrame.SetActive(false);
            _objEmotion.SetActive(false);
            _objButtonFrameForChat.SetActive(true);
            _cineCam.m_Lens.FieldOfView = 25f;
            CheckChatLoungeList(true);
            _minimapController.gameObject.SetActive(false);
            StartCoroutine(SetMediaPlayerMute(true));
            GameEvents.Instance.RequstClearChatLog();
        }
    }
    
    private void OnCompleteLoadScene()
    {
        StartCoroutine(SetChatTableReady());
        InvokeRepeating("ExcuteChatLoungeCheck", 0f, 2f);
        _minimapController.Init();
        StartCoroutine(SetPlayerSpawnPostion());

        //_canvasVideoScreen.gameObject.SetActive(true);
        //mediaPlayerPanel.SetActive(true);
        //mediaPlayer.SetActive(true);
        StartCoroutine(_mediaPlayerController.WaitSeekSetting());
        //StartCoroutine(SetMediaPlayerMute());        
        //if (_mediaPlayerController.AVMP.AudioMuted)
        //    _mediaPlayerController.AudioMuteConvert();
    }

    private void ZoomVideo(bool zoomIn)
    {
        //_imgtarget.raycastTarget = (!zoomIn);
        _canvasTouchZone.enabled = (!zoomIn);
        _canvasJoystick.enabled = (!zoomIn);
        _canvasVideoScreen.enabled = (zoomIn);
    }

    public void SetChatRoomName(string Roomname)
    {
        Text text = _objButtonFrameForChat.transform.GetComponentInChildren<Text>();
        LocalizationController.Instance.WaitLocaleText((localeText) => {
            if (Roomname.Length < 9)
                text.text = "<color=white>" + localeText + "</color>\n" + Roomname;
            else
                text.text = "<color=white>" + localeText + "</color>\n" + Roomname.Substring(0, 8) + "..."; ;
        }, "LobbySceneUIController_ChatLounge");
    }
    
    private void ChangeUIBySit(bool sit)
    {
        _objEmotion.SetActive(!sit);
        _objEmotionSit.SetActive(sit);
        _objJumpButton.SetActive(!sit);
        _objSprintButton.SetActive(!sit);
    }

    #endregion

    //public void SwitchScene()
    //{
    //    LoadScene();

    //    if (PlayerData.myPlayerinfo.universityCode.Equals("SelectView") || PlayerData.myPlayerinfo.universityCode.Equals("AvatarView"))
    //    {
    //        Screen.fullScreen = false;
    //    }
    //    else
    //    {
    //        if (string.IsNullOrEmpty(PlayerData.myPlayerinfo.avatar))
    //            PlayerData.myPlayerinfo.avatar = "100_11";
    //        if (PlayerData.myPlayerinfo.memberId.Equals(0))
    //            PlayerData.myPlayerinfo.memberId = 123;
    //        if (string.IsNullOrEmpty(PlayerData.myPlayerinfo.userName))
    //            PlayerData.myPlayerinfo.userName = "unityTest";
    //        ServerManager.Instance.TryConnect(PlayerData.myPlayerinfo.universityCode,
    //            (error) => { Debug.LogError($"Login 에러 l {error} l"); });

    //        Screen.fullScreen = true;
    //    }
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


    //=============================================Region Divider======================================

    #region Script for test on UnityEditor Env region
#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            //비공개방 만들기
            if (ServerManager.Instance.ClickedChattingLoungeOBJ != null)
                NativeManager.Instance.SendToUnity(
                    "{\"type\":\"ChatCreate\",\"value\":{\"roomname\":\"privatroombyscript\",\"isprivate\":true,\"password\":\"123123\"}}"
                    );
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            //공개방 만들기
            if (ServerManager.Instance.ClickedChattingLoungeOBJ != null)
                NativeManager.Instance.SendToUnity(
                    "{\"type\":\"ChatCreate\",\"value\":{\"roomname\":\"privatroombyscript\",\"isprivate\":false,\"password\":\"\"}}"
                );
        }
        
        if (Input.GetKeyDown(KeyCode.D))
        {
            //GameEvents.Instance.RequestSilencePenalty();
            NativeManager.Instance.SendToUnity(
                "{\"type\":\"DoAction\",\"value\":{\"actionId\":\"SilenceAction\"}}"
            );
        }
        
        if (Input.GetKeyDown(KeyCode.F))
        {NativeManager.Instance.SendToUnity(
                "{\"type\":\"DoAction\",\"value\":{\"actionId\":\"WarningAction\"}}"
            );
        }
        
        if (Input.GetKeyDown(KeyCode.G))
        {NativeManager.Instance.SendToUnity(
                "{\"type\":\"DoAction\",\"value\":{\"actionId\":\"KickAction\"}}"
            );
        }
        
        if (Input.GetKeyDown(KeyCode.H))
        {NativeManager.Instance.SendToUnity(
                "{\"type\":\"JoinChat\",\"value\":{\"tableId\":\"TC02\"}}"
            );
        }
        
        if (Input.GetKeyDown(KeyCode.J))
        {
            InviteFriendInfo data = new InviteFriendInfo();
            data.friendIds = new List<int>();
            data.friendIds.Add(77);
            ServerManager.Instance.ChatLoungeRoom.Send("invite", new Dictionary<string, object>()
            {
                {
                    "data", JsonConvert.SerializeObject(data)
                }
            });
        }
    }
#endif
    #endregion    
    
    //=============================================Region Divider======================================

    #region internal functions

    private IEnumerator SetChatTableReady()
    {
        while (!DownloadManager.Instance.isDownComplete)
        {
            yield return new WaitForEndOfFrame();
        }
        ChatLoungeformList = new List<ChatLoungeform>();
        GameObject[] canvases = GameObject.FindGameObjectsWithTag("LoungeTableChatPanel");
        if (canvases != null)
        {
            foreach (var item in canvases)
            {
                GameObject roomIcon;
                roomIcon = Instantiate(_RoomIcon, Vector3.zero, quaternion.identity);
                roomIcon.transform.SetParent(item.transform);
                roomIcon.transform.localPosition = new Vector3(0, 3.0f, 0);
                
                ChatLoungeform temp = new ChatLoungeform();
                temp.table = item.transform.parent.gameObject;
                temp.Controller = roomIcon.GetComponent<ChatLoungeController>();

                temp.Controller.SetInfo(null,item.transform.parent.gameObject);
                
                ChatLoungeformList.Add(temp);
            }
        }
        
        _objChatLoungeInfoPanel.SetActive(false);
        yield return null;
    }

    private IEnumerator SetPlayerSpawnPostion()
    {
        while (!NetworkedEntityFactory.Instance.GetMine().isInitializeComplete)
        {
            yield return new WaitForEndOfFrame();
        }
        if (PlayerData.myPlayerinfo.goToLoungeLocation == (int)MinimapController.TranportLocationPoint.none)
            PlayerData.myPlayerinfo.goToLoungeLocation = (int)MinimapController.TranportLocationPoint.lounge;
        
        if (PlayerData.myPlayerinfo.seminarType.Equals("festival"))
        {
            _minimapController.MoveTo(new Vector3(-86f, 0f, 131f), new Vector3(0f, 312f, 0f));
            PlayerData.myPlayerinfo.seminarType = "";
        }
        else
        {
            Transform tr = _minimapController.TransportList[PlayerData.myPlayerinfo.goToLoungeLocation];
            _minimapController.MoveTo(tr.position, tr.eulerAngles);
        }

        PlayerData.myPlayerinfo.goToLoungeLocation = (int)MinimapController.TranportLocationPoint.none;

        yield return null;
    }

    private IEnumerator SetMediaPlayerMute(bool muted)
    {
        while (!NetworkedEntityFactory.Instance.GetMine().isInitializeComplete)
        {
            yield return new WaitForEndOfFrame();
        }
        if (muted != _mediaPlayerController.AVMP.AudioMuted)
            MediaPlayerMute();

        yield return null;
    }

    private void ExcuteChatLoungeCheck()
    {
        CheckChatLoungeList();
    }
    
    private void CheckChatLoungeList(bool Setting = false)
    {
        ServerManager.Instance.GetChattingLoungeList<ChattingLoungeListResponse>(
            delegate(RequestResponse response)
            {
                ChattingLoungeListResponse chattingLoungeListResponse = (ChattingLoungeListResponse)response;
                if (chattingLoungeListResponse.result.rooms != null)
                {
                    CurrentRoomList = chattingLoungeListResponse.result.rooms.ToList();
                    UpdateChatLoungeRoomInfo();

                    foreach (var info in CurrentRoomList)
                    {
                        if (NetworkedEntityFactory.Instance.GetMine() != null)
                        {
                            if (info.tableId.Equals(NetworkedEntityFactory.Instance.GetMyEntity().Table))
                            {
                                if (Setting)
                                {
                                    SetChatRoomName(info.title);
                                    _objChatLoungeInfoPanelLockImage.SetActive(info.privateRoom);
                                    _txtChatLoungeMemberCount.text =
                                        string.Format("{0}/{1}", info.clients, info.maxClients);
                                    _objChatLoungeInfoPanel.SetActive(true);
                                }

                                if (_objChatLoungeInfoPanel.activeSelf)
                                    _txtChatLoungeMemberCount.text =
                                        string.Format("{0}/{1}", info.clients, info.maxClients);
                                break;
                            }
                        }
                    }
                }
            });
    }

    private void UpdateChatLoungeRoomInfo()
    {
        bool match = false;
        
        //정보 갱신
        foreach (var form in ChatLoungeformList)
        {
            match = false;
            foreach (var info in CurrentRoomList)
            {
                if (info.tableId.Equals(form.table.name))
                {
                    match = true;
                    form.Controller.SetInfo(info,form.table);
                    break;
                }
            }

            if (!match)
            {
                form.Controller.SetInfo(null, form.table);
            }
            if(!form.Controller.isInitComplete) StartCoroutine(form.Controller.Init());
        }
    }

    private IEnumerator SeizureChatInputField()
    {
        float timer = 0;
        while (timer < 10)
        {
            _inputChat.interactable = false;
            yield return new WaitForSeconds(0.5f);
            timer += 0.5f;
        }
        _inputChat.interactable = true;
        //{#침묵당한사람 닉네임}님의 채팅방 사용 제한이 해제되었습니다.
        ServerManager.Instance.Room.Send("sendChat", string.Format("#SYSMSG#{0}#RoomID#{1}#Entry#{2}",
            ServerManager.Instance.ChatLoungeRoom.Id, "LobbySceneUIController_ChatPenaltySilenceDone", PlayerData.myPlayerinfo.userName));

        yield return null;
    }

    #endregion

}
