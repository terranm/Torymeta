using System;
using System.Collections;
using System.Collections.Generic;
using RenderHeads.Media.AVProVideo;
using UnityEngine;

public class AVProMediaPlayerController : MonoBehaviour
{
    public MediaPlayer AVMP = null;
    public ApplyToMesh AppToMesh = null;
    public bool isMediaPlayerEnded = false;

    //private bool isMediaPlayerStarted = false;
    private bool isPlayStart = false;
    private bool isUnstalled = false;
    private bool isInit = false;
    private bool isReadyToPlay = false;

    private void Awake()
    {
        
        Init();
    }


    // Start is called before the first frame update
    public void Init()
    {
        isInit = true;
        Debug.Log("MediaPlayer Init");
        //this.transform.GetComponent<MediaPlayer>().Events.AddListener(HandleEvent);
        AVMP = this.transform.GetComponent<MediaPlayer>();
        AppToMesh = this.transform.GetComponentInChildren<ApplyToMesh>();
        AVMP.CloseMedia();
        AVMP.OpenMedia(new MediaPath(PlayerData.myPlayerinfo.seminarUrl, MediaPathType.AbsolutePathOrURL), false);
        if (AppToMesh.Player.Equals(null))
            AppToMesh.Player = AVMP;
        if (AppToMesh.MeshRenderer.Equals(null))
            AppToMesh.MeshRenderer = this.transform.GetComponentInChildren<MeshRenderer>();
        AVMP.Loop = false;
        AppToMesh.transform.GetChild(0).gameObject.SetActive(true);
        //AVMP.Info.GetVideoWidth();
        //AVMP.TextureProducer.GetTexturePixelAspectRatio();
        AVMP.Events.RemoveAllListeners();
        AVMP.Events.AddListener(CheckMediaPlayerEvent);

        StartCoroutine(YieldSetting());
    }

    private void Start()
    {
        
    }
    private void Update()
    {
        //if (!isMediaPlayerEnded && isPlayStart && !isMediaPlayerStarted)
        //{
        //    Debug.Log("Player Error, PlayStart");
        //    AVMP.Play();
        //}
    }

    private void OnDestroy()
    {
        //if (GameEvents.Instance != null)
        //{
        //    GameEvents.Instance.OnRequstMediaPlay -= Init;
        //}
    }

    //private void OnDisable()
    //{
    //    if(AVMP != null)
    //        AVMP.ForceDispose();
    //}

    private void CheckMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
    {
        Debug.Log("MediaPlayer " + mp.name + " generated event: " + et.ToString());
        switch (et)
        {
            case MediaPlayerEvent.EventType.FirstFrameReady:
                Debug.Log("MediaPlayer - FirstFrameReady");
                break;
            case MediaPlayerEvent.EventType.FinishedPlaying:
                isMediaPlayerEnded = true;
                ActiveCurtain();
                Debug.Log("MediaPlayer - FinishedPlaying // isMediaPlayerEnded : " + isMediaPlayerEnded);
                break;
            case MediaPlayerEvent.EventType.Started:
                Debug.Log("MediaPlayer - Started");
                AppToMesh.transform.GetChild(0).gameObject.SetActive(false);
                isUnstalled = true;
                break;
            case MediaPlayerEvent.EventType.Stalled:
                Debug.Log("MediaPlayer - Stalled");
                AppToMesh.transform.GetChild(0).gameObject.SetActive(true);
                isUnstalled = false;
                StartCoroutine(WaitInit());
                break;
            case MediaPlayerEvent.EventType.Unstalled:
                Debug.Log("MediaPlayer - Unstalled");
                AppToMesh.transform.GetChild(0).gameObject.SetActive(false);
                isUnstalled = true;
                //Init();
                break;
            case MediaPlayerEvent.EventType.ResolutionChanged:
                Debug.Log("MediaPlayer - ResolutionChanged");
                //Init();
                break;
            case MediaPlayerEvent.EventType.ReadyToPlay:
                Debug.Log("MediaPlayer - ReadyToPlay");
                isReadyToPlay = true;
                //Init();
                break;
            case MediaPlayerEvent.EventType.Error:
                Debug.Log("MediaPlayer - Error " + errorCode);
                break;
        }
    }


    IEnumerator WaitInit()
    {
        yield return new WaitForSeconds(2f);
        Init();
        yield return new WaitForSeconds(2f);

        yield return null;
    }

