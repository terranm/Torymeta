using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameEvents : Singleton<GameEvents>
{
    //[FormerlySerializedAs("prevUnivCode")] public string currentUnivCode;
    //[FormerlySerializedAs("nextUnivCode")] public string destUnivCode;
    
    public string[] UniversityList =
    {
        "KKU",
        "CAU",
        "SMOONU",
        "HONAU"
    };
    //=============================================Region Divider======================================
    
    #region Native Related event
    //씬전환 이벤트
    public event Action OnRequestSceneChange;
    // ReSharper disable Unity.PerformanceAnalysis
    public void RequestSceneChange()
    {
        if (OnRequestSceneChange != null)
        {
            OnRequestSceneChange();
        }
    }
    
    //캐릭터 변환 이벤트
    public event Action<string> OnRequestCharacterSet;
    public void RequestCharacterSet(string part)
    {
        if (OnRequestCharacterSet != null)
        {
            OnRequestCharacterSet(part);
        }
    }
    
    public event Action OnRequestAvatarRotate;
    public void RequestAvatarRotate()
    {
        if (OnRequestAvatarRotate != null)
        {
            OnRequestAvatarRotate();
        }
    }
    
    public event Action<bool> OnRequestAvatarZoom;
    public void RequestAvatarZoom(bool Zoom)
    {
        if (OnRequestAvatarZoom != null)
        {
            OnRequestAvatarZoom(Zoom);
        }
    }
    
    public event Action<bool> OnRequestCamMove;
    public void RequestCamMove(bool Up)
    {
        if (OnRequestCamMove != null)
        {
            OnRequestCamMove(Up);
        }
    }
    
    public event Action OnRequestSetAvatarInfo;
    public void RequestSetAvatarInfo()
    {
        if (OnRequestSetAvatarInfo != null)
        {
            OnRequestSetAvatarInfo();
        }
    }
    
    public event Action<string> OnRequestFindAndJoinChat;
    public void RequestFindAndJoinChat(string tableId)
    {
        if (OnRequestFindAndJoinChat != null)
        {
            OnRequestFindAndJoinChat(tableId);
        }
    }
    
    public event Action<string> OnRequestFindAndJoinChatByRoomId;
    public void RequestFindAndJoinChatByRoomId(string roomId)
    {
        if (OnRequestFindAndJoinChatByRoomId != null)
        {
            OnRequestFindAndJoinChatByRoomId(roomId);
        }
    }
    
    #endregion
    
    //=============================================Region Divider======================================

    #region Non-UI Click event

    //다음 씬 로드 이벤트 전달
    public event Action<bool> OnRequestZooom;
    public void RequestZooom(bool zoomIn)
    {
        if (OnRequestZooom != null)
        {
            OnRequestZooom(zoomIn);
        }
    }

    public event Action<GameObject> OnClickChatLounge;
    public void ClickChatLounge(GameObject hit)
    {
        if (OnClickChatLounge != null)
        {
            OnClickChatLounge(hit);
        }
    }

    public event Action<GameObject> OnClickTeleportObj;
    public void ClickTeleportObj(GameObject hit)
    {
        if (OnClickTeleportObj != null)
        {
            OnClickTeleportObj(hit);
        }
    }

    public event Action OnClickMediaPlayerMuteBtn;
    public void ClickMediaPlayerMuteBtn()
    {
        if (OnClickMediaPlayerMuteBtn != null)
        {
            OnClickMediaPlayerMuteBtn();
        }
    }

    #endregion

    //=============================================Region Divider======================================

    #region basic event

    public event Action OnCompleteLoadScene;
    public void CompleteLoadScene()
    {
        if (OnCompleteLoadScene != null)
        {
            OnCompleteLoadScene();
        }
    }
    
    public event Action<int,string> OnRequestSetVideoCanvas;
    public void RequestSetVideoCanvas(int memberId, string channelId)
    {
        if (OnRequestSetVideoCanvas != null)
        {
            OnRequestSetVideoCanvas(memberId,channelId);
        }
    }
    
    public event Action OnLeaveChatRoom;
    public void LeaveChatRoom()
    {
        if (OnLeaveChatRoom != null)
        {
            OnLeaveChatRoom();
        }
    }
    
    public event Action OnLeaveRoom;
    public void LeaveRoom()
    {
        if (OnLeaveRoom != null)
        {
            OnLeaveRoom();
        }
    }

    public event Action<bool> OnChangeUIBySit;
    public void ChangeUIBySit(bool sit)
    {
        if (OnChangeUIBySit != null)
        {
            OnChangeUIBySit(sit);
        }
    }
    
    public event Action OnRequestSilencePenalty;
    public void RequestSilencePenalty()
    {
        if (OnRequestSilencePenalty != null)
        {
            OnRequestSilencePenalty();
        }
    }
    
    public event Action OnRequestWarningPenalty;
    public void RequestWarningPenalty()
    {
        if (OnRequestWarningPenalty != null)
        {
            OnRequestWarningPenalty();
        }
    }

    public event Action OnRequestMapActiveConvert;
    public void RequestMapActiveConvert()
    {
        if (OnRequestMapActiveConvert != null)
        {
            OnRequestMapActiveConvert();
        }
    }

    public event Action OnRequestFixedMapActiveConvert;
    public void RequestFixedMapActiveConvert()
    {
        if (OnRequestMapActiveConvert != null)
        {
            OnRequestMapActiveConvert();
        }
    }

    public event Action OnRequestLookAvatarForward;
    public void RequestLookAvatarForward()
    {
        if (OnRequestLookAvatarForward != null)
        {
            OnRequestLookAvatarForward();
        }
    }
    
    public event Action OnRequestNormalizeChatUI;
    public void RequestNormalizeChatUI()
    {
        if (OnRequestNormalizeChatUI != null)
        {
            OnRequestNormalizeChatUI();
        }
    }
    
    public event Action OnRequstMediaPlay;
    public void RequstMediaPlay()
    {
        if (OnRequstMediaPlay != null)
        {
            OnRequstMediaPlay();
        }
    }
    
    public event Action OnRequstClearChatLog;
    public void RequstClearChatLog()
    {
        if (OnRequstClearChatLog != null)
        {
            OnRequstClearChatLog();
        }
    }

    #endregion

    //=============================================Region Divider======================================

    #region static Process
    
    public string CheckUniversityCode()
    {
        string SceneName = PlayerData.myPlayerinfo.universityCode;
        
        foreach (string university in UniversityList)
        {
            if(SceneName == university)
            {
                SceneName = "world";
                break;
            }
        }

        return SceneName;
    }
    

    #endregion
    
    
    
}
