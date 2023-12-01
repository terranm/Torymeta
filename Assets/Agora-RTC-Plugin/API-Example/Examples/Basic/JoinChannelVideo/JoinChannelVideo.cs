using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using Agora.Util;
using UnityEditor.Rendering;
using Logger = Agora.Util.Logger;

namespace Agora_RTC_Plugin.API_Example.Examples.Basic.JoinChannelVideo
{
    public class JoinChannelVideo : MonoBehaviour
    {
        [FormerlySerializedAs("appIdInput")]
        [SerializeField]
        private AppIdInput _appIdInput;

        [Header("_____________Basic Configuration_____________")]
        [FormerlySerializedAs("APP_ID")]
        [SerializeField]
        private string _appID = "";

        [FormerlySerializedAs("TOKEN")]
        [SerializeField]
        private string _token = "";

        [FormerlySerializedAs("CHANNEL_NAME")]
        [SerializeField]
        private string _channelName = "";

        public Text LogText;
        internal Logger Log;
        internal IRtcEngine RtcEngine = null;

        public Dropdown _videoDeviceSelect;
        private IVideoDeviceManager _videoDeviceManager;
        private DeviceInfo[] _videoDeviceInfos;

        // Use this for initialization
        private void Start()
        {
            if (PlayerData.myPlayerinfo.universityCode.Equals("seminar"))
            {
                if (RtcEngine == null)
                {
                    LoadAssetData(PlayerData.myPlayerinfo.seminarTitle);
                    if (CheckAppId())
                    {
                        InitEngine();
                        SetBasicConfiguration();
                    }
                }

                StartPreview();
                JoinChannel();
                SetMikeOff();
                SetVideoOff();
            }
        }

        public void StartChattingLounge(string roomId)
        {
            if (RtcEngine == null)
            {
                LoadAssetData(roomId);
                if (CheckAppId())
                {
                    InitEngine();
                    SetBasicConfiguration();
                }
            }

            StartPreview();
            JoinChannel(roomId);
        }
        
        // Update is called once per frame
        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();
        }

        //Show data in AgoraBasicProfile
        [ContextMenu("ShowAgoraBasicProfileData")]
        private void LoadAssetData(string roomId = "")
        {
            if (_appIdInput == null) return;
            _appID = _appIdInput.appID;
            _token = _appIdInput.token;
            _channelName = (roomId.Equals(""))?_appIdInput.channelName:roomId;
            if (!PlayerData.myPlayerinfo.isAppRelease)
                _channelName = "test-" + _channelName;
        }

        private bool CheckAppId()
        {
            Log = new Logger(LogText);
            return Log.DebugAssert(_appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
        }

        private void InitEngine()
        {
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(_appID, 0,
                                        CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                                        AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT, AREA_CODE.AREA_CODE_GLOB, new LogConfig("./log.txt"));
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);
        }

        private void SetBasicConfiguration()
        {
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            VideoEncoderConfiguration config = new VideoEncoderConfiguration
            {
                dimensions = new VideoDimensions(640, 360),
                frameRate = 15,
                bitrate = 0
            };
            RtcEngine.SetVideoEncoderConfiguration(config);
            RtcEngine.SetChannelProfile(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_COMMUNICATION);
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        }

        #region -- Button Events ---

        public void JoinChannel(string roomId="")
        {
            _channelName = (roomId.Equals(""))?PlayerData.myPlayerinfo.seminarId:roomId;
            RtcEngine.JoinChannel(_token, _channelName);
                MakeVideoView(0);
        }

        public void LeaveChannel()
        {
            RtcEngine.LeaveChannel();
        }
        
        public void RemoveVideo(uint uid)
        {
            DestroyVideoView(uid);
        }

        public void StartPreview()
        {
            RtcEngine.StartPreview();
            MakeVideoView(0);
        }

        public void StopPreview()
        {
            DestroyVideoView(0);
            RtcEngine.StopPreview();
        }

