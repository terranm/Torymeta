using System;
using System.Collections;
using Colyseus;
using LucidSightTools;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyController : MonoBehaviour
{
    private IEnumerator Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        while (!MMOManager.IsReady)
        {
            yield return new WaitForEndOfFrame();
        }

        // Screen.orientation = ScreenOrientation.LandscapeLeft;
        
        ConnectToServer();
    }

    public void ConnectToServer()
    {
        try
        {
            ColyseusSettings clonedSettings = MMOManager.Instance.CloneSettings();
            clonedSettings.colyseusServerAddress = MMOManager.Instance.ColyseusServerAddress;
            clonedSettings.colyseusServerPort = MMOManager.Instance.ColyseusServerPort;
            clonedSettings.useSecureProtocol = MMOManager.Instance.ColyseusUseSecure;

            MMOManager.Instance.OverrideSettings(clonedSettings);
            MMOManager.Instance.InitializeClient();
        
            Debug.Log("Login Start"); //UnityEngine.Random.Range(0, 100).ToString()
            
            GetComponent<CreateUserMenu>().TryConnect(PlayerData.myPlayerinfo.universityCode,
                (error) => { Debug.LogError($"Login 에러 l {error} l"); });
        }
        catch (Exception e)
        {
            Debug.Log("ConnectToServer() - exception : "+ e.StackTrace);
        }
    }

    public void ConsumeSeatReservation(UserAuthResponse userAuthResponse)
    {
        if (userAuthResponse != null)
        {
            MMOManager.Instance.StartCoroutine(Co_LoadNextSceneThenJoinRoom(userAuthResponse,
                PlayerData.myPlayerinfo.universityCode, null));
        }
        else
        {
            LSLog.LogError($"Failed to convert response to UserAuthResponse!");
        }
    }

    private IEnumerator Co_LoadNextSceneThenJoinRoom(UserAuthResponse userAuthResponse, string scene, Action onComplete)
    {
        // Load the next scene
        yield return LoadSceneAsync(scene, onComplete);
    }

    private IEnumerator LoadSceneAsync(string scene, Action onComplete)
    {
        // Scene currScene = SceneManager.GetActiveScene();

        string SceneName = PlayerData.myPlayerinfo.universityCode;

        if (!SceneName.Equals("lobby") && !SceneName.Equals("seminar") && !SceneName.Equals("AvatarView") && !SceneName.Equals("SelectView"))
        // if (!SceneName.Equals("lobby") && !SceneName.Equals("AvatarView") && !SceneName.Equals("SelectView"))
            SceneName = "world";
        
        AsyncOperation op = SceneManager.LoadSceneAsync(SceneName);
        Debug.Log("LobbyController");
        
        while (op.progress <= 0.9f)
        {
            //Wait until the scene is loaded
            yield return new WaitForEndOfFrame();
        }

        op.allowSceneActivation = true;

        onComplete?.Invoke();
    }
}