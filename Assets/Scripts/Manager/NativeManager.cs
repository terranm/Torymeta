using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Colyseus;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Settings;
using System.Threading.Tasks;

public class NativeManager : Singleton<NativeManager>
{
    
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    public static extern void sendMessageToMobileApp(string message);
#endif
    public enum LifeCycle
    {
        Awake=0,
        Start,
        OnDestroy,
    }
    
    
    private GameObject TargetPlayer;

    private ButtonForm btnCancel;

    private DateTime prevSendTime;
    private string prevSendMsg;
    private DateTime prevRecvTime;
    private string prevRecvMsg;

    public string gotoRoomId;
    public bool flagGetMemberInfo;

    private string btnConfirmLocaleText = "";

    private void Start()
    {
        btnCancel = new ButtonForm();
        //공통으로 사용하는 취소버튼 하나로 사용
        //btnCancel.title = "취소";
        btnCancel.color = "#000000";
        btnCancel.actionId = "";

        prevSendTime = DateTime.Now;
        prevRecvTime = DateTime.Now;
        prevSendMsg = "";
        prevRecvMsg = "";

        flagGetMemberInfo = false;
        //StartCoroutine(LocalizationController.Instance.WaitLocaleText((localeText) => { btnConfirmLocaleText = localeText; }, "NativeManager_ModalDone"));
        LocalizationController.Instance.WaitLocaleText((localeText) => { btnConfirmLocaleText = localeText; }, "NativeManager_ModalDone");
        LocalizationController.Instance.WaitLocaleText((localeText) => { btnCancel.title = localeText; }, "NativeManager_ModalCancel");
        //StartCoroutine(LocalizationController.Instance.WaitLocaleText((localeText) => { btnCancel.title = localeText; }, "NativeManager_ModalCancel"));
    }


    private void Update()
    {
        //Debug.Log(btnConfirmLocaleText);
    }
    #region Native To Unity

    public void SendToUnity(string msg)
    {
        if((DateTime.Now - prevRecvTime).Milliseconds < 500 && msg.Equals(prevRecvMsg))
            return;
        
        //네이티브에서 유니티로 오는 메세지 통일
        Debug.Log(Application.platform + " Native To Unity Send Message : " + msg);
        BasicMessage data = JsonConvert.DeserializeObject<BasicMessage>(msg);

        switch (data.type)
        {
            case "CharacterSet" :
                DeserializeCharacterSetMSG(data.value);
                break;
            case "SwitchScene" :
                DeserializeSwitchSceneMSG(data.value);
                break;
            case "SwitchSceneSimple" :
                DeserializeSwitchSceneSimpleMSG(data.value);
                break;
            case "ChatCreate" :
                DeserializeChatCreateMSG(data.value);
                break;
            case "AvatarInfo" :
                DeserializeAvatarInfoMSG(data.value);
                break;
            case "DoAction" :
                DeserializeDoActionMSG(data.value);
                break;
            case "ChatInvite" :
                DeserializeChatInviteMSG(data.value);
                break;
            case "JoinChat":
                DeserializeJoinChatMSG(data.value);
                break;
            case "AvatarRotate" :
                GameEvents.Instance.RequestAvatarRotate();
                break;
            case "ZoomIn" :
                GameEvents.Instance.RequestAvatarZoom(true);
                break;
            case "ZoomOut" :
                GameEvents.Instance.RequestAvatarZoom(false);
                break;
            case "CamUp" :
                GameEvents.Instance.RequestCamMove(true);
                break;
            case "CamDown" :
                GameEvents.Instance.RequestCamMove(false);
                break;
            case "Kick":
                //StartCoroutine(LocalizationController.Instance.WaitLocaleText((localeText) => { SendConfirm("KickAction", localeText); ; }, "NativeManager_ModalDescript_Kick"));
                LocalizationController.Instance.WaitLocaleText((localeText) => { SendConfirm("KickAction", localeText); ; }, "NativeManager_ModalDescript_Kick");
                break;
            case "Mandate":
                //StartCoroutine(LocalizationController.Instance.WaitLocaleText((localeText) => { SendConfirm("MandateAction", localeText); ; }, "NativeManager_ModalDescript_Mandate"));
                LocalizationController.Instance.WaitLocaleText((localeText) => { SendConfirm("MandateAction", localeText); ; }, "NativeManager_ModalDescript_Mandate");
                break;
            case "Silence":
                //StartCoroutine(LocalizationController.Instance.WaitLocaleText((localeText) => { SendConfirm("SilenceAction", localeText); ; }, "NativeManager_ModalDescript_Silence"));
                LocalizationController.Instance.WaitLocaleText((localeText) => { SendConfirm("SilenceAction", localeText); ; }, "NativeManager_ModalDescript_Silence");
                break;
            case "Warning":
                //StartCoroutine(LocalizationController.Instance.WaitLocaleText((localeText) => { SendConfirm("WarningAction", localeText); ; }, "NativeManager_ModalDescript_Warning"));
                LocalizationController.Instance.WaitLocaleText((localeText) => { SendConfirm("WarningAction", localeText); ; }, "NativeManager_ModalDescript_Warning");
                break;
            case "ChatJoin" :
                ServerManager.Instance.JoinChattRoomByIdWithPassword(ServerManager.Instance.ClickedChattingLoungeRoomInfo.password);
                break;
            case "RequestInvitable":
                ServerManager.Instance.ChatLoungeRoom.Send("findFriend");
                break;
            case "LoungeVideoUrl":
                DeserializeLoungeVideoUrl(data.value);
                break;
            case "PassMember":
                DeserializeMemberInfo(data.value);
                break;
            case "JoinByPush":
                DeserializeJoinByPushInfo(data.value);
                break;
            case "DisconnectServer":
                ServerManager.Instance.DisconnectAll();
                break;
            default:
                Debug.Log("Message type [" + data.type + "] from Native can't identify");
                break;
        }
        
        prevRecvTime = DateTime.Now;
        prevRecvMsg = msg;
    }


