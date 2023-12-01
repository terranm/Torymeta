
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Colyseus;
using Newtonsoft.Json;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WorldSceneUIController : MonoBehaviour
{
    [SerializeField] private Canvas _canvasTouchZone;
    [SerializeField] private Canvas _canvasJoystick;
    
    [SerializeField] private GameObject _objJumpButton;

    [SerializeField] private GameObject _objEmotion;
    [SerializeField] private GameObject _objEmotionSit;

    [SerializeField] private MinimapController _minimapController;

    class ChatLoungeform
    {
        public GameObject table;
        public ChatLoungeController Controller;
    }
    
    private List<ChatLoungeform> ChatLoungeformList;
    private List<RoomInfo> CurrentRoomList;

    private int warningCount;
    
    private void Awake()
    {
        NativeManager.Instance.SendUnityLifecycleState(NativeManager.LifeCycle.Awake);
    }

    private void Start()
    {
        GameEvents.Instance.OnCompleteLoadScene += OnCompleteLoadScene;
        GameEvents.Instance.OnChangeUIBySit += ChangeUIBySit;
        GameEvents.Instance.OnClickTeleportObj += Teleport;

        CurrentRoomList = new List<RoomInfo>();
        ServerManager.Instance.ChatLoungeProcessDone = true;
        warningCount = 0;
        
        NativeManager.Instance.SendUnityLifecycleState(NativeManager.LifeCycle.Start);
    }

    private void OnDestroy()
    {
        ServerManager.Instance.ChatLoungeProcessDone = true;
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnCompleteLoadScene -= OnCompleteLoadScene;
            GameEvents.Instance.OnChangeUIBySit -= ChangeUIBySit;
            GameEvents.Instance.OnClickTeleportObj -= Teleport;
        }
        
        if(NativeManager.Instance != null)NativeManager.Instance.SendUnityLifecycleState(NativeManager.LifeCycle.OnDestroy);
    }


    //=============================================Region Divider======================================

    #region Game Event handler Region

    // 텔레포트 기능 구현 - 230713 유정균 // 텔레포트 관련 하여 캐릭터 이동 관련 기능을 끄고 이동시켜야하는 상황이므로 아바타 관련 코드가 정리된 후 진행필요 되도록 NetworkedEntityFactory에서 진행해야함 230802 유정균
    private void Teleport(GameObject hitObject)
    {
        if (!hitObject.layer.Equals(LayerMask.NameToLayer("Teleport"))) return;
        Transform targetTr = hitObject.transform.GetChild(0);
        _minimapController.MoveTo(targetTr.position,targetTr.rotation.eulerAngles);
    }
    
    private void OnCompleteLoadScene()
    {
        _minimapController.Init();
    }
    
    private void ChangeUIBySit(bool sit)
    {
        _objEmotion.SetActive(!sit);
        _objEmotionSit.SetActive(sit);
        _objJumpButton.SetActive(!sit);
    }
    
    #endregion
   

    //=============================================Region Divider======================================
    
    #region Script for test on UnityEditor Env region
    
#if UNITY_EDITOR
#endif
    
    #endregion    
    
    //=============================================Region Divider======================================

    #region internal functions

    

    #endregion

}
