using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Colyseus.Schema;
using LucidSightTools;
using Newtonsoft.Json;
using StarterAssets;
using TMPro;
using UniRx.Triggers;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NetworkedEntity : MonoBehaviour
{
    
    //Is this entity view representing the current client
    public bool isMine = false;
    public bool isTeleporting = false;
    public Vector3 Vector3TelePos;
    public Vector3 Vector3TeleRot;
    [SerializeField] private TextMeshPro userName; 
    /// <summary>
    /// Getter for the active session Id
    /// </summary>
    public string Id => state.entityId;
    
    public string UserName => state.username;
    
    public int MemberID => (int)state.memberId;
    
    public string Seat;
    
    public string Table;

    public bool RoomMaker;
    
    //public string Table => state.table;

    public GameObject _objChatLoungePanel;
    public GameObject _objChatLoungePanelCover;
    public GameObject _objCamOff;
    public GameObject _objMicOff;
    public GameObject _objMapPointerMine;
    public GameObject _objMapPointerOther;
    
    [SerializeField]
    private NetworkedEntityState state;
    private NetworkedEntityState previousState;
    private NetworkedEntityState localUpdatedState;

    private ActionState actionState;
    
    [SerializeField] private EntityMovement movement = null;

    [SerializeField] public ChatDisplay chatDisplay = null;

    [SerializeField]
    private float updateTimer = 0.5f;


    //Display elements
    [SerializeField]
    private AvatarDisplay avatarModel = null;

    //Movement Sync
    [SerializeField]
    private float maxAngleForSnapRotation = 35f;
    public double interpolationBackTimeMs = 200f;
    public double extrapolationLimitMs = 500f;
    public float positionLerpSpeed = 5f;
    public float rotationLerpSpeed = 5f;
    private Vector3 tempPos;
    
    private bool ignoreMovementSync = false;
    public Animator anim;
    
    private float speed = 0.0f;
    private Vector3 prevPosition;
    private float magnitude = 10;
    
    private string _chatID = "";

    public string ChatID { get { return _chatID; } }

    public int Coins { get { return state != null ? (int)state.coins : 0; } }

    public AvatarState Avatar { get { return localUpdatedState?.avatar; } }

    private PlayerData _playerData;
    private PlayerInput _playerInput;
    private CharctorMeshAndMaterialController _charctorMeshAndMaterialController;

    /// <summary>
    /// Synchronized object state
    /// </summary>
    [System.Serializable]
    private struct EntityState
    {
        public double timestamp;
        public Vector3 pos;
        public Quaternion rot;
    }

    // Clients store twenty states with "playback" information from the server. This
    // array contains the official state of this object at different times according to
    // the server.
    [SerializeField]
    private EntityState[] proxyStates = new EntityState[20];

    private object[] changeSet;
    
    // Keep track of what slots are used
    private int proxyStateCount;

    public bool isInitializeComplete = false;
    private int unchangedCount;

    private AnimationController animCtrl = null;

    public IEnumerator Initialize(NetworkedEntityState initialState, bool isPlayer = false)
    {
        Debug.Log("Entity Initialize Start " + isInitializeComplete);
        Debug.Log("transform name " + transform.name);
        //while (!(DownloadManager.Instance.isDownComplete && TryGetComponent<Animator>(out anim) && TryGetComponent<PlayerData>(out _playerData) && TryGetComponent<PlayerInput>(out _playerInput) && TryGetComponent<CharctorMeshAndMaterialController>(out _charctorMeshAndMaterialController)))
        //{
        //    yield return new WaitForUpdate();
        //}
        if (state != null)
        {// Unsubscribe from existing state events
            state.OnChange -= OnStateChange;
            state.avatar.OnChange -= AvatarOnOnChange;
        }

        isMine = isPlayer;
        state = initialState;
        previousState = state;
        state.OnChange += OnStateChange;
        
        state.avatar.OnChange += AvatarOnOnChange;
        if (!TryGetComponent<Animator>(out anim))
        {
            Debug.Log("Entity Initialize AnimatorTry GetComponent fail");
        }

        // transform.position = PlayerData.GetSpawnPosition(PlayerData.myPlayerinfo.universityCode);
        
        userName.text = state.username;
        
        chatDisplay = new ChatDisplay();
        
        
        if (!isMine)
        {
            TryGetComponent<AnimationController>(out animCtrl);
            _chatID = initialState.chatId;
            if (!state.seat.Equals("0"))
            {
                transform.rotation.Set(state.xRot, state.yRot, state.zRot, state.wRot);
                anim.SetBool("isSit", true);
            }
            transform.position.Set(state.xPos,state.yRot,state.zPos);
            Seat = state.seat;
            Table = state.table;
            _objMapPointerMine.SetActive(false);
            _objMapPointerOther.SetActive(true);
        }
        else
        {
            unchangedCount = 0;
            // DownloadManager.Instance.Download();
            if (TryGetComponent<PlayerData>(out _playerData))
            {
                _playerData.Initialize();
            }
            else
            {
                Debug.Log("Entity Initialize PlayerData Try GetComponent fail");
            }
            //GetComponent<PlayerData>().Initialize();
            if (TryGetComponent<PlayerInput>(out _playerInput))
            {
                _playerInput.enabled = true;
            }
            else
            {
                Debug.Log("Entity Initialize PlayerInput Try GetComponent fail");
            }
            //GetComponent<PlayerInput>().enabled = true;
            Seat = "0";
            Table = "0";
            isTeleporting = false;
            Vector3TelePos = Vector3.zero;
            Vector3TeleRot = Vector3.zero;
            _objMapPointerMine.SetActive(true);
            _objMapPointerOther.SetActive(false);
        }

        if (TryGetComponent<CharctorMeshAndMaterialController>(out _charctorMeshAndMaterialController))
        {
            _charctorMeshAndMaterialController.Init();
            _charctorMeshAndMaterialController.CharacterSetting(state.avatar);
        }
        else
        {
            Debug.Log("Entity Initialize CharctorMeshAndMaterialController Try GetComponent fail");
        }
        //GetComponent<CharctorMeshAndMaterialController>().Init();
        //GetComponent<CharctorMeshAndMaterialController>().CharacterSetting(state.avatar);
        
        isInitializeComplete = true;
        Debug.Log("MemberId :" + initialState.memberId.ToString() +" Entity Initialize End " + isInitializeComplete);
        StartCoroutine(TickSyncServerWithView(0.1f));
        yield return null;
    }

    private void AvatarOnOnChange(List<DataChange> changes)
    {
        //Debug.LogError("AvatarOnOnChange");
        UpdateAvatar();
    }

    public void UpdateAvatar()
    {
        //hsy 임시 주석
        // avatarModel.DisplayFromState(state.avatar);
    }

    // private void Update()
    // {
    //     // if (!isMine)
    //     // {
    //     //     ProcessViewSync();
    //     // }
    //     // else
    //     // {
    //         if (Input.GetKeyDown(KeyCode.Return))
    //         {
    //             Debug.Log("Return");
    //             ChatManager.Instance.SendChat();
    //         }
    //     // }
    // }

    void FixedUpdate()
    {
        //if (isMine)
        //{
        //    if (isTeleporting)
        //    {
        //        if (Vector2.Distance(new Vector2(Vector3TelePos.x, Vector3TelePos.z), new Vector2(transform.position.x, transform.position.z))<3)//Vector3TelePos.Equals(transform.position) && Vector3TelePos.x == state.xPos &&
        //            //Vector3TelePos.y == state.yPos && Vector3TelePos.z == state.zPos)
        //        {
        //            Vector3TelePos = Vector3.zero;
        //            Vector3TeleRot = Vector3.zero;
        //            isTeleporting = false;
        //            GameEvents.Instance.RequestLookAvatarForward();
        //            gameObject.GetComponent<PlayerInput>().enabled = true;
        //            gameObject.GetComponent<StarterAssetsInputs>().enabled = true;
        //            NetworkedEntityFactory.Instance.GetMyEntity().SendRFC("Stand");
        //            anim.SetBool("isSit",false);
        //            //gameObject.GetComponent<StarterAssetsInputs>().LookInput();
        //        }
        //    }
        //    SyncServerWithView();
        //}
        //else
        //{
        if (!isMine)
            ProcessViewSync();
        //}
    }

    private void SyncEntityAnimation()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SendRFC("isJumpStart");
        }
    }

    public void SendRFC(string actionId,string chairName="0",string table ="0")
    {
        if (actionId.Equals("isSit"))
        {
            Seat = chairName;
            Table = table;
            if (table.Equals("0"))
                GameEvents.Instance.ChangeUIBySit(true);
        }
        else if(actionId.Equals("Stand"))
        {
            if (table.Equals("0"))
                GameEvents.Instance.ChangeUIBySit(false);
            Seat = "0";
            Table = "0";
        }
        
        RFC("action", new ActionState() { entityId = Id, actionId = actionId });
    }

    private void RFC(string actionId, ActionState rfc)
    {
        ServerManager.NetSend(actionId, new Dictionary<string, object>()
        {
            {
                "data", JsonConvert.SerializeObject(rfc)
            }
        });
    }
    
    public void RemoteFunctionCallHandler(ActionState _rfc)
    {
        System.Type thisType = GetType();
        MethodInfo theMethod = thisType.GetMethod(_rfc.actionId);
        if (theMethod != null)
            theMethod.Invoke(this, new object[]
            {
                _rfc
            });
        else
            LSLog.LogError("Missing Fucntion: " + _rfc.actionId);
    }
    
    public void SetChatID(string chatID)
    {
        _chatID = chatID;
        SyncServerWithView();   //Let the server know we have a ChatID now
    }



    private IEnumerator TickSyncServerWithView(float tickTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(tickTime);
            if (isMine)
            {
                if (isTeleporting)
                {
                    if (Vector2.Distance(new Vector2(Vector3TelePos.x, Vector3TelePos.z), new Vector2(transform.position.x, transform.position.z)) < 3)//Vector3TelePos.Equals(transform.position) && Vector3TelePos.x == state.xPos &&
                                                                                                                                                       //Vector3TelePos.y == state.yPos && Vector3TelePos.z == state.zPos)
                    {
                        Vector3TelePos = Vector3.zero;
                        Vector3TeleRot = Vector3.zero;
                        isTeleporting = false;
                        GameEvents.Instance.RequestLookAvatarForward();
                        gameObject.GetComponent<PlayerInput>().enabled = true;
                        gameObject.GetComponent<StarterAssetsInputs>().enabled = true;
                        NetworkedEntityFactory.Instance.GetMyEntity().SendRFC("Stand");
                        anim.SetBool("isSit", false);
                        //gameObject.GetComponent<StarterAssetsInputs>().LookInput();
                    }
                }
                SyncServerWithView();
            }
        }
        yield return null;
    }

    /// <summary>
    /// Send this entity's position and rotation values to the server to be synced with all other clients.
    /// </summary>
    public void SyncServerWithView()
    {
        previousState = state.Clone();

        //Copy Transform to State (round position to fix floating point issues with state compare)
        if (!Vector3TelePos.Equals(Vector3.zero))
        {
            transform.position = Vector3TelePos;
            transform.rotation = Quaternion.Euler(Vector3TeleRot);
        }

        state.xPos = (float)System.Math.Round((decimal)transform.localPosition.x, 4);
        state.yPos = (float)System.Math.Round((decimal)transform.localPosition.y, 4);
        state.zPos = (float)System.Math.Round((decimal)transform.localPosition.z, 4);

        if (unchangedCount == 100)
        {
            state.yPos += 0.001f;
        }
        else if(unchangedCount == 200)
        {
            state.yPos -= 0.001f;
            unchangedCount = 0;
        }
        
        state.xRot = transform.rotation.x;
        state.yRot = transform.rotation.y;
        state.zRot = transform.rotation.z;
        state.wRot = transform.rotation.w;

        state.seat = Seat;
        state.table = Table;

        if (!state.chatId.Equals(_chatID))
        {
            state.chatId = _chatID;
        }

        ////No need to update again if last sent state == current view modified state
        if (localUpdatedState != null)
        {
            //TODO: Uses reflection so might be slow, replace with defined compare to improve speed
            List<NetworkedEntityChanges> changesLocal = NetworkedEntityChanges.Compare(localUpdatedState, state);
            if (changesLocal.Count == 0 || (changesLocal.Count == 1 && changesLocal[0].Name == "timestamp"))
            {
                unchangedCount++;
                return;
            }
        }

        //TODO: Uses reflection so might be slow, replace with defined compare to improve speed
        List<NetworkedEntityChanges> changes = NetworkedEntityChanges.Compare(previousState, state);
        // List<NetworkedEntityChanges> changes = NetworkedEntityChanges.Compare(new NetworkedEntityState(), state);
        
        if (changes.Count > 0)
        {
            unchangedCount = 0;
            //Create Change Set Array for NetSend
            changeSet = new object[(changes.Count * 2) + 1];
            changeSet[0] = state.entityId;
            int saveIndex = 1;
            for (int i = 0; i < changes.Count; i++)
            {
                changeSet[saveIndex] = changes[i].Name;
                changeSet[saveIndex + 1] = changes[i].NewValue;
                saveIndex += 2;
            }
            localUpdatedState = state.Clone();
            
            ServerManager.NetSend("entityUpdate", changeSet);
            //Debug.Log("timestamp : " + unchangedCount);
        }
        
    }

    private void OnStateChange(List<DataChange> changes)
    {
        //If not mine Sync
        if (!isMine)
        {
            SyncViewWithServer();
        }
    }

    /// <summary>
    /// Synchronize this entity with the current position and rotation values from the state
    /// </summary>
    private void SyncViewWithServer()
    {
        // Network player, receive data
        Vector3 pos = new Vector3((float)state.xPos, (float)state.yPos, (float)state.zPos);
        Quaternion rot = new Quaternion((float)state.xRot, (float)state.yRot, (float)state.zRot, (float) state.wRot);

        // Shift the buffer sideways, deleting state 20
        for (int i = proxyStates.Length - 1; i >= 1; i--)
        {
            proxyStates[i] = proxyStates[i - 1];
        }

        // Record current state in slot 0
        EntityState newState = new EntityState() { timestamp = state.timestamp }; //Make sure timestamp is in ms
                                                                                  //newState.timestamp = state.timestamp;

        newState.pos = pos;
        newState.rot = rot;
        proxyStates[0] = newState;
        
        proxyStateCount = Mathf.Min(proxyStateCount + 1, proxyStates.Length);

        //Debug.Log("Timestamp inconsistent: " + proxyStates[0].timestamp + " should be greater than " + proxyStates[1].timestamp);
        // Check if states are in order
        if (proxyStates[0].timestamp < proxyStates[1].timestamp)
        {
#if UNITY_EDITOR
            LSLog.Log("Timestamp inconsistent: " + proxyStates[0].timestamp + " should be greater than " + proxyStates[1].timestamp, LSLog.LogColor.yellow);
#endif
        }

        _chatID = state.chatId;
        
    }

    private bool isFirst = true;

    /// <summary>
    /// Lerp this entity's position and rotation towards latest 
    /// </summary>
    protected virtual void ProcessViewSync()
    {
        // if (proxyStates[0].pos.Equals(PlayerData.GetSpawnPosition(PlayerData.myPlayerinfo.universityCode))) return;
        if (proxyStates[0].pos.Equals(Vector3.zero))
        {
            //Debug.Log("MemberID : "+ MemberID +" Users avatar stuct in zero");
            return;
        }
        //if (!Seat.Equals("0")) proxyStates[0].pos.y = transform.localPosition.y;

        transform.position = Vector3.Lerp(transform.localPosition, proxyStates[0].pos,
            Time.deltaTime * (positionLerpSpeed));// * 2.5f);

        if (math.abs(transform.position.y - proxyStates[0].pos.y) > 10)
            transform.position = new Vector3(transform.position.x, proxyStates[0].pos.y, transform.position.z);

        float dis = Vector2.Distance(new Vector2(prevPosition.x, prevPosition.z),
            new Vector2(transform.position.x, transform.position.z));

        speed = Mathf.Lerp(speed, dis > 0.1f ? 15 : 0, Time.deltaTime * magnitude);

        // animation 동작
        anim.SetFloat("Speed", speed);

        if (Mathf.Abs(Quaternion.Angle(transform.rotation, proxyStates[0].rot)) > maxAngleForSnapRotation)
            transform.rotation = proxyStates[0].rot;
        else
            transform.rotation = Quaternion.Slerp(transform.rotation, proxyStates[0].rot,
                Time.deltaTime * (rotationLerpSpeed + 50));

        prevPosition = transform.position;
    }


    public void HandMessages(ChatQueue queue)
    {
        chatDisplay ??= new ChatDisplay();
        
        chatDisplay.HandMessages(queue);
    }

    public void EntityNearInteractable(Interactable interactable)
    {
        movement.SetCurrentInteractable(interactable);
    }

    public void SetMovementEnabled(bool val)
    {
        
    }

    public void SetIgnoreMovementSync(bool ignore)
    {
        ignoreMovementSync = ignore;
    }

    private void OnDestroy()
    {
        if (isMine)
        {
            Debug.Log("MyCharacter Destroyed");
        }
    }

    public NetworkedEntityState GetState()
    {
        return state;
    }
}