        public void StartPublish()
        {
            var options = new ChannelMediaOptions();
            options.publishMicrophoneTrack.SetValue(true);
            options.publishCameraTrack.SetValue(true);
            var nRet = RtcEngine.UpdateChannelMediaOptions(options);
            this.Log.UpdateLog("UpdateChannelMediaOptions: " + nRet);
        }

        public void StopPublish()
        {
            var options = new ChannelMediaOptions();
            options.publishMicrophoneTrack.SetValue(false);
            options.publishCameraTrack.SetValue(false);
            var nRet = RtcEngine.UpdateChannelMediaOptions(options);
            this.Log.UpdateLog("UpdateChannelMediaOptions: " + nRet);
        }
        
        public void SetMikeOn()
        {
            RtcEngine.EnableLocalAudio(true);
        }
        
        public void SetMikeOff()
        {
            RtcEngine.EnableLocalAudio(false);
        }
        
        public void SetVideoOn()
        {
            var options = new ChannelMediaOptions();
            options.publishCameraTrack.SetValue(true);
            var nRet = RtcEngine.UpdateChannelMediaOptions(options);
            this.Log.UpdateLog("UpdateChannelMediaOptions: " + nRet);
            RtcEngine.StartPreview();
            MakeVideoView(0);
        }
        
        public void SetVideoOff()
        {
            var options = new ChannelMediaOptions();
            options.publishCameraTrack.SetValue(false);
            var nRet = RtcEngine.UpdateChannelMediaOptions(options);
            this.Log.UpdateLog("UpdateChannelMediaOptions: " + nRet);
            DestroyVideoView(0);
            RtcEngine.StopPreview();
        }

        public void AdjustVideoEncodedConfiguration640()
        {
            VideoEncoderConfiguration config = new VideoEncoderConfiguration();
            config.dimensions = new VideoDimensions(640, 360);
            config.frameRate = 15;
            config.bitrate = 0;
            RtcEngine.SetVideoEncoderConfiguration(config);
        }

        public void AdjustVideoEncodedConfiguration480()
        {
            VideoEncoderConfiguration config = new VideoEncoderConfiguration();
            config.dimensions = new VideoDimensions(480, 480);
            config.frameRate = 15;
            config.bitrate = 0;
            RtcEngine.SetVideoEncoderConfiguration(config);
        }

        public void GetVideoDeviceManager()
        {
            _videoDeviceSelect.ClearOptions();

            _videoDeviceManager = RtcEngine.GetVideoDeviceManager();
            _videoDeviceInfos = _videoDeviceManager.EnumerateVideoDevices();
            Log.UpdateLog(string.Format("VideoDeviceManager count: {0}", _videoDeviceInfos.Length));
            for (var i = 0; i < _videoDeviceInfos.Length; i++)
            {
                Log.UpdateLog(string.Format("VideoDeviceManager device index: {0}, name: {1}, id: {2}", i,
                    _videoDeviceInfos[i].deviceName, _videoDeviceInfos[i].deviceId));
            }

            _videoDeviceSelect.AddOptions(_videoDeviceInfos.Select(w =>
                    new Dropdown.OptionData(
                        string.Format("{0} :{1}", w.deviceName, w.deviceId)))
                .ToList());
        }

        public void SelectVideoCaptureDevice()
        {
            if (_videoDeviceSelect == null) return;
            var option = _videoDeviceSelect.options[_videoDeviceSelect.value].text;
            if (string.IsNullOrEmpty(option)) return;

            var deviceId = option.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
            var ret = _videoDeviceManager.SetDevice(deviceId);
            Log.UpdateLog("SelectVideoCaptureDevice ret:" + ret + " , DeviceId: " + deviceId);
        }

        #endregion

        private void OnDestroy()
        {
            Debug.Log("Agora OnDestroy");
            if (RtcEngine == null) return;
            RtcEngine.InitEventHandler(null);
            RtcEngine.LeaveChannel();
            RtcEngine.Dispose();
        }

