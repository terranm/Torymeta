
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Cinemachine;
using Colyseus;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SeminarSceneUIController : MonoBehaviour
{
    [SerializeField] private GameObject mediaPlayer;
    [SerializeField] private GameObject mediaPlayerPanel;
    
    [SerializeField] private Canvas _canvasTouchZone;
    [SerializeField] private Canvas _canvasJoystick;
    [SerializeField] private Canvas _canvasVideoScreen;
    //[SerializeField] private GameObject _objZoomOutButton;
    [SerializeField] private Image _imgtarget;
    [SerializeField] private Camera _camera;
    
    [SerializeField] private GameObject _objEmotion;
    [SerializeField] private GameObject _objEmotionSit;
    [SerializeField] private GameObject _objJump;
    [SerializeField] private GameObject _objSprintButton;

    [SerializeField] private GameObject[] _videoCanvasLeft = new GameObject[3];//0-대형, 1-중형, 2-소형
    [SerializeField] private GameObject[] _videoCanvasRight = new GameObject[3];//0-대형, 1-중형. 2-소형

    [SerializeField] private GameObject[] _objMute;
    [SerializeField] private GameObject[] _objUnmute;

    private Vector3 cameraOriginalPos;
    private Vector3 cameraOriginalRotation;
    private Vector3 webViewOriginalPos  = Vector3.zero;
    private Vector3 webViewOriginalScale = Vector3.zero;
    const float orthographicSize = 2.5f;

    [SerializeField] private AVProMediaPlayerController _mediaPlayerController;

    private void Awake()
    {
        //_canvasVideoScreen.gameObject.SetActive(false);
        _canvasVideoScreen.enabled = false;
        //mediaPlayer.SetActive(false);
        //mediaPlayerPanel.SetActive(false);
        mediaPlayer = GameObject.Find("MediaPlayer");
        mediaPlayerPanel = GameObject.Find("MediaPlayerPanel");
        _mediaPlayerController = mediaPlayer.GetComponent<AVProMediaPlayerController>();

        switch (PlayerData.myPlayerinfo.seminarType)
        {
            case "20":
                mediaPlayer.transform.localPosition = new Vector3(0.21f, 11.71f, 15.76f);
                mediaPlayer.transform.eulerAngles = new Vector3(90, 0, 180);
                mediaPlayerPanel.transform.localScale = new Vector3(2.8f, 1, 1.8f);
                _videoCanvasLeft[0].transform.parent.gameObject.SetActive(true);
                _videoCanvasRight[0].transform.parent.gameObject.SetActive(true);
                break;
            case "21":
                mediaPlayer.transform.localPosition = new Vector3(0.31f, 8.37f, 14.85f);
                mediaPlayer.transform.eulerAngles = new Vector3(90, 0, 180);
                mediaPlayerPanel.transform.localScale = new Vector3(2.2f, 1, 1.4f);
                _videoCanvasLeft[1].transform.parent.gameObject.SetActive(true);
                _videoCanvasRight[1].transform.parent.gameObject.SetActive(true);
                break;
            case "22":
                mediaPlayer.transform.localPosition = new Vector3(0.51f, 7.1f, 12.78f);
                mediaPlayer.transform.eulerAngles = new Vector3(90, 0, 180);
                mediaPlayerPanel.transform.localScale = new Vector3(1.4f, 1, 0.885f);
                _videoCanvasLeft[2].transform.parent.gameObject.SetActive(true);
                _videoCanvasRight[2].transform.parent.gameObject.SetActive(true);
                break;
        }
        
        NativeManager.Instance.SendUnityLifecycleState(NativeManager.LifeCycle.Awake);
    }

    private void Start()
    {
        GameEvents.Instance.OnRequestZooom += ZoomVideo;
        GameEvents.Instance.OnCompleteLoadScene += OnCompleteLoadScene;
        GameEvents.Instance.OnRequestSetVideoCanvas += SetCamCanvas;
        GameEvents.Instance.OnChangeUIBySit += ChangeUIBySit;
        GameEvents.Instance.OnClickMediaPlayerMuteBtn += MediaPlayerMute;
        NativeManager.Instance.SendUnityLifecycleState(NativeManager.LifeCycle.Start);
    }

    private void OnDestroy()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnRequestZooom -= ZoomVideo;
            GameEvents.Instance.OnCompleteLoadScene -= OnCompleteLoadScene;
            GameEvents.Instance.OnRequestSetVideoCanvas -= SetCamCanvas;
            GameEvents.Instance.OnChangeUIBySit -= ChangeUIBySit;
            GameEvents.Instance.OnClickMediaPlayerMuteBtn -= MediaPlayerMute;
        }
        if(NativeManager.Instance != null)NativeManager.Instance.SendUnityLifecycleState(NativeManager.LifeCycle.OnDestroy);
    }

    
    //=============================================Region Divider======================================
    
    #region Game Event handler Region

    private void SetCamCanvas(int memberId, string channelId)
    {
        int temp = (memberId == PlayerData.myPlayerinfo.memberId) ? 0 : memberId;
        switch (PlayerData.myPlayerinfo.seminarType)
        {
            case "20":
                if (_videoCanvasLeft[0].transform.childCount > 25)
                    ServerManager.Instance.StartVideoChat(temp,_videoCanvasRight[0],channelId);
                else
                    ServerManager.Instance.StartVideoChat(temp,_videoCanvasLeft[0],channelId);
                break;
            case "21":
                if (_videoCanvasLeft[1].transform.childCount > 16)
                    ServerManager.Instance.StartVideoChat(temp,_videoCanvasRight[1],channelId);
                else
                    ServerManager.Instance.StartVideoChat(temp,_videoCanvasLeft[1],channelId);
                break;
            case "22":
                if (_videoCanvasLeft[2].transform.childCount > 9)
                    ServerManager.Instance.StartVideoChat(temp,_videoCanvasRight[2],channelId);
                else
                    ServerManager.Instance.StartVideoChat(temp,_videoCanvasLeft[2],channelId);
                break;
        }
    }
    
    private void OnCompleteLoadScene()
    {
        ServerManager.Instance.InitializeVideoChat();
        StartCoroutine(_mediaPlayerController.WaitSeekSetting());
    }

    private void ZoomVideo(bool zoomIn)
    {
        //_imgtarget.raycastTarget = (!zoomIn);
        _canvasTouchZone.enabled = (!zoomIn);
        _canvasJoystick.enabled = (!zoomIn);
        _canvasVideoScreen.enabled = (zoomIn);
    }

    private void MediaPlayerMute()
    {
        //음소거 기능 수행 & 이미지 변환
        bool muteState = _mediaPlayerController.AudioMuteConvert();
        foreach (var obj in _objMute)
        {
            obj.SetActive(muteState);
        }

        foreach (var obj in _objUnmute)
        {
            obj.SetActive(!muteState);
        }
    }

    private float originPlayerCameraLocY = 1.72f; // 기본 값
   
    private void ChangeUIBySit(bool sit)
    {
        _objEmotion.SetActive(!sit);
        _objEmotionSit.SetActive(sit);
        _objJump.SetActive(!sit);
        _objSprintButton.SetActive(!sit);
        NetworkedEntityFactory.Instance.GetMine().transform.Find("PlayerCameraRoot").localPosition = new Vector3(0, sit ? 5 : originPlayerCameraLocY, 0);
    }
    
    #endregion
   

    //=============================================Region Divider======================================
    
    #region Script for test on UnityEditor Env region
#if UNITY_EDITOR
    
#endif
    #endregion    

}
