using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BasicSceneManager : Singleton<BasicSceneManager>
{
    private void Awake()
    {
        //currentUnivCode = "";
        //destUnivCode = "";
        GameEvents.Instance.OnRequestSceneChange += SwitchScene;
    }

    private void OnDestroy()
    {
        if(GameEvents.Instance != null)
            GameEvents.Instance.OnRequestSceneChange -= SwitchScene;
    }

    private void SwitchScene()
    {
        Debug.Log("CheckPoint : SwitchScene");
        
        if(!ServerManager.Instance.isConnected)
            ServerManager.Instance.ConnectToServer();
        
        if (PlayerData.myPlayerinfo.universityCode.Equals("SelectView") || PlayerData.myPlayerinfo.universityCode.Equals("AvatarView"))
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Screen.orientation = ScreenOrientation.Portrait;
#endif
            Screen.fullScreen = false;
        }
        else
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Screen.orientation = ScreenOrientation.LandscapeLeft;
#endif
            Screen.fullScreen = true;
        }
        DownloadManager.Instance.StartLoadingProcess();

        StartCoroutine(GetNewestMemberInfoToLoadScene());
        //CheckScene();
    }

    private IEnumerator GetNewestMemberInfoToLoadScene()
    {
#if !UNITY_EDITOR
        NativeManager.Instance.flagGetMemberInfo = true;
        Debug.Log("CheckPoint : SendRequestMemberInfo");
        NativeManager.Instance.SendRequestMemberInfo();
        int count = 0;
        while (NativeManager.Instance.flagGetMemberInfo)
        {
            yield return new WaitForUpdate();
            Debug.Log("wait for memberInfo");
            count++;
            if(count%5 == 0)
            {
                Debug.Log("Resend SendRequestMemberInfo");
                NativeManager.Instance.SendRequestMemberInfo();
            }
        }
#endif
        LoadScene();
        yield return null;
    }

    private void CheckScene()
    {
        Debug.Log("CheckPoint : CheckScene");
        if (!(PlayerData.myPlayerinfo.universityCode.Equals("SelectView") || PlayerData.myPlayerinfo.universityCode.Equals("AvatarView")))
        {
            if (string.IsNullOrEmpty(PlayerData.myPlayerinfo.avatar))
                PlayerData.myPlayerinfo.avatar = "100_11";
            if (PlayerData.myPlayerinfo.memberId.Equals(0))
                PlayerData.myPlayerinfo.memberId = 123;
            if (string.IsNullOrEmpty(PlayerData.myPlayerinfo.userName))
                PlayerData.myPlayerinfo.userName = "unityTest"+PlayerData.myPlayerinfo.memberId;
            StartCoroutine(TryConnetAfterLeave());
        }
    }

    private IEnumerator TryConnetAfterLeave()
    {
        Debug.Log("CheckPoint : TryConnetAfterLeave");
        while (ServerManager.Instance.Room != null || ServerManager.Instance.ChatLoungeRoom != null || AgoraManager.Instance.RtcEngine != null)
        {
            yield return new WaitForUpdate();
        }
        
        ServerManager.Instance.TryConnect(PlayerData.myPlayerinfo.universityCode, (error) => { Debug.LogError($"Login 에러 l {error} l"); });
        yield return null;
    }

    private void LoadScene()
    {
        Debug.Log("CheckPoint : LoadScene");
        NativeManager.Instance.SendRequestMemberInfo();
        StartCoroutine(LoadSceneAsync(CheckScene));
    }

    private IEnumerator LoadSceneAsync(Action onComplete = null)
    {
        Debug.Log("CheckPoint : LoadSceneAsync");
        // Scene currScene = SceneManager.GetActiveScene();

        string SceneName = GameEvents.Instance.CheckUniversityCode();

        //if (!SceneName.Equals("lobby") && !SceneName.Equals("seminar") && !SceneName.Equals("AvatarView") && !SceneName.Equals("SelectView"))
        //    // if (!SceneName.Equals("lobby") && !SceneName.Equals("AvatarView") && !SceneName.Equals("SelectView"))
        //    SceneName = "world";

        AsyncOperation op = SceneManager.LoadSceneAsync(SceneName);

        while (op.progress <= 0.9f)
        {
            //Wait until the scene is loaded
            yield return new WaitForEndOfFrame();
        }

        op.allowSceneActivation = true;
        
        while (!op.isDone)
        {
            yield return new WaitForEndOfFrame();
        }
        //currentUnivCode = PlayerData.myPlayerinfo.universityCode;

        if (onComplete != null)
            onComplete?.Invoke();
    }
    
    public IEnumerator WaitTillLeaveAndJoinInvitedChatRoom(string tableId)
    {
        while (ServerManager.Instance.ChatLoungeRoom != null 
               || AgoraManager.Instance.RtcEngine != null 
               || !ServerManager.Instance.Room.Id.Equals("lobby")  
               || !SceneManager.GetActiveScene().name.Equals("lobby"))
        {
            yield return new WaitForUpdate();
        }
        
        GameEvents.Instance.RequestFindAndJoinChat(tableId);

        yield return null;
    }
}