    //private IEnumerator WaitLocaleTextForAction(string action, string entry)
    //{
    //    while (!LocalizationController.Instance.isChangeLocaleComplete)
    //    {
    //        yield return new WaitForSeconds(1);
    //    }
    //    LocalizationController.Instance.GetLocalizeText(entry);
    //    while (LocalizationController.Instance.Result == "")
    //    {
    //        yield return new WaitForFixedUpdate();
    //    }
    //    SendConfirm(action, LocalizationController.Instance.Result);
    //    LocalizationController.Instance.Result = "";

    //    yield return null;
    //}

    

    //캐릭터 수정 신호 처리
    private void DeserializeCharacterSetMSG(object value)
    {
        CharacterSelect data = JsonConvert.DeserializeObject<CharacterSelect>(value.ToString());
        if (!string.IsNullOrEmpty(data.characterId))
            PlayerData.myPlayerinfo.avatar = data.characterId;//없어 질거
        if (!string.IsNullOrEmpty(data.item))
        {
            if (PlayerData.myPlayerinfo.state == null)
            {
                PlayerData.myPlayerinfo.state = new();
            }
            string[] id = data.item.Split("_");
            switch (id[0])
            {
                case "HAIR":// 헤어 코드 2001 ~ 2008, 헤어 컬러 8개
                    PlayerData.myPlayerinfo.state.hairCode = data.item;
                    PlayerData.myPlayerinfo.state.hairColorCode = data.color;
                    break;
                case "SKIN":// 스킨 코드 7001, 스킨 컬러 16개
                    PlayerData.myPlayerinfo.state.skinCode = data.item;
                    PlayerData.myPlayerinfo.state.skinColorCode = data.color;
                    break;
                case "FACE":// 페이스 코드 3001
                    PlayerData.myPlayerinfo.state.faceCode = data.item;
                    break;
                case "TOP":// 탑 코드 4001~4008
                    PlayerData.myPlayerinfo.state.topCode = data.item;
                    break;
                case "BOTTOM":// 바텀 코드 5001~5008
                    PlayerData.myPlayerinfo.state.bottomCode = data.item;
                    break;
                case "SHOES":// 슈즈 코드 6001~6004
                    PlayerData.myPlayerinfo.state.shoesCode = data.item;
                    break;
                default:
                    Debug.Log("partially Okay");
                    break;
            }
            GameEvents.Instance.RequestCharacterSet(id[0]);
        }
    }
    
