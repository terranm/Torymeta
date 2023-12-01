using System.Collections;
using System.Collections.Generic;
using Colyseus;
using Colyseus.Schema;
using LucidSightTools;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    public float messageShowTime;

    private static ChatManager instance;
    [SerializeField] private InputField inputField;
    public static ChatManager Instance
    {
        get
        {
            if (instance == null)
            {
                LSLog.LogError("No ChatManager in scene!");
                instance = FindObjectOfType<ChatManager>();
            }
            return instance;
        }
    }

    private ColyseusRoom<ChatRoomState> chatRoom;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        
        inputField.onSubmit.AddListener(delegate {
            SendChat();
            //Debug.Log("onSubmit end");
        });
    }
    
    private void Update()
        {
            if (Input.GetKey(KeyCode.KeypadEnter))
            {
                Debug.Log("GetKey !!!!!!!!!!!!");
            }
            
            if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                Debug.Log("GetKeyDown !!!!!!!!!!!!!!!!!");
            }
        }

    /// <summary>
    /// Hand the manager the current ChatRoom 
    /// </summary>
    /// <param name="room"></param>
    public void SetRoom(ColyseusRoom<ChatRoomState> room)
    {
        chatRoom = room;
        RegisterForMessages();
        ConnectIDs();
    }

    private void RegisterForMessages()
    {
        if (chatRoom != null)
        {
            // chatRoom.OnStateChange += ChatRoomOnOnStateChange;
        }
    }

    private void UnregisterForMessages()
    {
        if (chatRoom != null)
        {
            // chatRoom.OnStateChange -= ChatRoomOnOnStateChange;
        }
    }

    //Chat room ID and MMO Room ID will be different, need to connect those values
    private void ConnectIDs()
    {
        NetworkedEntity entity = NetworkedEntityFactory.Instance.GetMine();
        if (entity && chatRoom != null)
        {
            entity.SetChatID(chatRoom.SessionId);
        }
    }

    public void ChatRoomOnOnStateChange(RoomState state, bool isfirststate)
    {
        Debug.LogError("ChatRoomOnOnStateChange");
        //We have at least 1 message
        if (state.chatQueue.Count > 0)
        {
            HandleMessages(state.chatQueue);
        }
    }

    private void HandleMessages(MapSchema<ChatQueue> chatQueue)
    {
        chatQueue.ForEach((clientID, queue) => { NetworkedEntityFactory.Instance.HandMessages(clientID, queue); });
    }

    public void SendChat()
    {
        if (inputField.interactable)
        {
            ServerManager.Instance.Room.Send("sendChat", inputField.text);
            inputField.text = "";
        }
    }

    public void LeaveChatroom()
    {
        Debug.Log("LeaveChatRoom");
        UnregisterForMessages();
        chatRoom?.Leave(true);
    }
}
