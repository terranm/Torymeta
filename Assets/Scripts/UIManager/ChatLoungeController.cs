using System;
using System.Collections;
using System.Collections.Generic;
using Colyseus;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UIElements.Image;

public class ChatLoungeController : MonoBehaviour
{
    
    [SerializeField] private GameObject _objRoom;
    [SerializeField] private TMP_Text _txtRoomTitle;
    [SerializeField] private TMP_Text _txtRoomMember;
    [SerializeField] private Button _BtnEnter;

    [SerializeField] private GameObject _objPrivateRoom;
    [SerializeField] private TMP_Text _txtPrivateRoomTitle;
    [SerializeField] private TMP_Text _txtPrivateRoomMember;
    [SerializeField] private Button _BtnPrivateEnter;
    
    [SerializeField] private GameObject _objIconFar;
    
    [SerializeField] private GameObject _BtnCreate;

    [SerializeField] private Sprite _sprEnter;
    [SerializeField] private Sprite _sprEnterDisable;
    
    private GameObject player;
    private GameObject table;
    private RoomInfo Info;

    public float dist = 0;

    public bool isInitComplete;

    private void Awake()
    {
        isInitComplete = false;
    }

    public IEnumerator Init()
    {
        while (NetworkedEntityFactory.Instance.GetMine() == null)
        {
            yield return new WaitForUpdate();
        }
        player = NetworkedEntityFactory.Instance.GetMine().gameObject;
        
        GameEvents.Instance.OnRequestFindAndJoinChat += FindAndJoinChat;
        GameEvents.Instance.OnRequestFindAndJoinChatByRoomId += FindAndJoinChatByRoomId;

        if (!string.IsNullOrEmpty(PlayerData.myPlayerinfo.goToTableId))
        {
            StartCoroutine(WaitTillInfoSetAndJoin());
            //FindAndJoinChat(PlayerData.myPlayerinfo.goToTableId);
        }

        isInitComplete = true;
        yield return null;
    }