    //씬이동 신호 처리
    private void DeserializeSwitchSceneMSG(object value)
    {
        SwitchSceneForm data = JsonConvert.DeserializeObject<SwitchSceneForm>(value.ToString());

        //GameEvents.Instance.currentUnivCode = PlayerData.myPlayerinfo.universityCode;
        //GameEvents.Instance.destUnivCode = data.scene;

        PlayerData.myPlayerinfo.seminarUrl = data.url;
        PlayerData.myPlayerinfo.seminarType = data.roomType;
        PlayerData.myPlayerinfo.seminarTitle = data.roomTitle;
        PlayerData.myPlayerinfo.seminarId = data.roomId;
        PlayerData.myPlayerinfo.videoStartTime = data.videoStartTime;
        PlayerData.myPlayerinfo.isAppRelease = data.isAppRelease;
        ServerManager.Instance.isAppRelease = data.isAppRelease;
        PlayerData.myPlayerinfo.universityCode = data.scene;
        PlayerData.myPlayerinfo.goToTableId = data.tableId;
        GameEvents.Instance.RequestSceneChange();
    }
    
    //씬이동 신호 처리
    private void DeserializeSwitchSceneSimpleMSG(object value)
    {
        SwitchSceneSimpleForm data = JsonConvert.DeserializeObject<SwitchSceneSimpleForm>(value.ToString());
        PlayerData.myPlayerinfo.universityCode = data.scene;
        PlayerData.myPlayerinfo.isAppRelease = data.isAppRelease;
        ServerManager.Instance.isAppRelease = data.isAppRelease;
        GameEvents.Instance.RequestSceneChange();
    }
    
    //채팅라운지 생성 정보 처리
    private void DeserializeChatCreateMSG(object value)
    {
        ChatLoungeCreateData data = JsonConvert.DeserializeObject<ChatLoungeCreateData>(value.ToString());
        
        ServerManager.Instance.CreateNewChatLounge(data.isPrivate,data.roomName,data.password,
            ServerManager.Instance.ClickedChattingLoungeOBJ.name,ServerManager.Instance.ClickedChattingLoungeChairCount);
    }
    
    //캐릭터 전체 정보 신호 처리
    private void DeserializeAvatarInfoMSG(object value)
    {
        if (PlayerData.myPlayerinfo.state == null)
        {
            PlayerData.myPlayerinfo.state = new();
        }
        AvatarInfo data = JsonConvert.DeserializeObject<AvatarInfo>(value.ToString());
        PlayerData.myPlayerinfo.memberId = data.memberId;
        //PlayerData.myPlayerinfo.userName = data.userName;
        PlayerData.myPlayerinfo.state = data.avatarState.Clone();
        GameEvents.Instance.RequestSetAvatarInfo();
    }
    
    private void DeserializeChatInviteMSG(object value)
    {
        ChatLoungeInviteData data = JsonConvert.DeserializeObject<ChatLoungeInviteData>(value.ToString());
        InviteFriendInfo info = new InviteFriendInfo();
        info.friendIds = new List<int>();
        info.friendIds.Add(data.memberId);
        ServerManager.Instance.ChatLoungeRoom.Send("invite", new Dictionary<string, object>()
        {
            {
                "data", JsonConvert.SerializeObject(info)
            }
        });
    }
    
    
    private void DeserializeJoinChatMSG(object value)
    {
        ChatLoungeJoinData data = JsonConvert.DeserializeObject<ChatLoungeJoinData>(value.ToString());
        GameEvents.Instance.RequestFindAndJoinChat(data.tableId);
    }


    private void DeserializeLoungeVideoUrl(object value)
    {
        LoungeVideoUrl data = JsonConvert.DeserializeObject<LoungeVideoUrl>(value.ToString());
        PlayerData.myPlayerinfo.seminarUrl = data.url;
    }

