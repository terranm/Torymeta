using System;
using System.Linq;
using Agora_RTC_Plugin.API_Example.Examples.Basic.JoinChannelVideo;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using Agora.Util;

    public class AgoraManager : Singleton<AgoraManager>
    {
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
        
        
        internal IRtcEngine RtcEngine = null;

        public Dropdown _videoDeviceSelect;
        private IVideoDeviceManager _videoDeviceManager;
        private DeviceInfo[] _videoDeviceInfos;
        public GameObject TargetCanvas;
        

        private void Start()
        {
            TargetCanvas = null;
        }

        public void SetVideoCanvas(GameObject target)
        {
            TargetCanvas = target;
            Debug.Log("Target canvas Set : " + TargetCanvas.transform.parent.name + "/" + TargetCanvas.name);
        }
        
        // Use this for initialization
        public void StartVideoChat(string roomId = "")
        {
            LoadAssetData(roomId);
            InitEngine();

            //StartPreview();
            JoinChannel();
            if (roomId.Equals(""))
            {
                SetMikeOff();
                SetVideoOff();
            }
        }

        //Show data in AgoraBasicProfile
        [ContextMenu("ShowAgoraBasicProfileData")]
        private void LoadAssetData(string roomId)
        {
            //_appID = PlayerData.myPlayerinfo.isAppRelease ?
            //    "e178155d16cb403ea26c852da700984e" : // 실 서버용 
            //    "e415784e2d7d45ffb0d48068a263f2fc"; // 테스트 서버용
            _appID = "e178155d16cb403ea26c852da700984e";
            _token = "";
            _channelName = (roomId.Equals(""))?PlayerData.myPlayerinfo.seminarTitle:roomId;
            if (!PlayerData.myPlayerinfo.isAppRelease)
                _channelName = "test-" + _channelName;
        }


        private void InitEngine()
        {
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngineEx();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(_appID, 0,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(new UserEventHandler(this));
        }

        #region -- Button Events ---

        public void JoinChannel()
        {
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            
            ChannelMediaOptions options = new ChannelMediaOptions();
            options.autoSubscribeAudio.SetValue(true);
            options.autoSubscribeVideo.SetValue(true);

            options.publishCameraTrack.SetValue(true);
            options.publishScreenTrack.SetValue(false);
            options.enableAudioRecordingOrPlayout.SetValue(true);
            options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            Debug.Log("StartEchoTest : " + RtcEngine.StartEchoTest());
            Debug.Log("StopEchoTest : " + RtcEngine.StopEchoTest());
            Debug.Log("JoinChannel : " + _channelName);
            Debug.Log("JoinChannel return value : " + 
                      RtcEngine.JoinChannel(_token, _channelName,(uint)PlayerData.myPlayerinfo.memberId,options));
            //MakeVideoView(0,_channelName);
        }

        public void LeaveChannel()
        {
            if (RtcEngine == null) return;
            RtcEngine.InitEventHandler(null);
            Debug.Log("LeaveChannel return value : " + RtcEngine.LeaveChannel());
            RtcEngine.Dispose();
            RtcEngine = null;
        }

        public void StartPublish()
        {
            var options = new ChannelMediaOptions();
            options.publishMicrophoneTrack.SetValue(true);
            options.publishCameraTrack.SetValue(true);
            var nRet = RtcEngine.UpdateChannelMediaOptions(options);
        }

        public void StopPublish()
        {
            var options = new ChannelMediaOptions();
            options.publishMicrophoneTrack.SetValue(false);
            options.publishCameraTrack.SetValue(false);
            var nRet = RtcEngine.UpdateChannelMediaOptions(options);
        }
        
        public void SetMikeOn()
        {
            RtcEngine.EnableLocalAudio(true);
            if (!_channelName.Equals(PlayerData.myPlayerinfo.seminarTitle) &&
                !_channelName.Equals("test-" + PlayerData.myPlayerinfo.seminarTitle))
            {
                NetworkedEntityFactory.Instance.SetEntityMicOff(PlayerData.myPlayerinfo.memberId, false);
            }
        }
        
        public void SetMikeOff()
        {
            RtcEngine.EnableLocalAudio(false);
            if (!_channelName.Equals(PlayerData.myPlayerinfo.seminarTitle) &&
                !_channelName.Equals("test-" + PlayerData.myPlayerinfo.seminarTitle))
            {
                NetworkedEntityFactory.Instance.SetEntityMicOff(PlayerData.myPlayerinfo.memberId, true);
            }
        }
        
        public void SetVideoOn()
        {
            var options = new ChannelMediaOptions();
            options.publishCameraTrack.SetValue(true);
            var nRet = RtcEngine.UpdateChannelMediaOptions(options);
            RtcEngine.StartPreview();
            UserCamOff((uint)PlayerData.myPlayerinfo.memberId,false);
        }
        
        public void SetVideoOff()
        {
            var options = new ChannelMediaOptions();
            options.publishCameraTrack.SetValue(false);
            var nRet = RtcEngine.UpdateChannelMediaOptions(options);
            if (!_channelName.Equals(PlayerData.myPlayerinfo.seminarTitle) &&
                !_channelName.Equals("test-" + PlayerData.myPlayerinfo.seminarTitle))
            {
                UserCamOff((uint)PlayerData.myPlayerinfo.memberId, true);
            }
            else
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

        internal void MakeVideoView(uint uid, string channelId = "")
        {
            var go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null)) return; // reusen;
            
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

            videoSurface.SetEnable(true);
            
            videoSurface.OnTextureSizeModify += (int width, int height) =>
            {
                float scale = 1;//(float)height / (float)width;
                videoSurface.transform.localScale = new Vector3(1, 1 * scale, 1);
                Debug.Log("OnTextureSizeModify: " + width + "  " + height);
            };
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
        private VideoSurface MakeImageSurface(string goName)
        {
            GameObject go = new GameObject();

            if (go == null)
            {
                return null;
            }

            go.name = goName;
            // to be renderered onto
            go.AddComponent<RawImage>();
            
            go.AddComponent<UIElementDrag>();
            if (TargetCanvas != null)
            {
                Debug.Log("Target canvas : " + TargetCanvas.transform.parent.name + "/" + TargetCanvas.name);
                go.transform.SetParent(TargetCanvas.transform,true);
                Debug.Log("add video view");
            }
            else
            {
                Debug.Log("Canvas is null video view");
            }

            // set up transform
            go.transform.rotation = Quaternion.Euler(0, TargetCanvas.transform.eulerAngles.y, 180);
            go.transform.localPosition = Vector3.zero;
            
            TargetCanvas = null;
            // configure videoSurface
            var videoSurface = go.AddComponent<VideoSurface>();
            return videoSurface;
        }

        internal void DestroyVideoView(uint uid)
        {
            var go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                if (!_channelName.Equals(PlayerData.myPlayerinfo.seminarTitle)&&
                    !_channelName.Equals("test-" + PlayerData.myPlayerinfo.seminarTitle))
                { 
                    NetworkedEntityFactory.Instance.SetEntityCamOff((int)uid,true,true);
                }
                Destroy(go);
            }
        }
        
        internal void UserCamOff(uint uid, bool mute)
        {
            if (!_channelName.Equals(PlayerData.myPlayerinfo.seminarTitle)&&
                !_channelName.Equals("test-" + PlayerData.myPlayerinfo.seminarTitle))
            {
                NetworkedEntityFactory.Instance.SetEntityCamOff((int)uid,mute);
            }
            
            if (mute)
            {
                var go = GameObject.Find(uid.ToString());
                if (!ReferenceEquals(go, null))
                {
                    Destroy(go);
                }
            }
            else
                GameEvents.Instance.RequestSetVideoCanvas((int)uid, _channelName);
        }
        
        # endregion
    }

    #region -- Agora Event ---

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly AgoraManager _videoSample;

        internal UserEventHandler(AgoraManager videoSample)
        {
            _videoSample = videoSample;
        }

        public override void OnError(int err, string msg)
        {
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            Debug.Log("Agora: OnJoinChannelSuccess ");
            if (connection.localUid == (uint)PlayerData.myPlayerinfo.memberId)
            {
                if (PlayerData.myPlayerinfo.universityCode.Equals("lobby"))
                {
                    GameEvents.Instance.RequestSetVideoCanvas((int)connection.localUid, connection.channelId);
                }
            }
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            Debug.Log("Agora: OnReJoinChannelSuccess ");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            Debug.Log("Agora: OnLeaveChannelSuccess ");
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
        {
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            if (uid != (uint)PlayerData.myPlayerinfo.memberId)
            {
                GameEvents.Instance.RequestSetVideoCanvas((int)uid, connection.channelId);
            }
        }

        public override void OnUserMuteVideo(RtcConnection connection, uint remoteUid, bool muted)
        {
            base.OnUserMuteVideo(connection, remoteUid, muted);
            Debug.Log(remoteUid.ToString() + "'s video mute " + muted);
            AgoraManager.Instance.UserCamOff(remoteUid, muted);
        }

        public override void OnUserMuteAudio(RtcConnection connection, uint remoteUid, bool muted)
        {
            base.OnUserMuteAudio(connection, remoteUid, muted);
            if (!connection.channelId.Equals(PlayerData.myPlayerinfo.seminarTitle)&&
                !connection.channelId.Equals("test-" + PlayerData.myPlayerinfo.seminarTitle))
            {
                NetworkedEntityFactory.Instance.SetEntityMicOff((int)remoteUid,muted);
            }
            Debug.Log(remoteUid.ToString() + "'s audio mute " + muted);
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            AgoraManager.Instance.DestroyVideoView(uid);
        }

        public override void OnUplinkNetworkInfoUpdated(UplinkNetworkInfo info)
        {
        }

        public override void OnDownlinkNetworkInfoUpdated(DownlinkNetworkInfo info)
        {
        }
    }

    # endregion
