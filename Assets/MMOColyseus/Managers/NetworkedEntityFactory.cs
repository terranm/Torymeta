using System;
using System.Collections.Generic;
using System.Linq;
using LucidSightTools;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkedEntityFactory : MonoBehaviour
{
    private static NetworkedEntityFactory instance;

    private NetworkedEntity myPlayer;
    private Animator anim;

    public static NetworkedEntityFactory Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindGameObjectWithTag("ServerManager").GetComponent<NetworkedEntityFactory>();
                // LSLog.LogError("No NetworkedEntityFactory in scene!");
            }
            return instance;
        }
    }

    [SerializeField]
    private Dictionary<string, NetworkedEntity> entities = new Dictionary<string, NetworkedEntity>();

    public CameraController cameraController;

    private string _ourEntityId;
    
    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void SetCameraTarget(Transform target)
    {
        // cameraController.SetFollow(target);
    }

    /// <summary>
    /// Instantiates a new player object setting its position and rotation as it is in the state. 
    /// </summary>
    /// <param name="state">The state for the player entity.</param>
    /// <param name="isPlayer">Will this entity belong to this client?</param>
    public void SpawnEntity(NetworkedEntityState state, bool isPlayer = false)
    {
        if (isPlayer)
        {
            _ourEntityId = state.entityId;
        }
        
        Vector3 position = new Vector3((float)state.xPos, (float)state.yPos, (float)state.zPos);
        Quaternion rot = new Quaternion((float)state.xRot, (float)state.yRot, (float)state.zRot, state.wRot);

        // Spawn the entity while also making it a child object of the grid area

        //string avatar = PlayerData.myPlayerinfo.avatar;

        //Debug.Log("state.avatar.bodyCode" + state.avatar.bodyCode);

        //if (!string.IsNullOrEmpty(state.avatar.bodyCode))
        //    avatar = state.avatar.bodyCode;

        GameObject newEntity = Instantiate(Resources.Load("BODY_AA_001") as GameObject, Vector3.zero, Quaternion.identity);//, position, rot);
        new WaitForUpdate();
        // newEntity.transform.SetParent(EnvironmentController.Instance.CurrentArea.transform);
        NetworkedEntity entity = newEntity.GetComponentInChildren<NetworkedEntity>();

        if (isPlayer)
        {
            PlayerData.myPlayerinfo.entityId = state.entityId;
            entity.GetComponent<CharacterController>().enabled = true;
            entity.GetComponent<StarterAssets.StarterAssetsInputs>().enabled = true;
            entity.GetComponent<AnimationController>().enabled = false;

            StartCoroutine(DownloadManager.Instance.KillLoadingProcess());
            myPlayer = entity;
            anim = myPlayer.GetComponent<Animator>();
            GameEvents.Instance.CompleteLoadScene();
            Debug.Log("Spawn My Entity");
        }

        if (!entities.ContainsKey(state.entityId))
            entities.Add(state.entityId, entity);

        StartCoroutine(entity.Initialize(state, isPlayer));
    }

    public void MakeEntityDoAction(string key, ActionState value)
    {
        if(entities.Equals(null) || myPlayer.Equals(null)) return;
        if (entities.ContainsKey(key) && !key.Equals(myPlayer.Id))
        {
            Animator anim = entities[key].GetComponent<Animator>();
            
             if (value.actionId.Equals("Stand"))
             {
                 entities[key]._objMicOff.SetActive(false);
                 entities[key]._objCamOff.SetActive(false);
                 entities[key]._objChatLoungePanel.SetActive(false);
                 entities[key]._objChatLoungePanelCover.SetActive(false);
                 
                 if(anim.GetBool("isItsMe"))anim.SetBool("isItsMe", false);
                 anim.SetBool("isSit",false);
                 // entity.GetComponent<CharacterController>().enabled = false;
                 return;
             }

             if (anim.GetBool("isItsMe") && value.actionId.Equals("isItsMe"))
             {
                 anim.SetBool("isItsMe", false);
             }
             else
             {
                 if (anim.GetBool("isJumpStart") && value.actionId.Equals("isJumpStart")) return;
                 anim.SetBool(value.actionId, true);
             }
        }
    }

    public NetworkedEntity GetEntity(string entityId)
    {
        if (entities.ContainsKey(entityId))
        {
            return entities[entityId];
        }
        
        Debug.Log("GetEntity Check " + entityId);

        return null;
    }
    
    public NetworkedEntity GetEntityForChat(string entityId)
    {
        if (entities.ContainsKey(entityId))
        {
            // && string.IsNullOrEmpty(entities[entityId].Seat)
            // Seat 이 null 이거나 0이면 서있는 상태
            // Seat 에 ^ 가 없으면 일반 의자에 앉은 상태
            // Seat 에 ^ 가 있으면 채팅라운지 앉은 상태
            bool isChatReceive = false;
            if (string.IsNullOrEmpty(GetMine().Seat) || GetMine().Seat.Equals("0")) // 내가 서있고
            {
                if (string.IsNullOrEmpty(entities[entityId].Seat) || entities[entityId].Seat.Equals("0")) // 상대가 서있는 경우에
                {
                    isChatReceive = true;
                }
                if (entities[entityId].Table.Equals("0")) // 상대가 일반의자에 앉은 경우에 
                {
                    isChatReceive = true;
                }
            }
            else if (GetMine().Table.Equals("0")) // 내가 일반의자에 앉은 상태에서 
            {
                if (string.IsNullOrEmpty(entities[entityId].Seat) || entities[entityId].Seat.Equals("0")) // 상대가 서있는 경우에
                {
                    isChatReceive = true;
                }
                if (entities[entityId].Table.Equals("0")) // 상대가 일반의자에 앉은 경우에 
                {
                    isChatReceive = true;
                }
            }
            else // 내가 채팅라운지에 들어가있는 상태에서 
            {
                if (entities[entityId].Table.Equals(GetMine().Table)) // 나와 채팅 테이블이 같으면 
                    isChatReceive = true;
            }

            if (isChatReceive)
            {
                return entities[entityId]; // 채팅받음 
            }
            else
            {
                return null; // 안받음 
            }
        }
        
        Debug.Log("GetEntity Check " + entityId );

        return null;
    }
    
    public NetworkedEntity GetMyEntity()
    {
        return entities.ContainsKey(PlayerData.myPlayerinfo.entityId) ? entities[PlayerData.myPlayerinfo.entityId] : null;
    }

    /// <summary>
    /// Updates this client's entity with the new state.
    /// </summary>
    /// <param name="state">The state to update this client's entity with.</param>
    /// <returns></returns>
    public bool UpdateOurEntity(NetworkedEntityState state)
    {
        if (entities.ContainsKey(_ourEntityId))
        {
            NetworkedEntity entity = entities[_ourEntityId];
            entities.Remove(_ourEntityId);

            entity.Initialize(state, true);

            _ourEntityId = state.entityId;

            entities.Add(_ourEntityId, entity);

            return true;
        }

        LSLog.LogError($"Missing our entity? - \"{_ourEntityId}\"");

        return false;
    }

    /// <summary>
    /// Removes the entity, keyed by session Id, from the controlled entities and
    /// destroys the player game object.
    /// </summary>
    /// <param name="id"></param>
    public void RemoveEntity(string id)
    {
        if (entities.ContainsKey(id))
        {
            NetworkedEntity entity = entities[id];
            entities.Remove(id);
            Destroy(entity.gameObject);
        }
    }

    public void HandMessages(string id, ChatQueue queue)
    {
        foreach (var entry in entities.Where(entry => entry.Key.Equals(id)))
        {
            entry.Value.HandMessages(queue);
        }
    }


    /// <summary>
    /// Returns the <see cref="NetworkedEntity"/> belonging to this client.
    /// </summary>
    /// <returns></returns>
    public NetworkedEntity GetMine()
    {
        return myPlayer;
    }

    /// <summary>
    /// Returns the <see cref="NetworkedEntity"/> belonging to the given session Id if one exists.
    /// </summary>
    /// <param name="sessionId">The session Id of the desired <see cref="NetworkedEntity"/></param>
    /// <returns></returns>
    public NetworkedEntity GetEntityByID(string sessionId)
    {
        if (entities.ContainsKey(sessionId))
        {
            return entities[sessionId];
        }

        return null;
    }

    /// <summary>
    /// Clears the collection of controlled <see cref="NetworkedEntity"/>s and destroys all the linked player game objects.
    /// </summary>
    /// <param name="excludeOurs">If true the <see cref="NetworkedEntity"/> and player game object belonging to this client will not be removed and destroyed.</param>
    public void RemoveAllEntities(bool excludeOurs)
    {
        List<string> keys = new List<string>(entities.Keys);

        for (int i = keys.Count - 1; i >= 0; i--)
        {
            if (entities[keys[i]].isMine && excludeOurs)
            {
                continue;
            }

            Destroy(entities[keys[i]].gameObject);

            entities.Remove(keys[i]);
        }
    }

    public void MakePlayerChatJoin(GameObject table)
    {
        List<bool> sitable = new List<bool>();
        
        for (int i = 0; i < ServerManager.Instance.ClickedChattingLoungeChairCount; i++)
            sitable.Add(true);
        
        foreach (var item in entities)
        {
            if (table.name.Equals(item.Value.Table))
                sitable[int.Parse(item.Value.Seat.Substring(2)) - 1] = false;
        }
        
        Transform sitobj = null;

        for (int i = 0; i < sitable.Count; i++)
        {
            if (sitable[i])
            {
                for (int j = 0; j < table.transform.childCount; j++)
                {
                    if (table.transform.GetChild(j).name.Substring(2).Equals((i+1).ToString()))
                    {
                        sitobj = table.transform.GetChild(j);
                        break;
                    }
                }
            }
            if(sitobj != null) break;
        }

        if (sitobj != null)
        {
            myPlayer.GetComponent<CharacterController>().enabled = false;
            table.GetComponentInChildren<ChatLoungeController>().SetUIObjectOff();
            
            Vector3 pos = new Vector3(sitobj.position.x,
                sitobj.position.y - 1f /*0.75f*/,
                sitobj.position.z);

            myPlayer.transform.position = pos - sitobj.up * 0.35f;
            myPlayer.transform.rotation = Quaternion.LookRotation(sitobj.up);
            
            SetAnimation();

            // anim.SetBool("OnEmotion", true);
            anim.SetBool("isSit", true);
            myPlayer.SendRFC("isSit",sitobj.name,table.name);
            Debug.Log("seat : " + sitobj.name + "\ttable : " + table.name);
            ServerManager.Instance.InitializeVideoChat(ServerManager.Instance.ChatLoungeRoom.Id);
        }
    }
    
    public void MakePlayerSeat(GameObject hit)
    {
        if (myPlayer.Seat.Contains("c")||myPlayer.Seat.Contains("C")) return;
        if (myPlayer.Seat.Contains("c")||myPlayer.Seat.Contains("C")) return;
        if (Vector3.Distance(hit.transform.position, myPlayer.transform.position) > 10) return;
        
        myPlayer.GetComponent<CharacterController>().enabled = false;
        
        Vector3 pos = new Vector3(hit.transform.position.x,
            hit.transform.position.y-1f,
            hit.transform.position.z);
        
        myPlayer.transform.position = pos - hit.transform.up*0.35f;
        myPlayer.transform.rotation = Quaternion.LookRotation(hit.transform.up);

        SetAnimation();

        // anim.SetBool("OnEmotion", true);
        anim.SetBool("isSit", true);
        myPlayer.SendRFC("isSit",hit.name);
    }

    public void MakePlayerAction(string action)
    {
        //SetAnimation();
        bool isOn = anim.GetBool(action);
        anim.SetBool(action, !isOn);
        
        myPlayer.SendRFC(action);
    }

    public void MakePlayerAction_Trigger(string action)
    {
        anim.SetTrigger(action);

        myPlayer.SendRFC(action);
    }

    private void SetAnimation()
    {
        // anim.SetBool("OnEmotion", false);
        anim.SetBool("isItsMe", false);
        //anim.SetBool("Fighting", false);
        //anim.SetBool("Hello", false);
        anim.SetBool("isSit", false);
    }

    public void SetEntityCanvasReadyByMemberId(int memberId, string channelId)
    {
        List<NetworkedEntity> elist = entities.Values.ToList();
        foreach (var entity in elist)
        {
            if (memberId == entity.MemberID)
            {
                entity._objChatLoungePanel.SetActive(true);
                entity._objChatLoungePanelCover.SetActive(true);
                new WaitForUpdate();
                int temp = (memberId == PlayerData.myPlayerinfo.memberId) ? 0 : memberId;
                ServerManager.Instance.StartVideoChat(temp,entity._objChatLoungePanel, channelId);
                Debug.Log("Set Video parent active and set");
                break;
            }
        }
    }
    
    public void ClearChatCanvas()
    {
        List<NetworkedEntity> elist = entities.Values.ToList();
        foreach (var entity in elist)
        {
            if (myPlayer.Table == entity.Table)
            {
                Transform[] childs = entity._objChatLoungePanel.GetComponentsInChildren<Transform>();
                if (childs != null)
                {
                    for (int i = 1; i < childs.Length; i++)
                    {
                        if (childs[i] != entity._objChatLoungePanel.transform)
                        {
                            //Debug.Log("destroied obj : " + childs[i].gameObject.name);
                            Destroy(childs[i].gameObject);
                        }
                    }
                }
                GameEvents.Instance.RequestNormalizeChatUI();
                entity._objChatLoungePanel.SetActive(false);
                entity._objChatLoungePanelCover.SetActive(false);
                entity._objMicOff.SetActive(false);
                entity._objCamOff.SetActive(false);
            }
        }
        anim.SetBool("isSit", false);
    }
    
    public void ClearEntities()
    {
        entities.Clear();
    }

    public void SetEntityCamOff(int memberId,bool mute,bool Kill=false)
    {
        List<NetworkedEntity> elist = entities.Values.ToList();
        foreach (var entity in elist)
        {
            if (entity.MemberID == memberId)
            {
                entity._objChatLoungePanel.SetActive((Kill) ? false : !mute);
                entity._objCamOff.SetActive((Kill) ? false : mute);
                if (Kill)
                {
                    entity._objChatLoungePanelCover.SetActive(false);
                    entity._objMicOff.SetActive(false);
                }
                break;
            }
        }
    }
    
    public void SetEntityMicOff(int memberId,bool mute)
    {
        List<NetworkedEntity> elist = entities.Values.ToList();
        foreach (var entity in elist)
        {
            if (entity.MemberID == memberId)
            {
                entity._objMicOff.SetActive(mute);
                break;
            }
        }
    }

    public void UpdateEntityInfo(string Id,NetworkedEntityState state)
    {
        if(entities == null) return;
        if (entities.Count <= 0) return;
        //Debug.Log("UpdateEntityInfo " + entities.Count);
        foreach (var item in entities.Values.ToList())
        {
            if (item.Id.Equals(state.entityId) && !item.MemberID.Equals(PlayerData.myPlayerinfo.memberId))
            {
                item.Table = state.table;
                item.Seat = state.seat;
                break;;
            }
        }
    }
    
    public void UpdateEntityRoomMakerInfo(string Id,NetworkedEntityState state)
    {
        foreach (var item in entities.Values.ToList())
        {
            if (item.MemberID.Equals((int)state.memberId))
            {
                if (item.MemberID == myPlayer.MemberID && !myPlayer.RoomMaker && state.roomMaker)
                {
                    //{#위임받은사람}님이 새로운 방장이 되었습니다.
                    ServerManager.Instance.Room.Send("sendChat", string.Format("#SYSMSG#{0}#RoomID#{1}#Entry#{2}",
                        ServerManager.Instance.ChatLoungeRoom.Id, "NetworkedEntityFactory_ChatMandateChange", myPlayer.UserName));

                    myPlayer.RoomMaker = state.roomMaker;
                }
                item.RoomMaker = state.roomMaker;
                break;;
            }
        }
    }
}