    public void CloseMedia()
    {
        Debug.Log("AVMP.CloseMedia();");
        AVMP.ForceDispose();
    }

    IEnumerator YieldSetting()
    {
        while (AVMP.Info.GetVideoWidth() == 0)
        {
            yield return new WaitForUpdate();
        }

        RatioSetting();
        isInit = false;
        //SeekSetting();
        yield return null;
    }

    public IEnumerator WaitSeekSetting()
    {
        do
        {
            yield return new WaitForSeconds(0.5f);
        }
        while (!isReadyToPlay);

        SeekSetting();
        yield return null;
    }

    private void RatioSetting()
    {
        float videoWidth = (float)AVMP.Info.GetVideoWidth();
        float videoHeight = (float)AVMP.Info.GetVideoHeight();
        float videoRatio = videoWidth / videoHeight;
        float meshRatio = AppToMesh.transform.localScale.x / AppToMesh.transform.localScale.z;

        if (videoRatio > meshRatio)
        {
            // 1 : videoRatio = z : x => x = z * videoRatio => z = x / videoRatio
            AppToMesh.transform.localScale = new Vector3(AppToMesh.transform.localScale.x, AppToMesh.transform.localScale.y, AppToMesh.transform.localScale.x / videoRatio);
        }
        else
        {
            AppToMesh.transform.localScale = new Vector3(AppToMesh.transform.localScale.z * videoRatio, AppToMesh.transform.localScale.y, AppToMesh.transform.localScale.z);
        }
    }

    private void SeekSetting()
    {
        // Debug.Log("PlayerData.myPlayerinfo.seminarStartTime " + PlayerData.myPlayerinfo.timeDiff + " DateTime.Now " + DateTime.Now.ToString());
        if (PlayerData.myPlayerinfo.universityCode == "lobby")// string.IsNullOrEmpty(PlayerData.myPlayerinfo.videoStartTime))
        {
            AVMP.Loop = true;
            AVMP.Control.Seek(0.1f);
            AVMP.Play();
            isPlayStart = true;
            return;
        }
        /// 2023-07-03 20:41:40
        //string[] startTime = PlayerData.myPlayerinfo.seminarStartTime.Split(' ');
        //string[] date = startTime[0].Split('-');
        //string[] time = startTime[1].Split(':');
        try
        {
            DateTime ST = Convert.ToDateTime(PlayerData.myPlayerinfo.videoStartTime);//new DateTime(int.Parse(date[0]), int.Parse(date[1]), int.Parse(date[2]), int.Parse(time[0]), int.Parse(time[1]), int.Parse(time[2]));
            TimeSpan timeSpan = (DateTime.Now - ST);
            double spanSecond = timeSpan.TotalSeconds;//PlayerData.myPlayerinfo.timeDiff; 

            Debug.Log("timeSpan.TotalSeconds " + spanSecond);// + " timeSpan " + timeSpan);
            if (spanSecond <= 0)
            {
                AVMP.Pause();
                StartCoroutine(WaitingForStartTime(spanSecond * -1));
            }
            else
            {
#if UNITY_EDITOR
                double seek = spanSecond % AVMP.Info.GetDuration();
                AVMP.Control.Seek(seek);
#else
                AVMP.Control.Seek(spanSecond);
#endif
                AVMP.Play();
                isPlayStart = true;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    IEnumerator WaitingForStartTime(double timeSpan)
    {
        yield return new WaitForSeconds((float)timeSpan);
        AVMP.Play();
        isPlayStart = true;
        yield return null;
    }

    public bool AudioMuteConvert()
    {
        AVMP.AudioMuted = !AVMP.AudioMuted;
        return AVMP.AudioMuted;
    }


    //private void HandleEvent(MediaPlayer mp, MediaPlayerEvent.EventType eventType, ErrorCode code)
    //{
    //    if (eventType == MediaPlayerEvent.EventType.Error)
    //    {
    //        Debug.LogError("Error: " + code);
    //    }
    //    switch (eventType.ToString())
    //    {
    //        case "FinishedPlaying":
    //            break;
    //        default:
    //            break;
    //    }
    //}

    private void ActiveCurtain()
    {
        AVMP.CloseMedia();
        AppToMesh.transform.GetChild(0).gameObject.SetActive(true);
    }
}
