using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agora.Rtc;
using UnityEngine;
using Colyseus;
using Colyseus.Schema;
using LucidSightTools;
using NativeWebSocket;
using Newtonsoft.Json;
using UniRx;
using UniRx.Triggers;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ServerManager : ColyseusManager<ServerManager>
{
    
    public delegate void OnRoomChanged(ColyseusRoom<RoomState> room);
    public static event OnRoomChanged onRoomChanged;

    public bool isConnected = false;
    public bool endflag = false;
    
    public ColyseusRoom<RoomState> Room { get; set; }
    
    public ColyseusRoom<RoomState> ChatLoungeRoom { get; set; }

    private bool isQuitting = false;
    //The chat room
    private ColyseusRoom<ChatRoomState> _chatRoom;
    
    public bool ChatLoungeProcessDone;
    public bool responedFlag;
    public bool responed;

    public GameObject ClickedChattingLoungeOBJ;
    public int ClickedChattingLoungeChairCount;
    public RoomInfo ClickedChattingLoungeRoomInfo;

    private Coroutine taskConnectionCheck;

    private string currRoomId;
    private string currRoomSessionId;
    private string currChatRoomId;
    private string currChatRoomSessionId;
    
    protected override void Awake()
    {
        taskConnectionCheck = null;
        isConnected = false;
        DontDestroyOnLoad(gameObject);
        isAppRelease = PlayerData.myPlayerinfo.isAppRelease;
        base.Awake();
    }

    protected override void Start()
    {
        endflag = false;
        Debug.Log("Server Manager Start " + SceneManager.GetActiveScene().name);
        
        Application.targetFrameRate = 30;
        InitializeClient();
    }

#region Room Related

    public void TryConnect(string roomId, Action<string> onError)
    {
        Debug.Log("CheckPoint : TryConnect");
        try
        {
            UserLogIn<UserAuthResponse>(roomId, (response) => { JoinRoom(onError, response); });
        }
        catch (Exception e)
        {
            Debug.Log("TryConnect() - exception : "+ e.StackTrace);
        }
    }
    
    public void UserLogIn<T>(string roomId, Action<RequestResponse> onComplete) where T : RequestResponse
    {
        WWWForm form = new WWWForm(); 
        Debug.Log("roomId = " + roomId);

        // 비방 생성 데이터 추가 23.06.11
        if (PlayerData.myPlayerinfo.universityCode.Equals("seminar")) 
        {
            form.AddField("roomId", PlayerData.myPlayerinfo.seminarId);
            form.AddField("chatGroup", roomId);
            switch (PlayerData.myPlayerinfo.seminarType)
            {
                case "20":
                    //key = "SeminarL5"; 
                    form.AddField("maxClients", 51);
                    break;
                case "21":
                    //key = "SeminarM5"; 
                    form.AddField("maxClients", 33);
                    break;
                case "22":
                    //key = "SeminarS5";
                    form.AddField("maxClients", 19); 
                    break;
            }
        }
        else
        {
            form.AddField("roomId", roomId);
            form.AddField("maxClients", 1024);
        }

        form.AddField("memberId", PlayerData.myPlayerinfo.memberId);

        StartCoroutine(Co_ServerRequest<T>("POST", "room/create", form, onComplete));
    }
    
    private async void JoinRoom(Action<string> onError, RequestResponse response)
    {
        if (response.error)
        {
            onError?.Invoke(response.output);
        }
        else
        {
            try
            { 
                UserAuthResponse userAuthResponse = (UserAuthResponse)response;

                Response root = JsonConvert.DeserializeObject<Response>(response.rawResponse);
                
                DownloadManager.Instance.Download();
                ColyseusRoom<RoomState> room = await JoinById(root.roomId);

                Room = room;
                RegisterHandlers();
                
                if (userAuthResponse.output.seatReservation.room == null)
                {
                    userAuthResponse.output.seatReservation = new SeatReservationData();
                    userAuthResponse.output.seatReservation.room = new ColyseusRoomAvailable()
                    {
                        roomId = room.Id,
                        name = "toryworld-pw"
                    };
                }
                else
                {
                    userAuthResponse.output.seatReservation.room = root.output.seatReservation;
                    userAuthResponse.output.seatReservation.room.roomId = root.roomId;
                    userAuthResponse.output.seatReservation.sessionId = room.SessionId;
                }

                Debug.Log($"Session Id = {room.SessionId}");

                PlayerData.myPlayerinfo.entityId = room.SessionId;
                
                userAuthResponse.output.user = new UserData()
                {
                    id = room.SessionId,
                    username = PlayerData.myPlayerinfo.userName
                };
            }
            catch (Exception e)
            {
                Debug.Log("JoinById() - exception : " + e.StackTrace);
            }
        }
    }
    
    public async Task<ColyseusRoom<RoomState>> JoinById(string roomId)
    {
        Debug.Log("JoinById " + PlayerData.myPlayerinfo.universityCode);

        string userName = string.IsNullOrEmpty(PlayerData.myPlayerinfo.userName)
            ? "USER " + UnityEngine.Random.Range(1, 100)
            : PlayerData.myPlayerinfo.userName;


        NetworkedEntityState state = new NetworkedEntityState()
        {
            entityId = PlayerData.myPlayerinfo.entityId,
            chatId = "ID",
            xPos = 1,
            yPos = 1,
            zPos = 1,
            xRot = 0,
            yRot = 0,
            zRot = 0,
            wRot = 0,
            timestamp = 1,
            coins = 1,
            avatar = PlayerData.myPlayerinfo.state.Clone(),
            username = PlayerData.myPlayerinfo.userName,
            seat = "0", 
            chatRoomHistoryId = 1, 
            memberId= PlayerData.myPlayerinfo.memberId, 
            roomMaker = false, 
            inputting = 0, 
            table ="0",
            clients = 1,
            maxClients = 1,
            password = ""
        };


        ColyseusRoom<RoomState> room = await client.JoinById<RoomState>(roomId, new Dictionary<string, object>()
        {
            { "data", JsonConvert.SerializeObject(state) }
        });

        currRoomId = roomId;
        currRoomSessionId = room.SessionId;
        return room;

    }

    private IEnumerator WaitThenSpawnPlayer(NetworkedEntityState state)
    {
        //while (!GameEvents.Instance.currentUnivCode.Equals(GameEvents.Instance.destUnivCode))
        //{
        //    yield return new WaitForEndOfFrame();
        //}
        
        while (!Room.State.networkedUsers.ContainsKey(state.entityId) || !DownloadManager.Instance.isDownComplete)
        {
            //if(!SceneManager.GetActiveScene().name.Equals("TempScene"))
            //    yield break;
            Debug.Log("########### WaitThenSpawnPlayer  while loof id : " + state.entityId + SceneManager.GetActiveScene().name);
        //     Debug.Log("Current Scene is " + SceneManager.GetActiveScene().name);
            //Wait until the room has a state for this ID (may take a frame or two, prevent race conditions)
            yield return new WaitForEndOfFrame();
        }

        bool isOurs = state.entityId.Equals(Room.SessionId);
        // NetworkedEntityState entityState = Room.State.networkedUsers[state.entityId];

        NetworkedEntityFactory.Instance.SpawnEntity(state, isOurs);
        // DownloadManager.Instance.Download();
        
    }

    private IEnumerator AwaitObjectInteraction(string objectID, string entityID)
    {
        while (!Room.State.interactableItems.ContainsKey(objectID))
        {
            //Wait for the room to be aware of the object
            yield return new WaitForEndOfFrame();
        }
    }

    /// <summary>
    /// Registers handlers for room state events as well as room messages
    /// </summary>
    public void RegisterHandlers()
    {
        if (Room != null)
        {
            Debug.Log("Event 등록 ");
            Room.OnLeave += OnLeave;

            Room.OnStateChange += OnRoomStateChange;
            Room.State.networkedUsers.OnAdd += NetworkedUsers_OnAdd;
            Room.State.networkedUsers.OnRemove += NetworkedUsers_OnRemove;

            Room.State.action.OnAdd += ActionState_OnAdd;
            
            Room.OnMessage<ObjectUseMessage>("objectUsed", (msg) =>
            {
                StartCoroutine(AwaitObjectInteraction(msg.interactedObjectID, msg.interactingStateID));
            });
            
            Room.OnMessage<ActionState>("onRFC", (msg) =>
            {
                Debug.Log("Receive Message " + "[" + msg.entityId + "]");

                NetworkedEntity entity = NetworkedEntityFactory.Instance.GetEntity(msg.entityId);
                
                entity.RemoteFunctionCallHandler(new ActionState()
                {
                    entityId = msg.entityId,
                    actionId = msg.actionId
                });
            });
            
            Room.OnMessage<ReceiveInvite>("receiveInvite", (msg) =>
            {
                Debug.Log("Receive Message " + msg.inviterNick + "이 방 ["+ msg.title+"]에 초대되었습니다.");
                NativeManager.Instance.SendInviteConfirm(msg.inviterNick, msg.roomId);
            });
        }
        else
        {
            LSLog.LogError($"Cannot register room handlers, room is null!");
        }
    }

    private void UnregisterHandlers()
    {
        if (Room != null)
        {
            Room.OnLeave -= OnLeave;

            Room.OnStateChange -= OnRoomStateChange;

            Room.State.networkedUsers.OnAdd -= NetworkedUsers_OnAdd;
            Room.State.networkedUsers.OnRemove -= NetworkedUsers_OnRemove;
            
            Room.State.action.OnAdd -= ActionState_OnAdd;
            
            // Room.State.action.OnAdd -= ActionState_OnAdd;
        }
    }
    
    
    public async void LeaveRooms()
    {
        base.OnDestroy();

        UnregisterHandlers();

        CleanUpRoom();
        // Leave current room
        //Room.Leave(true);
        await Room.colyseusConnection.Close();

        endflag = true;
        
        NetworkedEntityFactory.Instance.ClearEntities();
        Room = null;
        Debug.Log("LeaveRooms");
    }

    /// <summary>
    /// Removes all entities from the scene except the entity for this client
    /// </summary>
    private void CleanUpRoom()
    {
        if(NetworkedEntityFactory.Instance != null)
            NetworkedEntityFactory.Instance.RemoveAllEntities(true);
    }

    private void OnLeave(NativeWebSocket.WebSocketCloseCode code)
    {
        ReconnectRoom();
        Debug.Log("Trying to Reconnect Room");
    }
    async void ReconnectRoom()
    {
        UnregisterHandlers();
        
        Room = await client.Reconnect<RoomState>(currRoomId, currRoomSessionId);

        if (Room != null)
        {
            Debug.Log("Reconnected into room successfully.");
            RegisterHandlers();
            NetworkedEntity mine = NetworkedEntityFactory.Instance.GetMine();
            StartCoroutine(mine.Initialize(mine.GetState(), true));
        }
    }
    /// <summary>
    /// Callback for when a networked entity has been removed from the room state's collection of networked entities/users
    /// </summary>
    /// <param name="key">The sessionId of the networked entity that got removed</param>
    /// <param name="value">The <see cref="NetworkedEntityState"/> of the user that was removed</param>
    private void NetworkedUsers_OnRemove(string key, NetworkedEntityState value)
    {
        NetworkedEntityFactory.Instance.RemoveEntity(value.entityId);
    }

    /// <summary>
    /// Callback for when a networked entity has been added to the room state's collection of networked entities/users
    /// </summary>
    /// <param name="key">The sessionId of the networked entity that got added</param>
    /// <param name="value">The <see cref="NetworkedEntityState"/> of the user that was added</param>
    private void NetworkedUsers_OnAdd(string key, NetworkedEntityState value)
    {
        StartCoroutine(WaitThenSpawnPlayer(value));
    }

    private void ActionState_OnAdd(string key, ActionState value)
    {
        // Debug.Log("ActionState_OnAdd " + key +" , " + value.actionId);
        NetworkedEntityFactory.Instance.MakeEntityDoAction(key, value);
    }

    /// <summary>
    /// Event handler when the room receives its first state
    /// </summary>
    /// <param name="state"></param>
    /// <param name="isfirststate"></param>
    private void OnRoomStateChange(RoomState state, bool isfirststate)
    {
        state.chatQueue.ForEach((clientID, queue) => { NetworkedEntityFactory.Instance.HandMessages(clientID, queue); });
        state.networkedUsers.ForEach((clientID, queue) => { NetworkedEntityFactory.Instance.UpdateEntityInfo(clientID, queue); });
    }
    
#endregion
    
//=============================================Region Divider======================================

#region ChatLoungeRoom Related

    private void OnLeaveChatRoom(NativeWebSocket.WebSocketCloseCode code)
    {
        ReconnectChatRoom();
        Debug.Log("Trying to Reconnect ChatRoom");
    }

    async void ReconnectChatRoom()
    {
        UnregisterHandlersForChatRoom();

        ChatLoungeRoom = await client.Reconnect<RoomState>(currChatRoomId, currChatRoomSessionId);

        if (ChatLoungeRoom != null)
        {
            Debug.Log("Reconnected into room successfully.");
            RegisterHandlersForChatRoom();
        }
    }

    public void OnChatRoomStateChange(RoomState state, bool isfirststate)
    {
        state.networkedUsers.ForEach((clientID, queue) => { NetworkedEntityFactory.Instance.UpdateEntityRoomMakerInfo(clientID, queue); });
    }

    public void CreateNewChatLounge(bool isprivate, string roomname, string password, string tableid, int max)
    {
        GetChattingLoungeList<ChattingLoungeListResponse>(
            delegate(RequestResponse response)
            {
                ChattingLoungeListResponse chattingLoungeListResponse = (ChattingLoungeListResponse)response;
                if (chattingLoungeListResponse.result.rooms != null)
                {
                    bool match = false;
                    foreach (var info in chattingLoungeListResponse.result.rooms)
                    {
                        if (info.tableId.Equals(tableid))
                        {
                            match = true;
                            break;
                        }
                    }

                    if (!match)
                    {
                        CreateChattingLounge<RequestResponse>(isprivate, roomname, password, tableid, max,
                            (response) =>
                            {
                                JoinChattRoomById((error) => { Debug.LogError($"Login 에러 l {error} l"); }, response);
                            });
                    }
                }
                else
                {
                    CreateChattingLounge<RequestResponse>(isprivate, roomname, password, tableid, max,
                        (response) =>
                        {
                            JoinChattRoomById((error) => { Debug.LogError($"Login 에러 l {error} l"); }, response);
                        });
                }
            });
    }

    public void LeaveChatRoom()
    {
        if (ChatLoungeRoom != null)
        {
            ClickedChattingLoungeOBJ.GetComponentInChildren<ChatLoungeController>().SetUIObjectOff(true);
            UnregisterHandlersForChatRoom();
            //ChatLoungeRoom.Leave();
            StopCoroutine(taskConnectionCheck);
            taskConnectionCheck = null;
            ChatLoungeRoom.colyseusConnection.Close();
            ChatLoungeRoom = null;
            ClickedChattingLoungeChairCount = 0;
            ClickedChattingLoungeOBJ = null;
        }
    }
    
    

    public void RegisterHandlersForChatRoom()
    {
        if (ChatLoungeRoom != null)
        {
            ChatLoungeRoom.OnLeave += OnLeaveChatRoom;
            ChatLoungeRoom.OnStateChange += OnChatRoomStateChange;
            
            ChatLoungeRoom.OnMessage<penalty>("penalty", (msg) =>
            {
                switch (msg.penaltyType)
                {
                    case "w":
                        Debug.Log("Receive Message " + "[warning penalty]");
                        GameEvents.Instance.RequestWarningPenalty();
                        break;
                    case "s":
                        Debug.Log("Receive Message " + "[Silence penalty]");
                        GameEvents.Instance.RequestSilencePenalty();
                        break;
                    default:
                        Debug.Log("unidentified panalty code : " + msg.penaltyType);
                        break;
                }
            });

            ChatLoungeRoom.OnMessage<kick>("kick", (msg) =>
            {
                Debug.Log("Receive Message " + "[Kick from RoomMaker]");
                GameEvents.Instance.LeaveChatRoom();
                //{0}님이 방장에 의해 강퇴되었습니다.
                ServerManager.Instance.Room.Send("sendChat", string.Format("#SYSMSG#{0}#RoomID#{1}#Entry#{2}",
                        ServerManager.Instance.ChatLoungeRoom.Id, "ServerManager_ChatChatRoomKick", PlayerData.myPlayerinfo.userName));
                
                LocalizationController.Instance.WaitLocaleText((localeText) => {
                    NativeManager.Instance.SendBasicAlert(localeText);
                }, "ServerManager_AlertChatRoomKick");
            });

            ChatLoungeRoom.OnMessage<FriendList>("friendList", (msg) =>
            {
                if (msg.status.Equals("OFFLINE"))
                {
                    Debug.Log("Receive Invitable List - fail");
                    LocalizationController.Instance.WaitLocaleText((localeText) => {
                        NativeManager.Instance.SendBasicAlert(localeText);
                    }, "ServerManager_AlertChatRoomFriendListNull");
                    //NativeManager.Instance.SendBasicAlert("초대 가능한 사용자가 없습니다.");
                }
                else
                {
                    Debug.Log("Receive Invitable List - Success");
                    NativeManager.Instance.SendInvitableListMessage(msg.friendList);
                }
            });

            ChatLoungeRoom.OnMessage<FriendList>("participantList", (msg) =>
            {
                if (msg.status.Equals("OFFLINE"))
                    Debug.Log("Receive Participant List - fail");
                else
                {
                    Debug.Log("Receive Participant List - Success");
                    NativeManager.Instance.SendChatLoungeParticipantsListMessage(msg.friendList, msg.chatRoomId);
                }
            });

            ChatLoungeRoom.OnMessage<InviteResult>("inviteResult",
                (msg) => { Debug.Log("inviteResult message receive who i invite is " + msg.status); });
        }
    }
    
    private void UnregisterHandlersForChatRoom()
    {
        if (ChatLoungeRoom != null)
        {
            ChatLoungeRoom.OnLeave -= OnLeaveChatRoom;
            ChatLoungeRoom.OnStateChange -= OnChatRoomStateChange;
        }
    }

    private async void JoinChattRoomById(Action<string> onError, RequestResponse response)
    {
        if (response.error)
        {
            onError?.Invoke(response.output);
            ChatLoungeProcessDone = true;
        }
        else
        {
            Response root = JsonConvert.DeserializeObject<Response>(response.rawResponse);
            ColyseusRoom<RoomState> room = await ChatLoungeJoinById(root.roomId,true);
            
            ChatLoungeRoom = room;
            if (ChatLoungeRoom != null)
            {
                
                NetworkedEntityFactory.Instance.MakePlayerChatJoin(ClickedChattingLoungeOBJ);
                RegisterHandlersForChatRoom();

                LocalizationController.Instance.WaitLocaleText((localeText) => {
                    NativeManager.Instance.SendBasicAlert(localeText);
                }, "ServerManager_AlertChatRoomCreateDone");
                //NativeManager.Instance.SendBasicAlert("채팅방이 개설되었습니다.");
                
                taskConnectionCheck = StartCoroutine(CheckingServerConnection());
            }
            else
            {
                LocalizationController.Instance.WaitLocaleText((localeText) => {
                    NativeManager.Instance.SendBasicAlert(localeText);
                }, "ServerManager_AlertChatRoomCreateFail");
                //NativeManager.Instance.SendBasicAlert("채팅방 생성에 실패하였습니다.");
                ChatLoungeProcessDone = true;
            }
        }
    }

    public async void JoinChattRoomByIdWithPassword(string password)
    {
        ColyseusRoom<RoomState> room = await ChatLoungeJoinById(ClickedChattingLoungeRoomInfo.roomId
            , false, password);

        ChatLoungeRoom = room;

        if (ChatLoungeRoom != null)
        {
            NetworkedEntityFactory.Instance.MakePlayerChatJoin(ClickedChattingLoungeOBJ);
            RegisterHandlersForChatRoom();

            taskConnectionCheck = StartCoroutine(CheckingServerConnection());
        }
        else
        {
            LocalizationController.Instance.WaitLocaleText((localeText) => {
                NativeManager.Instance.SendBasicAlert(localeText);
            }, "ServerManager_AlertChatRoomJoinFail");
            //NativeManager.Instance.SendBasicAlert("채팅방 입장에 실패하였습니다.");
            ChatLoungeProcessDone = true;
        }
    }

    
    public async Task<ColyseusRoom<RoomState>> ChatLoungeJoinById(string roomId, bool _roomMaker = false, string _password ="")
    {
        string userName = string.IsNullOrEmpty(PlayerData.myPlayerinfo.userName)
            ? "USER " + UnityEngine.Random.Range(1, 100)
            : PlayerData.myPlayerinfo.userName;
        NetworkedEntityState state = new NetworkedEntityState()
        {
            entityId = PlayerData.myPlayerinfo.entityId, 
            chatId = "ID", 
            xPos = 1, 
            yPos = 1, 
            zPos = 1, 
            xRot = 0, 
            yRot = 0, 
            zRot = 0, 
            wRot = 0, 
            timestamp = 1, 
            coins = 1, 
            avatar = new AvatarState()
            {
                skinCode = "7001",
                skinColorCode = "#000000",
                hairCode = "2001",
                hairColorCode = "#000000",
                faceCode = "3001",
                faceColorCode = "#000000",
                topCode = "4001",
                topColorCode = "#000000",
                bottomCode = "5001",
                bottomColorCode = "#000000",
                shoesCode = "6001",
                bodyCode = PlayerData.myPlayerinfo.avatar//"1001"
            }, 
            username = PlayerData.myPlayerinfo.userName,
            seat = "0", 
            chatRoomHistoryId = 1, 
            memberId= PlayerData.myPlayerinfo.memberId, 
            roomMaker = _roomMaker, 
            inputting = 0, 
            table ="0",
            clients = 1,
            maxClients = 1,
            password = _password
        };
        ColyseusRoom<RoomState> room = await ChatLoungeclient.JoinById<RoomState>(roomId, new Dictionary<string, object>()
        {
            { "data", JsonConvert.SerializeObject(state) }
        });
        currChatRoomId = roomId;
        currChatRoomSessionId = room.SessionId;
        return room;
    }
    
    public void GetChattingLoungeList<T>(Action<RequestResponse> onComplete)
        where T : RequestResponse
    {
        WWWForm form = new WWWForm();
        form.AddField("chatGroup", "chat_lounge"); 
        form.AddField("limit", 1000); 
        StartCoroutine(Co_ServerRequest<T>("POST", "room/list", form, onComplete));
    }
     
    public void CreateChattingLounge<T>(bool isPrivate,string title, string password, string tableId, int max,Action<RequestResponse> onComplete)
        where T : RequestResponse
    {
        WWWForm form = new WWWForm();
        form.AddField("roomId", ""); 
        form.AddField("maxClients", max); 
        form.AddField("title", title); 
        form.AddField("image", (string.IsNullOrEmpty(PlayerData.myPlayerinfo.imgUrl)?"127.0.0.1":PlayerData.myPlayerinfo.imgUrl)); 
        form.AddField("creatorName", PlayerData.myPlayerinfo.userName); 
        form.AddField("chatGroup", "chat_lounge"); 
        form.AddField("tableId", tableId); 
        form.AddField("privateRoom", isPrivate? 1:0); 
        form.AddField("password",password);
        form.AddField("memberId", PlayerData.myPlayerinfo.memberId);
        StartCoroutine(Co_ServerRequest<T>("POST", "room/create", form, onComplete));
    }
    
#endregion

//=============================================Region Divider======================================

#region Agora Related


public void StartVideoChat(int memberId, GameObject canvas, string channel ="")
{
    AgoraManager.Instance.SetVideoCanvas(canvas);
    uint temp = (memberId == PlayerData.myPlayerinfo.memberId) ? 0 : (uint)memberId;
    AgoraManager.Instance.MakeVideoView(temp, channel);
}

public void LeaveVideoChat()
{
    AgoraManager.Instance.LeaveChannel();
}

public void VideoChatVideoMute(bool mute)
{
    if(mute)
        AgoraManager.Instance.SetVideoOff();
    else
        AgoraManager.Instance.SetVideoOn();
}

public void VideoChatAudioMute(bool mute)
{
    if(mute)
        AgoraManager.Instance.SetMikeOff();
    else
        AgoraManager.Instance.SetMikeOn();
}

public void InitializeVideoChat(string roomId = "")
{
    AgoraManager.Instance.StartVideoChat(roomId);
}

#endregion

//=============================================Region Divider======================================

#region Common & ETC

    private IEnumerator CheckingServerConnection()
    {
        while (ChatLoungeRoom.colyseusConnection.State.Equals(WebSocketState.Connecting) ||
               ChatLoungeRoom.colyseusConnection.State.Equals(WebSocketState.Open) ||
               Room.colyseusConnection.State.Equals(WebSocketState.Connecting) ||
               Room.colyseusConnection.State.Equals(WebSocketState.Open) ||
               AgoraManager.Instance.RtcEngine.GetConnectionState() == CONNECTION_STATE_TYPE.CONNECTION_STATE_CONNECTED ||
               AgoraManager.Instance.RtcEngine.GetConnectionState() == CONNECTION_STATE_TYPE.CONNECTION_STATE_CONNECTING||
               AgoraManager.Instance.RtcEngine.GetConnectionState() == CONNECTION_STATE_TYPE.CONNECTION_STATE_RECONNECTING)
        {
            yield return new WaitForSeconds(5.0f);
        }
        LocalizationController.Instance.WaitLocaleText((localeText) => {
            NativeManager.Instance.SendBasicAlert(localeText);
        }, "ServerManager_AlertDisconnect");
        //NativeManager.Instance.SendBasicAlert("세션이 종료되었습니다.\n토리라운지에 다시 입장해주세요.");
        yield return null;
    }

    public static void NetSend(string action, object message = null)
    {
        if (Instance.Room == null)
        {
            LSLog.LogError($"Error: Not in room for action {action} msg {message}");
            return;
        }

        _ = message == null ? Instance.Room.Send(action) : Instance.Room.Send(action, message);
    }

    public void SendObjectInteraction(Interactable interactable, NetworkedEntity entity)
    {
        LSLog.Log("Sending object interaction for ID " + interactable.ID);
        NetSend("objectInteracted", new object[] {interactable.ID, interactable.GetServerType()});
    }

    private IEnumerator Co_ServerRequest<T>(string method, string url, WWWForm form, Action<RequestResponse> onComplete) where T : RequestResponse
    {
        
        string fullURL = (PlayerData.myPlayerinfo.isAppRelease)
            ?$"{_colyseusSettings.WebRequestEndpoint}/{url}"
            :$"{_colyseusTestSettings.WebRequestEndpoint}/{url}";
        
        UnityWebRequest request = null;

        //Debug.Log("request url : " + fullURL);
        switch (method)
        {
            case "POST":
                request = UnityWebRequest.Post(fullURL, form);
                break;
            //case "GET":
            //    break;
            default:
                LSLog.LogImportant($"Unsupported Server Request Type - {method}", LSLog.LogColor.yellow);

                onComplete?.Invoke(new RequestResponse() { error = true, output = $"Unsupported Server Request Type - {method}" });

                yield break;
        }

        if (request == null)
        {
            onComplete?.Invoke(new RequestResponse() { error = true, output = $"Error making web request!" });
            yield break;
        }

        //Debug.Log("[Send] Send Request " + request.url);
        UnityWebRequestAsyncOperation op = request.SendWebRequest();

        while (op.isDone == false)
        {
            yield return 0;
            ChatLoungeProcessDone = true;
        }

        RequestResponse response = null;

        try
        {
            response = string.IsNullOrEmpty(request.error)
                ? JsonUtility.FromJson<T>(request.downloadHandler.text)
                : JsonUtility.FromJson<RequestResponse>(request.downloadHandler.text);

            response.rawResponse = request.downloadHandler.text;

        }
        catch (System.Exception err)
        {
            response = new RequestResponse() { error = true, output = $"{err.Message}" };
            ServerManager.Instance.ChatLoungeProcessDone = true;
        }

        request.Dispose();

        int wait = 0;
        while (!SceneManager.GetActiveScene().name.Equals(GameEvents.Instance.CheckUniversityCode()))
        {
            //Debug.Log("CheckPoint Co_ServerRequest Wait FrameCnt " + wait++ + " // SceneManager.GetActiveScene().name : " + SceneManager.GetActiveScene().name + " // GameEvents.Instance.CheckUniversityCode() : " + GameEvents.Instance.CheckUniversityCode());
            yield return new WaitForEndOfFrame();
        }
        
        onComplete?.Invoke(response);
    }

    public void ConnectToServer()
    {
        if(isConnected) return;
        try
        {
            ColyseusSettings clonedSettings = CloneSettings();
            clonedSettings.colyseusServerAddress = ColyseusServerAddress;
            clonedSettings.colyseusServerPort = ColyseusServerPort;
            clonedSettings.useSecureProtocol = ColyseusUseSecure;

            OverrideSettings(clonedSettings);
            InitializeClient();
        
            Debug.Log("Login Start"); //UnityEngine.Random.Range(0, 100).ToString()
            isConnected = true;
            //GetComponent<CreateUserMenu>().TryConnect(PlayerData.myPlayerinfo.universityCode,(error) => { Debug.LogError($"Login 에러 l {error} l"); });
        }
        catch (Exception e)
        {
            Debug.Log("ConnectToServer() - exception : "+ e.StackTrace);
        }
    }
    
    
    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        Room?.Leave(true);
    }

    public void DisconnectAll()
    {
        UnregisterHandlers();
        UnregisterHandlersForChatRoom();
        LeaveRooms();
        LeaveChatRoom();
        LeaveVideoChat();
        StartCoroutine(WaitTillDisconnection());
    }
    
    private IEnumerator WaitTillDisconnection()
    {
        while (Room != null || ChatLoungeRoom != null ||  AgoraManager.Instance.RtcEngine != null)
        {
            yield return new WaitForUpdate();
        }
        
        NativeManager.Instance.SendDisconnectCompleted();
        
        yield return null;
    }
#endregion

}