    private void SendPenaltyMsgToServer(bool warning)
    {
        if (TargetPlayer != null)
        {
            sendPenalty Data = new sendPenalty();
            Data.memberId =TargetPlayer.GetComponentInChildren<NetworkedEntity>().MemberID;
            Data.sec = 10;
            Data.penaltyType = warning ? "w" : "s";
            string targetUserName = TargetPlayer.GetComponentInChildren<NetworkedEntity>().UserName;
            string paneltyTypeEntry = warning ? "NativeManager_ChatPaneltyWarning" : "NativeManager_ChatPaneltySilence";
            //LocalizationController.Instance.WaitLocaleText((localeText) => { paneltyType = localeText; }, warning ? "NativeManager_ChatPaneltyWarning" : "NativeManager_ChatPaneltySilence");
            Debug.Log("Send "+(warning?"Warning":"Silence")+" Penalty to " + targetUserName);

            // {0}님께서 {1}님께 경고를/침묵을 요청했습니다.
            ServerManager.Instance.Room.Send("sendChat", string.Format("#SYSMSG#{0}#RoomID#{1}#Entry#{2}#Columns#{3}",
                ServerManager.Instance.ChatLoungeRoom.Id, paneltyTypeEntry, PlayerData.myPlayerinfo.userName, targetUserName));

            ServerManager.Instance.ChatLoungeRoom.Send("penalty", new Dictionary<string, object>()
            {
                {
                    "data", JsonConvert.SerializeObject(Data)
                }
            });


        }
    }
    
    
    private void DeserializeDoActionMSG(object value)
    {
        AtionType type = JsonConvert.DeserializeObject<AtionType>(value.ToString());
        switch (type.actionId)
        {
            case "KickAction":
                if (NetworkedEntityFactory.Instance.GetMine().RoomMaker && TargetPlayer != null)
                {
                    sendKick Data = new sendKick();
                    Data.memberId =TargetPlayer.GetComponentInChildren<NetworkedEntity>().MemberID;
                    Debug.Log("Send Kick to " + TargetPlayer.GetComponentInChildren<NetworkedEntity>().UserName);
                    ServerManager.Instance.ChatLoungeRoom.Send("sendKick", new Dictionary<string, object>()
                    {
                        {
                            "data", JsonConvert.SerializeObject(Data)
                        }
                    });
                }
                break;
            case "MandateAction":
                if (NetworkedEntityFactory.Instance.GetMine().RoomMaker && TargetPlayer != null)
                {
                    sendPerm Data = new sendPerm();
                    Data.memberId =TargetPlayer.GetComponentInChildren<NetworkedEntity>().MemberID;
                    string targetUserName = TargetPlayer.GetComponentInChildren<NetworkedEntity>().UserName;
                    Debug.Log("Send Mandate to " + targetUserName);
                    ServerManager.Instance.ChatLoungeRoom.Send("sendPerm", new Dictionary<string, object>()
                    {
                        {
                            "data", JsonConvert.SerializeObject(Data)
                        }
                    });
                }
                break;
            case "SilenceAction":
                SendPenaltyMsgToServer(false);
                break;
            case "WarningAction":
                SendPenaltyMsgToServer(true);
                break;
            case "ChatLeaveAction":
                GameEvents.Instance.LeaveChatRoom();
                break;
            case "LeaveRoomAction":
                GameEvents.Instance.LeaveRoom();
                break;
            case "JoinRoomAction":
                ServerManager.Instance.JoinChattRoomByIdWithPassword("");
                break;
            case "InviteAction" :
                GameEvents.Instance.RequestFindAndJoinChatByRoomId(gotoRoomId);
                break;
            case "BackToMain" :
                if (ServerManager.Instance.Room != null)
                    ServerManager.Instance.LeaveRooms();
                PlayerData.myPlayerinfo.backSign = true;
                PlayerData.myPlayerinfo.universityCode = "AvatarView";
                GameEvents.Instance.RequestSceneChange();
                break;
            default:
                Debug.Log("ActionId [" + type.actionId + "] from Native can't identify");
                break;
        }
    }
    
    private void DeserializeMemberInfo(object value)
    {
        flagGetMemberInfo = false;
        MemberInfoFromNative data = JsonConvert.DeserializeObject<MemberInfoFromNative>(value.ToString());
        PlayerData.myPlayerinfo.userName = data.profileName;
        PlayerData.myPlayerinfo.imgUrl = data.imgUrl;
    }
    
