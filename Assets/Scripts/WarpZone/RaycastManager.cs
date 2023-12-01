using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agora_RTC_Plugin.API_Example.Examples.Basic.JoinChannelVideo;
using Cinemachine;
using Colyseus;
using JetBrains.Annotations;
using StarterAssets;
using UniRx;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem.HID;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RaycastManager : MonoBehaviour
{
    [SerializeField] private Text bytes;
    
    //private NetworkedEntity myPlayer;
    //private Animator anim;
    private RaycastHit hit;
    private Ray ray;
    private GameObject hitObject;
    private string campusCode;
    private readonly long megaByte = 1048576;
    private IObservable<long> touchObservable;

   
    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out hit);
            if (hit.collider == null) return;
            
            hitObject = hit.collider.gameObject;
            ChangeWorld();
            SitOnChair();
            ToggleFullScreen();
            ChatLoungeCreate();
            ChatLoungeJoin();
            ChatLoungeParticipant();
            MediaPlayerMute();
            GameEvents.Instance.ClickTeleportObj(hitObject);
        }
    }
    


    public void ZoomOut()
    {
        GameEvents.Instance.RequestZooom(false);
    }
    
    
    private void ToggleFullScreen()
    {
        if(!hitObject.layer.Equals(LayerMask.NameToLayer("ToggleFullScreen"))) return;
        
        GameEvents.Instance.RequestZooom(true);
    }

    


    /* 건물 클릭으로 월드 입장 */
    private void ChangeWorld()
    {
        if (!hitObject.layer.Equals(LayerMask.NameToLayer("World"))) return;
        
            PlayerData.myPlayerinfo.universityCode = hit.collider.name;

            // bytes.text = Mathf.Round(Addressables.GetDownloadSizeAsync("Cube").Result / megaByte) + " MB";
            bytes.text = Mathf.Round(Addressables.GetDownloadSizeAsync(PlayerData.myPlayerinfo.universityCode).Result /
                                     megaByte) + " MB";
            Debug.LogError("size : " + bytes.text);

        
    }

    /* 의자 클릭으로 앉기 */
    private void SitOnChair()
    {
        Debug.Log(hit.collider.name + " , " + hit.collider.gameObject.layer);
        
        if (!hitObject.layer.Equals(LayerMask.NameToLayer("Chair"))) return;
        
        NetworkedEntityFactory.Instance.MakePlayerSeat(hitObject);
    }

    private void ChatLoungeCreate()
    {
        if (!hitObject.layer.Equals(LayerMask.NameToLayer("ChatCreate"))) return;
        if (!NetworkedEntityFactory.Instance.GetMine().Table.Equals("0"))return;
        
        hitObject.transform.parent.gameObject.GetComponent<ChatLoungeController>().CreateChat();
    }
    
    private void ChatLoungeJoin()
    {
        if (!hitObject.layer.Equals(LayerMask.NameToLayer("ChatJoin"))) return;
        if (!NetworkedEntityFactory.Instance.GetMine().Table.Equals("0"))return;
        
        hitObject.transform.parent.parent.gameObject.GetComponent<ChatLoungeController>().JoinChat();
    }

    private void ChatLoungeParticipant()
    {
        if (!hitObject.layer.Equals(LayerMask.NameToLayer("ChatParticipantPanel"))) return;

        NativeManager.Instance.SendChatLoungeParticipantsMenuMessage(hitObject);
    }
    
    
    private void MediaPlayerMute()
    {
        if (!hitObject.layer.Equals(LayerMask.NameToLayer("ToggleMediaPlayerMute"))) return;

        CallMediaPlayerMuteGameEvent();
    }

    public void CallMediaPlayerMuteGameEvent()
    {
        GameEvents.Instance.ClickMediaPlayerMuteBtn();
    }

    public void HelloBtn()
    {
        NetworkedEntityFactory.Instance.MakePlayerAction_Trigger("HelloTrigger");
    }

    public void FightingBtn()
    {
        NetworkedEntityFactory.Instance.MakePlayerAction_Trigger("FightingTrigger");
    }

    public void ItsMeBtn()
    {
        NetworkedEntityFactory.Instance.MakePlayerAction("isItsme");
    }

   /* 
    private void OnClickChatLoungeOBJ()
    {
        if (!hitObject.layer.Equals(LayerMask.NameToLayer("ChattingLounge"))) return;
        if(!ServerManager.Instance.ChatLoungeProcessDone) return;
        
        GameObject objTable = hitObject;
        bool top = false;
        while (!top)
        {
            if (objTable.transform.parent.gameObject.layer.Equals(LayerMask.NameToLayer("ChattingLounge")))
            {
                objTable = objTable.transform.parent.gameObject;
            }
            else
                top = true;
        }
        GameEvents.Instance.ClickChatLounge(objTable);
    }
 */

    #region Function

    

    #endregion
}