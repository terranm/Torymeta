using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Colyseus;
using Colyseus.Schema;
using LucidSightTools;
using Newtonsoft.Json;
using UniRx;
using UniRx.Triggers;
using Unity.Mathematics;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MMOManager : ColyseusManager<MMOManager>
{
    public delegate void OnRoomChanged(ColyseusRoom<RoomState> room);
    public static event OnRoomChanged onRoomChanged;

    public bool endflag = false;
    public static bool IsReady
    {
        get
        {
            return Instance != null;
        }
    }
    
    public ColyseusRoom<RoomState> Room { get; set; }
    
    public ColyseusRoom<RoomState> ChatLoungeRoom { get; set; }

    private bool isQuitting = false;
    //The chat room
    private ColyseusRoom<ChatRoomState> _chatRoom;

    public Dictionary<string,string> UnseatableOBJNAvatar;
    public Dictionary<string,string> UnseatableTableNId;
    public Dictionary<string,string> JoinRequestNameNId;
    
    public bool responedFlag;
    public bool responed;

    public string ClickedChattingLoungeOBJ;
    public int ClickedChattingLoungeOBJChildCount;
    public string ChatLoungeRoomMakerSitName;
    
    protected override void Awake()
    {
        DontDestroyOnLoad(gameObject);
        isAppRelease = PlayerData.myPlayerinfo.isAppRelease;
        base.Awake();
        UnseatableOBJNAvatar = new Dictionary<string, string>();
        UnseatableTableNId = new Dictionary<string, string>();
        JoinRequestNameNId = new Dictionary<string, string>();
    }

    protected override void Start()
    {
        endflag = false;
        Debug.Log("MMO Manager Start " + SceneManager.GetActiveScene().name);
        
        Application.targetFrameRate = 30;
        InitializeClient();
    }

    public void LeaveRooms()
    {
        base.OnDestroy();

        UnregisterHandlers();

        CleanUpRoom();

        UnseatableOBJNAvatar.Clear();
        UnseatableTableNId.Clear();
        JoinRequestNameNId.Clear();
        // Leave current room
        Room.Leave(true);
        endflag = true;
        Debug.Log("MMO LeaveRooms");
    }


    /// <summary>
    /// Registers handlers for room state events as well as room messages
    /// </summary>
    public void RegisterHandlers()
    {
        if (Room != null)
        {
            Debug.Log("Event 등록 ");
            Room.OnLeave += OnLeaveGridRoom;

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
            Room.OnLeave -= OnLeaveGridRoom;

            Room.OnStateChange -= OnRoomStateChange;

            Room.State.networkedUsers.OnAdd -= NetworkedUsers_OnAdd;
            Room.State.networkedUsers.OnRemove -= NetworkedUsers_OnRemove;
            
            Room.State.action.OnAdd -= ActionState_OnAdd;
            
            // Room.State.action.OnAdd -= ActionState_OnAdd;
        }
    }

    /// <summary>
    /// Removes all entities from the scene except the entity for this client
    /// </summary>
    private void CleanUpRoom()
    {
        NetworkedEntityFactory.Instance.RemoveAllEntities(true);
    }

    private void OnLeaveGridRoom(NativeWebSocket.WebSocketCloseCode code)
    {
        Debug.Log("MMO OnLeave");
        // We have left the current grid room
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
        try
        {

        }
        catch (Exception e)
        {
            Debug.Log("e : " + e.StackTrace);
        }
        StartCoroutine(WaitThenSpawnPlayer(value));
    }

    private void ActionState_OnAdd(string key, ActionState value)
    {
        // Debug.Log("ActionState_OnAdd " + key +" , " + value.actionId);
        NetworkedEntity entity = NetworkedEntityFactory.Instance.GetEntity(key);
        
        if (entity != null)
        {
            Animator anim = entity.GetComponent<Animator>();
            
            if (value.actionId.Equals("Stand"))
            {   
                anim.SetBool("isSit",false);
                // entity.GetComponent<CharacterController>().enabled = false;
                return;
            }
            
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("isItsMe") && value.actionId.Equals("isItsMe"))
                anim.SetBool("isItsMe",false);
            else
            {
                if(!anim.GetCurrentAnimatorStateInfo(0).IsName("Jump") && !value.actionId.Equals("Jump"))
                    anim.SetBool(value.actionId, true);
            }

            Debug.Log(key + ", " + value.actionId);
        }
    }

    /// <summary>
    /// Event handler when the room receives its first state
    /// </summary>
    /// <param name="state"></param>
    /// <param name="isfirststate"></param>
    private void OnRoomStateChange(RoomState state, bool isfirststate)
    {
        state.chatQueue.ForEach((clientID, queue) => { NetworkedEntityFactory.Instance.HandMessages(clientID, queue); });
    }

    public void CreateNewChatLounge(bool isprivate, string roomname, string description, string tableid, int max)
    {
        CreateChattingLounge<RequestResponse>(isprivate,roomname,description,tableid,max,
            (response) => { JoinChattRoomById((error) => { Debug.LogError($"Login 에러 l {error} l"); }, response,tableid); });
    }

    public void LeaveNewClientTest()
    {
        if (ChatLoungeRoom != null)
        {
            ChatLoungeRoom.Leave();
            ChatLoungeRoom = null;
        }
    }
    
    private async void JoinChattRoomById(Action<string> onError, RequestResponse response,string tableId)
    {
        if (response.error)
        {
            onError?.Invoke(response.output);
        }
        else
        {
            Response root = JsonConvert.DeserializeObject<Response>(response.rawResponse);
            ColyseusRoom<RoomState> room = await ChatLoungeJoinById(root.roomId,true);
            
            ChatLoungeRoom = room;
            
            if (ChatLoungeRoom != null)
            {
                UnseatableTableNId.Add(tableId,root.roomId);
                //GameObject.Find("NativeCallObject").GetComponent<NativeCall_Lobby>().MakeMyAvatarSit();
               
            }
            //else
                //GameObject.Find("NativeCallObject").GetComponent<NativeCall_Lobby>().MakeMyAvatarSit(false);            
            //
        }
    }
    
    public async Task<ColyseusRoom<RoomState>> ChatLoungeJoinById(string roomId,bool roommaker=false)
    {
        string userName = string.IsNullOrEmpty(PlayerData.myPlayerinfo.userName)
            ? "USER " + UnityEngine.Random.Range(1, 100)
            : PlayerData.myPlayerinfo.userName;
        NetworkedEntityState state = new NetworkedEntityState()
        {
            username = userName,
            avatar = new AvatarState()
            {
                bodyCode = PlayerData.myPlayerinfo.avatar
            },
            //roomMaker = roommaker,
            memberId = PlayerData.myPlayerinfo.memberId
        };

        ColyseusRoom<RoomState> room = await ChatLoungeclient.JoinById<RoomState>(roomId, new Dictionary<string, object>()
        {
            { "data", JsonConvert.SerializeObject(state) }
        });
        return room;
    }
    
    public async Task<ColyseusRoom<RoomState>> JoinById(string roomId)
    {
        Debug.Log("JoinById " + PlayerData.myPlayerinfo.universityCode);

        string userName = string.IsNullOrEmpty(PlayerData.myPlayerinfo.userName)
            ? "USER " + UnityEngine.Random.Range(1, 100)
            : PlayerData.myPlayerinfo.userName;


        NetworkedEntityState state = new NetworkedEntityState()
        {
            username = userName,
            avatar = new AvatarState()
            {
                bodyCode = PlayerData.myPlayerinfo.avatar
            },
            memberId = PlayerData.myPlayerinfo.memberId,
            seat = "0"
        };

        ColyseusRoom<RoomState> room = await client.JoinById<RoomState>(roomId, new Dictionary<string, object>()
        {
            { "data", JsonConvert.SerializeObject(state) }
        });
        return room;

    }

    private IEnumerator WaitThenSpawnPlayer(NetworkedEntityState state)
    {
        while (!Room.State.networkedUsers.ContainsKey(state.entityId) || SceneManager.GetActiveScene().name.Equals("TempScene"))
        {
            if(!SceneManager.GetActiveScene().name.Equals("TempScene"))
                yield break;
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

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        Room?.Leave(true);
    }
    
    public void UserLogIn<T>(string roomId, Action<RequestResponse> onComplete)
        where T : RequestResponse
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
    
    public void GetChattingLoungeList<T>(Action<RequestResponse> onComplete)
        where T : RequestResponse
    {
        WWWForm form = new WWWForm();
        form.AddField("chatGroup", "chat_lounge"); 
        form.AddField("limit", 1000); 
        StartCoroutine(Co_ServerRequest<T>("POST", "room/list", form, onComplete));
    }
     
    public void CreateChattingLounge<T>(bool isPrivate,string title, string description, string tableId, int max,Action<RequestResponse> onComplete)
        where T : RequestResponse
    {
        WWWForm form = new WWWForm();
        form.AddField("roomId", ""); 
        form.AddField("maxClients", max); 
        form.AddField("title", title); 
        form.AddField("image", "http://127.0.0.1"); 
        form.AddField("creatorName", PlayerData.myPlayerinfo.userName); 
        form.AddField("chatGroup", "chat_lounge"); 
        form.AddField("tableId", tableId); 
        form.AddField("privateRoom", isPrivate? 1:0); 
        form.AddField("description",description);
        form.AddField("memberId", PlayerData.myPlayerinfo.memberId);
        StartCoroutine(Co_ServerRequest<T>("POST", "room/create", form, onComplete));
    }
    
    private IEnumerator Co_ServerRequest<T>(string method, string url, WWWForm form, Action<RequestResponse> onComplete) where T : RequestResponse
    {
        
        string fullURL = (PlayerData.myPlayerinfo.isAppRelease)
            ?$"{_colyseusSettings.WebRequestEndpoint}/{url}"
            :$"{_colyseusTestSettings.WebRequestEndpoint}/{url}";
        
        UnityWebRequest request = null;

        Debug.Log("request url : " + fullURL);
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

        LSLog.Log("[Send] Send Request " + request.url);
        UnityWebRequestAsyncOperation op = request.SendWebRequest();

        while (op.isDone == false)
        {
            yield return 0;
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
        }

        request.Dispose();
        
        onComplete?.Invoke(response);
    }
}
