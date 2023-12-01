using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class ChatDisplay : MonoBehaviour
{ 
    private List<ChatMessage> messages = new();
    public Text display;
    private string key;

    private void Awake()
    {
        GameEvents.Instance.OnRequstClearChatLog += ClearChatLog;
    }

    void Start()
    {
        display = GameObject.FindWithTag("Selected Channel Text").GetComponent<Text>();
        key = "";
    }

    private void OnDestroy()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnRequstClearChatLog -= ClearChatLog;
        }
    }

    private void ClearChatLog()
    {
        display.text = "";
    }

    public void HandMessages(ChatQueue queue)
    {
        List<ChatMessage> compareList = new List<ChatMessage>();
        queue.chatMessages.ForEach((message) => { compareList.Add(message); });

        messages ??= new List<ChatMessage>();

        // compareList.Sort(MessageSort);
        if (messages.Equals(compareList)) return;
        
        messages = compareList;
        if(messages.Count > 0)
            HandleNewMessages();
        queue.chatMessages.Clear();
    }

    private void HandleNewMessages()
    {
        foreach (var t in messages)
        {
            NetworkedEntity entitiy;
            if(PlayerData.myPlayerinfo.universityCode.Equals("lobby"))
                entitiy = NetworkedEntityFactory.Instance.GetEntityForChat(t.entityId);
            else
                entitiy = NetworkedEntityFactory.Instance.GetEntity(t.entityId);
            
            if(entitiy == null) break;
            
            Debug.Log(entitiy.Id + " : " + t.message);

            if (display == null)
                display = GameObject.FindWithTag("Selected Channel Text").GetComponent<Text>();
            
            if (t.message.Length > 8)
            {
                if(t.message.Substring(0, 8).Equals("#SYSMSG#"))
                {
                    if(ServerManager.Instance.ChatLoungeRoom == null) return;
                    string[] msgParts = t.message.Substring(8).Split("#RoomID#");
                    if (ServerManager.Instance.ChatLoungeRoom.Id.Equals(msgParts[0]))
                    {
                        string inputMsg = "";
                        string entry = "";

                        string[] msgs = inputMsg.Split("#Entry#");
                        entry = msgs[0];
                        string[] columns = msgs[1].Split("#Columns#");
                        string newMsg = "";
                        LocalizationController.Instance.WaitLocaleText((localeText) => {
                            newMsg = string.Format(localeText, columns);
                        }, entry);

                        display.text += string.Format("{0}{1}{2}", "\n<color=red>", newMsg, "</color>");
                    }
                } 
                else
                    display.text += string.Format("{0}{1}{2}{3}","\n",entitiy.UserName," : ",t.message);
            }
            else
                display.text += string.Format("{0}{1}{2}{3}","\n",entitiy.UserName," : ",t.message);
        }
    }

}
