using System.Collections;
using Cinemachine;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerData : MonoBehaviour
{ 
    [SerializeField]
    public GameObject mainCamera;

    private GameObject followCamera;
    private GameObject joystick;
    private GameObject touchZone;

    private StarterAssetsInputs starterAssetsInputs;
    private GameObject loadingUI;

    public struct MyplayerInfo
    {
        public string universityCode;
        public string userName;
        public string avatar;
        public string entityId;
        public int memberId;
        public string seminarUrl;
        public string videoStartTime;
        public string seminarId;
        public string seminarTitle;
        public string seminarType;
        public bool backSign;
        public bool isAppRelease;
        public string goToTableId;
        public int goToLoungeLocation;
        public string imgUrl;
        public AvatarState state;
    }
    
    public static MyplayerInfo myPlayerinfo = new();
    private NetworkedEntity myPlayer;

    public void Initialize()
    {
        // if (myPlayerinfo.universityCode != "Lobby" || myPlayerinfo.universityCode != "seminar")
        // if (myPlayerinfo.universityCode != "Lobby")
        //GetComponent<CharacterController>().enabled = false;

        while (followCamera == null)
        {
            new WaitForEndOfFrame();
            followCamera = GameObject.FindGameObjectWithTag("PlayerFollowCamera");
            joystick = GameObject.FindGameObjectWithTag("Joystick");
            touchZone = GameObject.FindGameObjectWithTag("TouchZone");
        }

        gameObject.SetActive(true);

        gameObject.layer = LayerMask.NameToLayer("Player");

        GetComponent<PlayerInput>().enabled = true;

        // GetComponent<ThirdPersonController>()._mainCamera = GalleryGameManager.Instance.mainCamera.gameObject;
        followCamera.GetComponent<CinemachineVirtualCamera>().Follow = transform.Find("PlayerCameraRoot").transform;
        joystick.GetComponent<UICanvasControllerInput>().starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        touchZone.GetComponent<UICanvasControllerInput>().starterAssetsInputs = GetComponent<StarterAssetsInputs>();

        transform.position = GetSpawnPosition(myPlayerinfo.universityCode);
        if (myPlayerinfo.universityCode.Equals("lobby"))
        {
            transform.rotation = Quaternion.Euler(0, -60, 0);
        }
    }

    public static Vector3 GetSpawnPosition(string value)
    {
        return value switch
        {
            "CAU" => new Vector3(-63f, 60, -111),
            "HONAU" => new Vector3(-414, 50, 180),
            "SMOONU" => new Vector3(-120, 50, 138f),
            "KKU" => new Vector3(511, 50, -360),
            //"lobby" => new Vector3(-35, 0, 30),
            //"seminar" => new Vector3(0, 5, 0),
            "lobby" => new Vector3(-35 + GetRandomValue(), 3, 30 + GetRandomValue()),
            "seminar" => new Vector3(0 + GetRandomValue(), 5, 0 + GetRandomValue()),
            _ => new Vector3(0, 50, 0)
        };
    }

    public static float GetRandomValue()
    {
        return (float)((new System.Random().NextDouble() * 10f) - 5f);
    }
}