    private void DeserializeJoinByPushInfo(object value)
    {
        ChatLoungeJoinData data = JsonConvert.DeserializeObject<ChatLoungeJoinData>(value.ToString());
        PlayerData.myPlayerinfo.goToTableId = data.tableId;
        
        if (ServerManager.Instance.Room == null)
        {
            PlayerData.myPlayerinfo.universityCode = "lobby";
            GameEvents.Instance.RequestSceneChange();
        }
        else
        {
            if (ServerManager.Instance.Room.Id.Equals("lobby"))
            {
                if (ServerManager.Instance.ChatLoungeRoom == null)
                    GameEvents.Instance.RequestFindAndJoinChat(data.tableId);
                else
                {
                    GameEvents.Instance.LeaveChatRoom();
                    StartCoroutine(BasicSceneManager.Instance.WaitTillLeaveAndJoinInvitedChatRoom(data.tableId));
                }
            }
            else
            {
                GameEvents.Instance.LeaveRoom();
                StartCoroutine(BasicSceneManager.Instance.WaitTillLeaveAndJoinInvitedChatRoom(data.tableId));
                //현제 방 퇴장 이후 gotoTableId를 가진채로 로비로 이동하는 로직
            }
        }
    }
    
    #endregion

    //=============================================Region Divider======================================
    
    #region Unity To Native
    
    private void SendToNative(string msg)
    {
        if((DateTime.Now - prevSendTime).Milliseconds < 500 && msg.Equals(prevSendMsg))
            return;
        //유니티에서 네이티브로 가는 메세지 통일
        Debug.Log(Application.platform +" Unity To Native Message : " + msg);
        
#if UNITY_IOS && !UNITY_EDITOR
        sendMessageToMobileApp(msg);
#elif UNITY_ANDROID && !UNITY_EDITOR
        var androidJC = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var jo = androidJC.GetStatic<AndroidJavaObject>("currentActivity");
        var jc = new AndroidJavaClass("com.tnmeta.torymeta.MainBaseActivity");
        jc.CallStatic("sendToNative", jo, msg);
#endif
        prevSendTime = DateTime.Now;
        prevSendMsg = msg;
    }
    
    public void SendChatLoungeParticipantsMenuMessage(GameObject hit)
    {
        GameObject player = hit;
        bool top = false;
        while (!top)
        {
            if (!player.name.Contains("Clone"))
                player = player.transform.parent.gameObject;
            else
                top = true;
        }

        if (!player.GetComponentInChildren<NetworkedEntity>().isMine)
            TargetPlayer = player;
        else
            TargetPlayer = null;
        
        //방장인지 확인하는 로직 필요
        if (TargetPlayer != null)
        {
            ParticipantsMenuOptionForm optionForm = new ParticipantsMenuOptionForm();
            optionForm.memberId = TargetPlayer.GetComponentInChildren<NetworkedEntity>().MemberID.ToString();
            optionForm.roomMaker = NetworkedEntityFactory.Instance.GetMine().RoomMaker;
            
            FixedFormCall value = new FixedFormCall();
            value.form = "ParticipantsMenu";
            value.option = optionForm;

            BasicMessage data = new BasicMessage();
            data.type = "FormCall";
            data.value = value;
            SendToNative(JsonConvert.SerializeObject(data));
        }
    }
    
    public void SendInvitableListMessage(List<FriendInfo> list)
    {
        InvitableListOptionForm optionForm = new InvitableListOptionForm();
        optionForm.list = new List<FriendInfo>();
        foreach (var item in list)
        {
            FriendInfo temp = new FriendInfo();
            temp.memberId = item.memberId;
            temp.roomMaker = item.roomMaker;
            temp.imgUrl = item.imgUrl;
            temp.phoneNum = item.phoneNum;
            temp.profileName = item.profileName;
            optionForm.list.Add(temp);
        }
        
        FixedFormCall value = new FixedFormCall();
        value.form = "InvitableList";
        value.option = optionForm;
        
        BasicMessage data = new BasicMessage();
        data.type = "FormCall";
        data.value = value;
        SendToNative(JsonConvert.SerializeObject(data));
    }
    
