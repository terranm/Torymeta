using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using StarterAssets;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    [SerializeField] private GameObject _camMinimap;
    [SerializeField] private GameObject _objFixedMinimap;
    [SerializeField] private GameObject _objMapCanvas;

    [SerializeField] private Button _btnToMuseum;
    [SerializeField] private Button _btnToChatLounge;
    [SerializeField] private Button _btnToLounge;
    [SerializeField] private Button _btnToMaze;
    [SerializeField] private Button _btnToSeminarLobby;
    [SerializeField] private Button _btnToStage;

    [SerializeField] private GameObject _objTeleportLocationList;
    [SerializeField] private Transform[] _teleportLocList;
    public Transform[] TransportList
    {
        get { return _teleportLocList; }
    }

    public enum TranportLocationPoint
    {
        none, // 기본 위치 
        museumOut,
        museumIn,
        seminarLobbyOut,
        seminarLobbyIn,
        chatLounge,
        lounge,
        maze,
        stage
    }

    [SerializeField] private Camera _camera;

    private GameObject player;

    public bool isOnCafe1 = false;
    public bool isOnCafe2 = false;
    public bool isOnCafe3 = false;
    public bool isOnMuseum= false;
    public bool isOnSeminarLobby= false;

    public void Init()
    {
        player = NetworkedEntityFactory.Instance.GetMine().gameObject;
        GameEvents.Instance.OnRequestMapActiveConvert += MapActiveConvert;
        GameEvents.Instance.OnRequestLookAvatarForward += LookAvatarForward;
        GameEvents.Instance.OnRequestFixedMapActiveConvert += FixedMapActiveConvert;
        if (PlayerData.myPlayerinfo.universityCode == "lobby")
            AddTeleportBtnListener();
    }

    private void OnDestroy()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnRequestMapActiveConvert -= MapActiveConvert;
            GameEvents.Instance.OnRequestLookAvatarForward -= LookAvatarForward;
        }
    }

    private void LookAvatarForward()
    {
        if (!string.IsNullOrEmpty(PlayerData.myPlayerinfo.goToTableId))
            StartCoroutine(WaitUntilChatJoin());
        else
            StartCoroutine(AdjustingCamRotation());
    }

    private IEnumerator WaitUntilChatJoin()
    {
        while (ServerManager.Instance.ChatLoungeRoom == null)
        {
            yield return new WaitForUpdate();
        }
        PlayerData.myPlayerinfo.goToTableId = "";
        yield return null;
    }
    
    private IEnumerator AdjustingCamRotation()
    {
        StarterAssetsInputs assetsInputs =  player.GetComponent<StarterAssetsInputs>();
        int maxCnt = 0;
        
        float diff = GetCalculatableValue(_camera.transform.rotation.eulerAngles.y) - GetCalculatableValue(player.transform.rotation.eulerAngles.y);
        while (Mathf.Abs(diff) > 5f)
        {
#if UNITY_EDITOR || UNITY_ANDROID  
            assetsInputs.LookInput(new Vector2(-diff/10f,0));
#elif UNITY_IOS
            assetsInputs.LookInput(new Vector2(-diff*2f,0));
#endif
            diff = GetCalculatableValue(_camera.transform.rotation.eulerAngles.y) - GetCalculatableValue(player.transform.rotation.eulerAngles.y);
            
            if (++maxCnt > 1000) break;
            yield return new WaitForFixedUpdate();
        }
        assetsInputs.LookInput(new Vector2(0,0));
        yield return null;
    }

    private float GetCalculatableValue(float deg)
    {
        return (deg < 0) ? deg + 360 : deg;
    }

    private void MapActiveConvert()
    {
        _objFixedMinimap.SetActive(!_objFixedMinimap.activeSelf);
        _objMapCanvas.SetActive(!_objMapCanvas.activeSelf);
    }

    private void FixedMapActiveConvert()
    {
        _objFixedMinimap.SetActive(!_objFixedMinimap.activeSelf);
    }
    
    private void AddTeleportBtnListener()
    {
        _teleportLocList = _objTeleportLocationList.GetComponentsInChildren<Transform>();
        _btnToMuseum.onClick.AddListener(delegate { MoveTo(_teleportLocList[(int)TranportLocationPoint.museumOut].position, _teleportLocList[(int)TranportLocationPoint.museumOut].rotation.eulerAngles); });
        _btnToChatLounge.onClick.AddListener(delegate { MoveTo(_teleportLocList[(int)TranportLocationPoint.chatLounge].position, _teleportLocList[(int)TranportLocationPoint.chatLounge].rotation.eulerAngles); });
        _btnToLounge.onClick.AddListener(delegate { MoveTo(_teleportLocList[(int)TranportLocationPoint.lounge].position, _teleportLocList[(int)TranportLocationPoint.lounge].rotation.eulerAngles); });
        _btnToMaze.onClick.AddListener(delegate { MoveTo(_teleportLocList[(int)TranportLocationPoint.maze].position, _teleportLocList[(int)TranportLocationPoint.maze].rotation.eulerAngles); });
        _btnToSeminarLobby.onClick.AddListener(delegate { MoveTo(_teleportLocList[(int)TranportLocationPoint.seminarLobbyOut].position, _teleportLocList[(int)TranportLocationPoint.seminarLobbyOut].rotation.eulerAngles); });
        _btnToStage.onClick.AddListener(delegate { MoveTo(_teleportLocList[(int)TranportLocationPoint.stage].position, _teleportLocList[(int)TranportLocationPoint.stage].rotation.eulerAngles); });
    }

    public void MoveTo(Vector3 pos, Vector3 rot)
    {
        player.GetComponent<NetworkedEntity>().isTeleporting = true;
        player.GetComponent<UnityEngine.InputSystem.PlayerInput>().enabled = false;
        player.GetComponent<NetworkedEntity>().Vector3TelePos = pos;
        player.GetComponent<NetworkedEntity>().Vector3TeleRot = rot;
        player.GetComponent<StarterAssetsInputs>().enabled = false;
    }

    private void LateUpdate()
    {
        if (player != null)
        {
            if (PlayerData.myPlayerinfo.universityCode == "lobby")
            {
                if (!isOnCafe1 && !isOnCafe2 && !isOnCafe3 && !isOnSeminarLobby && !isOnMuseum)
                {
                    _camMinimap.transform.position =
                        new Vector3(player.transform.position.x, 1000f, player.transform.position.z);
                }
            }
            else
            {
                _camMinimap.transform.position =
                    new Vector3(player.transform.position.x, 1000f, player.transform.position.z);
            }
        }

        
    }

}