        internal string GetChannelName()
        {
            return _channelName;
        }

        #region -- Video Render UI Logic ---

        internal static void MakeVideoView(uint uid, string channelId = "")
        {
            var go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                return; // reuse
            }

            // create a GameObject and assign to this new user
            var videoSurface = MakeImageSurface(uid.ToString());
            if (ReferenceEquals(videoSurface, null)) return;
            // configure videoSurface
            if (uid == 0)
            {
                videoSurface.SetForUser(uid, channelId);
            }
            else
            {
                videoSurface.SetForUser(uid, channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
            }

            videoSurface.OnTextureSizeModify += (int width, int height) =>
            {
                float scale = 1;//(float)height / (float)width;
                videoSurface.transform.localScale = new Vector3(1, 1 * scale, 1);
                Debug.Log("OnTextureSizeModify: " + width + "  " + height);
                // videoSurface.transform.rotation = Quaternion.Euler(0,22,180);
            };

            videoSurface.SetEnable(true);
        }

        // VIDEO TYPE 1: 3D Object
        private static VideoSurface MakePlaneSurface(string goName)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);

            if (go == null)
            {
                return null;
            }

            go.name = goName;
            // set up transform
            go.transform.Rotate(-90.0f, 0.0f, 0.0f);
            var yPos = UnityEngine.Random.Range(3.0f, 5.0f);
            var xPos = UnityEngine.Random.Range(-2.0f, 2.0f);
            go.transform.position = Vector3.zero;
            go.transform.localScale = new Vector3(0.25f, 0.5f, 0.5f);

            // configure videoSurface
            var videoSurface = go.AddComponent<VideoSurface>();
            return videoSurface;
        }

        // Video TYPE 2: RawImage
        private static VideoSurface MakeImageSurface(string goName,string channelId="")
        {
            GameObject go = new GameObject();

            if (go == null)
            {
                return null;
            }

            go.name = goName;
            // to be renderered onto
            go.AddComponent<RawImage>();
            // make the object draggable
            go.AddComponent<UIElementDrag>();
            GameObject canvas;

            if (!PlayerData.myPlayerinfo.universityCode.Equals("seminar"))
            {
                //canvas = GameObject.Find("NativeCallObject").GetComponent<NativeCall_Lobby>().SetChatLoungePanelReady(MMOManager.Instance.ClickedChattingLoungeOBJ);
                // if (canvas != null)
                // {
                //     go.transform.SetParent(canvas.transform, true);
                //     Debug.Log("add video view");
                // }
                // else
                // {
                //     Debug.Log("Canvas is null video view");
                // }
            }
            else
            {
                switch (PlayerData.myPlayerinfo.seminarType)
                {
                    case "20":
                        //key = "SeminarL5"; 
                        //GameObject.Find("Canvas_Video_l").SetActive(true);
                        //GameObject.Find("Canvas_Video_m").SetActive(false);
                        //GameObject.Find("Canvas_Video_s").SetActive(false);
                        canvas = GameObject.Find("Canvas_Video_l/LeftPanel");
                        if (canvas.transform.childCount > 24)
                            canvas = GameObject.Find("Canvas_Video_l/RightPanel");
                        if (canvas != null)
                        {
                            go.transform.SetParent(canvas.transform, true);
                            Debug.Log("add video view");
                        }
                        else
                        {
                            Debug.Log("Canvas is null video view");
                        }
                        break;
                    case "21":
                        //key = "SeminarM5"; 
                        //GameObject.Find("Canvas_Video_l").SetActive(false);
                        //GameObject.Find("Canvas_Video_m").SetActive(false);
                        //GameObject.Find("Canvas_Video_s").SetActive(false);
                        canvas = GameObject.Find("Canvas_Video_m/LeftPanel");
                        if (canvas.transform.childCount > 24)
                            canvas = GameObject.Find("Canvas_Video_m/RightPanel");
                        if (canvas != null)
                        {
                            go.transform.SetParent(canvas.transform, true);
                            Debug.Log("add video view");
                        }
                        else
                        {
                            Debug.Log("Canvas is null video view");
                        }
                        break;
                    case "22":
                        //key = "SeminarS5"; 
                        //GameObject.Find("Canvas_Video_l").SetActive(false);
                        //GameObject.Find("Canvas_Video_m").SetActive(false);
                        //GameObject.Find("Canvas_Video_s").SetActive(false);
                        canvas = GameObject.Find("Canvas_Video_s/LeftPanel");
                        if (canvas.transform.childCount > 24)
                            canvas = GameObject.Find("Canvas_Video_s/RightPanel");
                        if (canvas != null)
                        {
                            go.transform.SetParent(canvas.transform, true);
                            Debug.Log("add video view");
                        }
                        else
                        {
                            Debug.Log("Canvas is null video view");
                        }
                        break;
                    default:
                        GameObject.Find("Canvas_Video_l").SetActive(true);
                        canvas = GameObject.Find("Canvas_Video_l/LeftPanel");
                        if (canvas.transform.childCount > 24)
                            canvas = GameObject.Find("Canvas_Video_l/RightPanel");
                        if (canvas != null)
                        {
                            go.transform.SetParent(canvas.transform, true);
                            Debug.Log("add video view");
                        }
                        else
                        {
                            Debug.Log("Canvas is null video view");
                        }
                        break;
                }
            }

            // set up transform
            //go.transform.rotation = Quaternion.Euler(0, canvas.transform.eulerAngles.y, 180);
            go.transform.localPosition = Vector3.zero;
            // configure videoSurface
            var videoSurface = go.AddComponent<VideoSurface>();
            return videoSurface;
        }

        internal static void DestroyVideoView(uint uid)
        {
            var go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                Destroy(go);
            }
        }

        # endregion
    }

    #region -- Agora Event ---

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly JoinChannelVideo _videoSample;

        internal UserEventHandler(JoinChannelVideo videoSample)
        {
            _videoSample = videoSample;
        }

        public override void OnError(int err, string msg)
        {
            _videoSample.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            Debug.Log("Agora: OnJoinChannelSuccess ");
            _videoSample.Log.UpdateLog(string.Format("sdk version: ${0}",
                _videoSample.RtcEngine.GetVersion(ref build)));
            _videoSample.Log.UpdateLog(string.Format("sdk build: ${0}",
              build));
            _videoSample.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                connection.channelId, connection.localUid, elapsed));
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _videoSample.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _videoSample.Log.UpdateLog("OnLeaveChannel");
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
        {
            //_videoSample.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnRemoteVideoStateChanged(RtcConnection connection, uint remoteUid, REMOTE_VIDEO_STATE state,
            REMOTE_VIDEO_STATE_REASON reason, int elapsed)
        {
            base.OnRemoteVideoStateChanged(connection, remoteUid, state, reason, elapsed);
            if(state == REMOTE_VIDEO_STATE.REMOTE_VIDEO_STATE_STOPPED)
                JoinChannelVideo.DestroyVideoView(remoteUid);
            if(state == REMOTE_VIDEO_STATE.REMOTE_VIDEO_STATE_STARTING)
                JoinChannelVideo.MakeVideoView(remoteUid, _videoSample.GetChannelName());
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _videoSample.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            JoinChannelVideo.MakeVideoView(uid, _videoSample.GetChannelName());
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _videoSample.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            JoinChannelVideo.DestroyVideoView(uid);
        }

        public override void OnUplinkNetworkInfoUpdated(UplinkNetworkInfo info)
        {
            _videoSample.Log.UpdateLog("OnUplinkNetworkInfoUpdated");
        }

        public override void OnDownlinkNetworkInfoUpdated(DownlinkNetworkInfo info)
        {
            _videoSample.Log.UpdateLog("OnDownlinkNetworkInfoUpdated");
        }
    }

    # endregion
}