    public void SendChatLoungeParticipantsListMessage(List<FriendInfo> list, int chatRoomId)
    {
        ParticipantsListOptionForm optionForm = new ParticipantsListOptionForm();
        optionForm.roomId = ServerManager.Instance.ChatLoungeRoom.Id;
        optionForm.maxClient = ServerManager.Instance.ClickedChattingLoungeChairCount;
        optionForm.tableId = ServerManager.Instance.ClickedChattingLoungeOBJ.name;
        optionForm.chatRoomId = chatRoomId;

        optionForm.list = new List<FriendInfo>();
        foreach (var item in list)
        {
            FriendInfo temp = new FriendInfo();
            temp.memberId = item.memberId;
            temp.roomMaker = item.roomMaker;
            temp.imgUrl = item.imgUrl;
            temp.phoneNum = item.phoneNum;
            temp.profileName = item.profileName;
            optionForm.list.Add(temp);
        }
        
        FixedFormCall value = new FixedFormCall();
        value.form = "ParticipantsList";
        value.option = optionForm;
        
        BasicMessage data = new BasicMessage();
        data.type = "FormCall";
        data.value = value;
        SendToNative(JsonConvert.SerializeObject(data));
    }
    
    public void SendChatLoungeListMessage()
    {
        FixedFormCall value = new FixedFormCall();
        value.form = "ChatList";
        value.option = "";
        
        BasicMessage data = new BasicMessage();
        data.type = "FormCall";
        data.value = value;
        SendToNative(JsonConvert.SerializeObject(data));
    }
    
    public void SendChatLoungeCreateMessage()
    {
        FixedFormCall value = new FixedFormCall();
        value.form = "ChatCreate";
        value.option = "";
        
        BasicMessage data = new BasicMessage();
        data.type = "FormCall";
        data.value = value;
        SendToNative(JsonConvert.SerializeObject(data));
    }
    
    public void SendChatLoungePasswordInput(string password)
    {
        PwInputOptionForm optionForm = new PwInputOptionForm();
        optionForm.password = password;
        
        FixedFormCall value = new FixedFormCall();
        value.form = "PasswordInput";
        value.option = optionForm;
        
        BasicMessage data = new BasicMessage();
        data.type = "FormCall";
        data.value = value;
        SendToNative(JsonConvert.SerializeObject(data));
    }

    public void SendNativeBack()
    {
        BasicMessage data = new BasicMessage();
        data.type = "NativeBack";
        data.value = "";
        SendToNative(JsonConvert.SerializeObject(data));
    }
    
    public void SendUnityLifecycleState(LifeCycle flag)
    {
        BasicMessage data = new BasicMessage();
        switch (flag)
        {
            case LifeCycle.Awake:
                data.type = "Awake";
                break;
            case LifeCycle.Start:
                data.type = "Start";
                break;
            case LifeCycle.OnDestroy:
                data.type = "OnDestroy";
                break;
            default:
                Debug.Log("Unidentified Cycle");
                return;
        }
        SendToNative(JsonConvert.SerializeObject(data));
    }

    public void SendRequireLoungeVideoUrl()
    {
        BasicMessage data = new BasicMessage();
        data.type = "RequireLoungeVideoUrl";
        SendToNative(JsonConvert.SerializeObject(data));
    }

    public void SendConfirm(string actionId, string description)
    {
        ButtonForm btnConfirm = new ButtonForm();
        btnConfirm.title = btnConfirmLocaleText;
        btnConfirm.color = "#6C5CE7";
        btnConfirm.actionId = actionId;

        ConfirmForm value = new ConfirmForm();
        value.buttons = new List<ButtonForm>();
        value.buttons.Add(btnCancel);
        value.buttons.Add(btnConfirm);
        value.description = description;


        BasicMessage data = new BasicMessage();
        data.type = "NativeShowModal";
        data.value = value;
        SendToNative(JsonConvert.SerializeObject(data));
    }