    private void OnDestroy()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnRequestFindAndJoinChat -= FindAndJoinChat;
            GameEvents.Instance.OnRequestFindAndJoinChatByRoomId -= FindAndJoinChatByRoomId;
        }
    }

    private IEnumerator WaitTillInfoSetAndJoin()
    {
        while (table == null)
        {
            yield return new WaitForSeconds(0.5f);
        }

        if (table.name.Equals(PlayerData.myPlayerinfo.goToTableId))
        {
            if (Info != null)
                FindAndJoinChat(PlayerData.myPlayerinfo.goToTableId);
            else
                LocalizationController.Instance.WaitLocaleText((localeText) => {
                    NativeManager.Instance.SendBasicAlert(/*"입장에 실패 했습니다."*/localeText);
                }, "ChatLoungeController_AlertChatJoinFail");
            
        }

        yield return null;
    }

    private void FindAndJoinChatByRoomId(string roomId)
    {
        if (Info != null)
        {
            if (Info.roomId.Equals(roomId))
            {
                if (Info.clients < Info.maxClients)
                {
                    int count = 0;
                    for (int i = 0; i < table.transform.childCount; i++)
                    {
                        if (table.transform.GetChild(i).name.Substring(0, 2).Equals("C0"))
                            count++;
                    }

                    ServerManager.Instance.ClickedChattingLoungeChairCount = count;
                    ServerManager.Instance.ClickedChattingLoungeOBJ = table;
                    ServerManager.Instance.ChatLoungeProcessDone = true;
                    ServerManager.Instance.ClickedChattingLoungeRoomInfo = Info;
                    ServerManager.Instance.JoinChattRoomByIdWithPassword(Info.password);
                }
            }
        }
    }
    
    private void FindAndJoinChat(string tableId)
    {
        if (Info != null)
        {
            if (Info.tableId.Equals(tableId))
            {
                if (Info.clients < Info.maxClients)
                {
                    int count = 0;
                    for (int i = 0; i < table.transform.childCount; i++)
                    {
                        if (table.transform.GetChild(i).name.Substring(0, 2).Equals("C0"))
                            count++;
                    }

                    ServerManager.Instance.ClickedChattingLoungeChairCount = count;
                    ServerManager.Instance.ClickedChattingLoungeOBJ = table;
                    ServerManager.Instance.ChatLoungeProcessDone = true;
                    ServerManager.Instance.ClickedChattingLoungeRoomInfo = Info;
                    ServerManager.Instance.JoinChattRoomByIdWithPassword(Info.password);
                }
            }
        }
    }

    public void SetInfo(RoomInfo _Info,GameObject _table)
    {
        if (Info == null)//초기세팅
            {
                if (_Info != null)
                {
                    Info = _Info;
                    //_objIconFar.SetActive(false);
                    //_objRoom.SetActive(false);
                    //_objPrivateRoom.SetActive(false);
                    //_BtnCreate.gameObject.SetActive(false);
                }
                table = _table;
            }
            else if (_Info == null)//초기화
            {
                Info = _Info;
                //_objIconFar.SetActive(false);
                //_objRoom.SetActive(false);
                //_objPrivateRoom.SetActive(false);
                //_BtnCreate.gameObject.SetActive(false);
                table = _table;
            }
            else if(!Info.roomId.Equals(_Info.roomId) 
                    || !Info.clients.Equals(_Info.clients) 
                    || !Info.privateRoom.Equals(_Info.privateRoom))//갱신
            {
                Info = _Info;
                table = _table;
                //_objIconFar.SetActive(false);
                //_objRoom.SetActive(false);
                //_objPrivateRoom.SetActive(false);
                //_BtnCreate.gameObject.SetActive(false);
            }
    }

    private void OnTriggerStay(Collider other)
    {
        if (Info != null)
            {
                _BtnCreate.gameObject.SetActive(false);
                _objIconFar.SetActive(false);
                if (Info.privateRoom) //비공개방
                {
                    _objPrivateRoom.SetActive(true);
                    _txtPrivateRoomTitle.text = Info.title;
                    _txtPrivateRoomMember.text = string.Format("{0}/{1}", Info.clients, Info.maxClients);
                    if (Info.clients < Info.maxClients)
                    {
                        _BtnEnter.gameObject.GetComponent<UnityEngine.UI.Image>().sprite = _sprEnter;
                    }
                    else
                        _BtnEnter.gameObject.GetComponent<UnityEngine.UI.Image>().sprite = _sprEnterDisable;

                    LayoutRebuilder.ForceRebuildLayoutImmediate(_objPrivateRoom.GetComponent<RectTransform>());
                }
                else //공개방
                {
                    _objRoom.SetActive(true);
                    _txtRoomTitle.text = Info.title;
                    _txtRoomMember.text = string.Format("{0}/{1}", Info.clients, Info.maxClients);
                    if (Info.clients < Info.maxClients)
                    {
                        _BtnPrivateEnter.gameObject.GetComponent<UnityEngine.UI.Image>().sprite = _sprEnter;
                    }
                    else
                        _BtnPrivateEnter.gameObject.GetComponent<UnityEngine.UI.Image>().sprite = _sprEnterDisable;
                    
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_objRoom.GetComponent<RectTransform>());
                }

                if (NetworkedEntityFactory.Instance.GetMine().Table.Equals(table.name))
                {
                    _objIconFar.SetActive(false);
                    _objRoom.SetActive(false);
                    _objPrivateRoom.SetActive(false);
                    _BtnCreate.gameObject.SetActive(false);
                }
            }
        else
        {
            _BtnCreate.gameObject.SetActive(true);
            if(_objIconFar.activeSelf)
                _objIconFar.SetActive(false);
            if(_objRoom.activeSelf)
                _objRoom.SetActive(false);
            if(_objPrivateRoom.activeSelf)
                _objPrivateRoom.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (Info != null)
        {
            if (!_objIconFar.activeSelf)
            {
                _objIconFar.SetActive(true);
                _objRoom.SetActive(false);
                _objPrivateRoom.SetActive(false);
            }
        }
        else
        {
            _objIconFar.SetActive(false);
            _objRoom.SetActive(false);
            _objPrivateRoom.SetActive(false);
            _BtnCreate.gameObject.SetActive(false);
        }

        if (_BtnCreate.gameObject.activeSelf)
        {
            _BtnCreate.gameObject.SetActive(false);
        }
    }
    
    public void CreateChat()
    {
        int count = 0;
        for (int i = 0; i < table.transform.childCount; i++)
        {
            if (table.transform.GetChild(i).name.Substring(0, 2).Equals("C0"))
                count++;
        }

        ServerManager.Instance.ClickedChattingLoungeChairCount = count;
        ServerManager.Instance.ClickedChattingLoungeOBJ = table;
        ServerManager.Instance.ChatLoungeProcessDone = true;
        
        NativeManager.Instance.SendChatLoungeCreateMessage();
    }
    
    public void JoinChat()
    {
        if (Info.clients < Info.maxClients)
        {
            int count = 0;
            for (int i = 0; i < table.transform.childCount; i++)
            {
                if (table.transform.GetChild(i).name.Substring(0, 2).Equals("C0"))
                    count++;
            }

            ServerManager.Instance.ClickedChattingLoungeChairCount = count;
            ServerManager.Instance.ClickedChattingLoungeOBJ = table;
            ServerManager.Instance.ChatLoungeProcessDone = true;
            ServerManager.Instance.ClickedChattingLoungeRoomInfo = Info;
            if(Info.privateRoom)
                NativeManager.Instance.SendChatLoungePasswordInput(Info.password);
            else
            {
#if UNITY_EDITOR
                ServerManager.Instance.JoinChattRoomByIdWithPassword("");
#else
                NativeManager.Instance.SendJoinPublicRoomConfirm();
#endif                
            }
        }
    }

    public void SetUIObjectOff(bool leaving = false)
    {
        _objIconFar.SetActive(false);
        _objRoom.SetActive(false);
        _objPrivateRoom.SetActive(false);
        _BtnCreate.gameObject.SetActive(false);
        if (leaving)
            Info = null;
    }
}