    public void SendKickConfirm()
    {
        ButtonForm btnConfirm = new ButtonForm();
        btnConfirm.title = btnConfirmLocaleText;
        btnConfirm.color = "#6C5CE7";
        btnConfirm.actionId = "KickAction";
        
        ConfirmForm value = new ConfirmForm();
        value.buttons = new List<ButtonForm>();
        value.buttons.Add(btnCancel);
        value.buttons.Add(btnConfirm);
        value.description = "선택한 사용자를 [강제퇴장] 시키겠습니까?";
        
        
        BasicMessage data = new BasicMessage();
        data.type = "NativeShowModal";
        data.value = value;
        SendToNative(JsonConvert.SerializeObject(data));
    }
    
    public void SendSilenceConfirm()
    {
        ButtonForm btnConfirm = new ButtonForm();
        btnConfirm.title = btnConfirmLocaleText;
        btnConfirm.color = "#6C5CE7";
        btnConfirm.actionId = "SilenceAction";
        
        ConfirmForm value = new ConfirmForm();
        value.buttons = new List<ButtonForm>();
        value.buttons.Add(btnCancel);
        value.buttons.Add(btnConfirm);
        value.description = "선택하신 사용자의 채팅권한을 [10초]간 제한하시겠습니까?";
        
        
        BasicMessage data = new BasicMessage();
        data.type = "NativeShowModal";
        data.value = value;
        SendToNative(JsonConvert.SerializeObject(data));
    }
    
    public void SendWarningConfirm()
    {
        ButtonForm btnConfirm = new ButtonForm();
        btnConfirm.title = btnConfirmLocaleText;
        btnConfirm.color = "#6C5CE7";
        btnConfirm.actionId = "WarningAction";
        
        ConfirmForm value = new ConfirmForm();
        value.buttons = new List<ButtonForm>();
        value.buttons.Add(btnCancel);
        value.buttons.Add(btnConfirm);
        value.description = "선택하신 사용자에게 [경고장]을 보내시겠습니까? 3회 누적 시 강제퇴장 처리됩니다.";
        
        
        BasicMessage data = new BasicMessage();
        data.type = "NativeShowModal";
        data.value = value;
        SendToNative(JsonConvert.SerializeObject(data));
    }
    
    public void SendMandateConfirm()
    {
        ButtonForm btnConfirm = new ButtonForm();
        btnConfirm.title = btnConfirmLocaleText;
        btnConfirm.color = "#6C5CE7";
        btnConfirm.actionId = "MandateAction";
        
        ConfirmForm value = new ConfirmForm();
        value.buttons = new List<ButtonForm>();
        value.buttons.Add(btnCancel);
        value.buttons.Add(btnConfirm);
        value.description = "선택하신 사용자에게 방장 권한을 이관하시겠습니까? [확인] 터치시 모든 권한이 이관됩니다.";
        
        
        BasicMessage data = new BasicMessage();
        data.type = "NativeShowModal";
        data.value = value;
        SendToNative(JsonConvert.SerializeObject(data));
    }
    
    public void SendInviteConfirm(string nick, string roomId)
    {
        gotoRoomId = roomId;
        
        ButtonForm btnConfirm = new ButtonForm();
        btnConfirm.title = btnConfirmLocaleText;
        btnConfirm.color = "#6C5CE7";
        btnConfirm.actionId = "InviteAction";
        
        ConfirmForm value = new ConfirmForm();
        value.buttons = new List<ButtonForm>();
        value.buttons.Add(btnCancel);
        value.buttons.Add(btnConfirm);
        //value.description = "["+ nick +"]님이 채팅방으로 초대하셨습니다.";
        LocalizationController.Instance.WaitLocaleText((localeText) => { value.description = "[" + nick + "]" + localeText; }, "NativeManager_ModalInvite");

        BasicMessage data = new BasicMessage();
        data.type = "NativeShowModal";
        data.value = value;
        SendToNative(JsonConvert.SerializeObject(data));
    }

    public void SendChatLeaveConfirm()
    {
        ButtonForm btnConfirm = new ButtonForm();
        btnConfirm.title = btnConfirmLocaleText;
        btnConfirm.color = "#6C5CE7";
        btnConfirm.actionId = "ChatLeaveAction";
        
        ConfirmForm value = new ConfirmForm();
        value.buttons = new List<ButtonForm>();
        value.buttons.Add(btnCancel);
        value.buttons.Add(btnConfirm);
        //value.description = "대화를 종료하고 채팅방을 나가시겠습니까?";
        LocalizationController.Instance.WaitLocaleText((localeText) => { value.description = localeText; }, "NativeManager_ModalLeave");

        BasicMessage data = new BasicMessage();
        data.type = "NativeShowModal";
        data.value = value;
        SendToNative(JsonConvert.SerializeObject(data));
    }
    
    public void SendLeaveRoomConfirm(string description)
    {
        Debug.Log("SendLeaveRoomConfirm " + description);
        ButtonForm btnConfirm = new ButtonForm();
        btnConfirm.title = btnConfirmLocaleText;
        btnConfirm.color = "#6C5CE7";
        btnConfirm.actionId = "LeaveRoomAction";
        
        ConfirmForm value = new ConfirmForm();
        value.buttons = new List<ButtonForm>();
        value.buttons.Add(btnCancel);
        value.buttons.Add(btnConfirm);
        value.description = description;// "토리메타 메인화면으로 이동하시겠습니까?";
        
        BasicMessage data = new BasicMessage();
        data.type = "NativeShowModal";
        data.value = value;
        SendToNative(JsonConvert.SerializeObject(data));
    }

    public void SendBasicAlert(string description,bool leave = false)
    {
        ButtonForm btnConfirm = new ButtonForm();
        btnConfirm.title = btnConfirmLocaleText;
        btnConfirm.color = "#6C5CE7";
        btnConfirm.actionId = leave ? "LeaveRoomAction" : "";
        
        ConfirmForm value = new ConfirmForm();
        value.buttons = new List<ButtonForm>();
        value.buttons.Add(btnConfirm);
        value.description = description;
        
        
        BasicMessage data = new BasicMessage();
        data.type = "NativeShowModal";
        data.value = value;
        SendToNative(JsonConvert.SerializeObject(data));
    }
    
    public void SendJoinPublicRoomConfirm()
    {
        ButtonForm btnConfirm = new ButtonForm();
        btnConfirm.title = btnConfirmLocaleText;
        btnConfirm.color = "#6C5CE7";
        btnConfirm.actionId = "JoinRoomAction";
        
        ConfirmForm value = new ConfirmForm();
        value.buttons = new List<ButtonForm>();
        value.buttons.Add(btnCancel);
        value.buttons.Add(btnConfirm);
        //value.description = "선택하신 채팅방에 참여하시겠습니까?";
        LocalizationController.Instance.WaitLocaleText((localeText) => { value.description = localeText; }, "NativeManager_ModalChatJoin");

        BasicMessage data = new BasicMessage();
        data.type = "NativeShowModal";
        data.value = value;
        SendToNative(JsonConvert.SerializeObject(data));
    }

    public void SendRequestMemberInfo()
    {
        BasicMessage data = new BasicMessage();
        data.type = "RequireMember";
        SendToNative(JsonConvert.SerializeObject(data));
    }
    
    public void SendDisconnectCompleted()
    {
        BasicMessage data = new BasicMessage();
        data.type = "DisconnectCompleted";
        SendToNative(JsonConvert.SerializeObject(data));
    }
    
    public void SendNetworkTimeOutConfirm()
    {
        ButtonForm btnConfirm = new ButtonForm();
        btnConfirm.title = btnConfirmLocaleText;
        btnConfirm.color = "#6C5CE7";
        btnConfirm.actionId = "BackToMain";
        
        ConfirmForm value = new ConfirmForm();
        value.buttons = new List<ButtonForm>();
        value.buttons.Add(btnConfirm);
        //value.description = "네트워크 연결이 원활하지 않습니다.\n네트워크 연결 상태를 확인하신 후\n다시 입장해 주세요.";
        LocalizationController.Instance.WaitLocaleText((localeText) => { value.description = localeText; }, "NativeManager_ModalNetworkTimeOut");

        BasicMessage data = new BasicMessage();
        data.type = "NativeShowModal";
        data.value = value;
        SendToNative(JsonConvert.SerializeObject(data));
    }
    #endregion
    

}
