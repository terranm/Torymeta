using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TouchController : MonoBehaviour
{
    public float perspectiveZoomSpeed = 0.05f;
    private bool isCameraObject = false;
    private bool isPlayerObject = false;
    private bool isMultiTouching = false;
    private Touch touchZero;
    private Touch touchOne;
    private Vector2 touchZeroPrevPos;
    private Vector2 touchOnePrevPos;
    private Vector2 deltaPos;
    private Vector3 mousePos;
    private Vector3 mousePrevPos;
    private Vector3 initialCameraPosition;
    private Quaternion initialCameraRotation;
    private float prevTouchDeltaMag;
    private float touchDeltaMag;
    private float deltaMagnitudeDiff;
    private Vector3 clickPos;
    private Vector3 deltaPosOne;
    private Vector3 deltaPosTwo;
    private float X = 0;
    private float Y = 0;
    private float angle = 0;
    private float prev_angle = 0;
    private float modified_angle = 0;

    private bool zoomin, zoomout;
    private float fov;
    
    [SerializeField] private Image image;
    [SerializeField] private Camera camera;
    [SerializeField] private GameObject avatarRoot;
    [SerializeField] private float rotationSpeed = 1;
    [SerializeField] private GameObject avatar;
    [SerializeField] private bool Z_axisLocked;
    [SerializeField] private bool isUpDownLocked;



    private void Awake()
    {
        
        initialCameraRotation = camera.transform.rotation;
        initialCameraPosition = camera.transform.position;
        
        //Debug.Log(initialCameraPosition + " , " + initialCameraRotation);
    }

    private void Start()
    {
        Application.targetFrameRate = 30;
        
        #if UNITY_EDITOR
        rotationSpeed = 5f;
        #elif UNITY_ANDROID
        rotationSpeed = 10f;
        #elif UNITY_IOS
        rotationSpeed = 10f;
        #endif

        fov = camera.fieldOfView;
        zoomin = false;
        zoomout = false;
        
        
    }
    
    private void Update()
    {
        DirectTouch();
        UpdateAngle();
    }

    
    
    private void DirectTouch()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            avatar.transform.rotation = Quaternion.Euler(X, Y, 0);
            mousePrevPos = mousePos;
            mousePos = Input.mousePosition;

            deltaPos = mousePos - mousePrevPos;

            UpdateRotateAndUpDown(deltaPos);

            //if (!Z_axisLocked)
            //{
            //    camera.transform.RotateAround(avatarRoot.transform.position, camera.transform.right,
            //        -deltaPos.y * Time.deltaTime * rotationSpeed);
            //}

            //// 위아래 이동 기능
            //float y = camera.transform.position.y + (deltaPos.y * Time.deltaTime);
            //camera.transform.position = new Vector3(camera.transform.position.x, Mathf.Clamp(y, 0.35f, 3.6f), camera.transform.position.z);

            //// 로테이션 기능
            //camera.transform.RotateAround(avatarRoot.transform.position, camera.transform.up,
            //    deltaPos.x * Time.deltaTime * rotationSpeed);
            //if (image != null) image.transform.RotateAround(avatarRoot.transform.position, camera.transform.up,
            //     deltaPos.x * Time.deltaTime * rotationSpeed);
        }
        deltaMagnitudeDiff = Input.GetAxis("Mouse ScrollWheel");
        if (deltaMagnitudeDiff != 0)
        {
            

            //Zoom(camera.fieldOfView + deltaMagnitudeDiff * 0.1f);
            // zoom 기능인데 이미 실시간으로 미세 작동하므로 보간할 필요가 없음
            camera.fieldOfView += deltaMagnitudeDiff * 100;
            camera.fieldOfView = Mathf.Clamp(camera.fieldOfView, 10, 60);
        }
#else
        switch (Input.touchCount)
        {
            case 0:
                isMultiTouching = false;
                break;
            case 1 when !isMultiTouching:
                isMultiTouching = false;

                deltaPos = Input.GetTouch(0).deltaPosition;

                UpdateRotateAndUpDown(deltaPos);

                //avatar.transform.rotation = Quaternion.Euler(X, Y, 0);

                
                //if (!Z_axisLocked)
                //{
                //    camera.transform.RotateAround(avatarRoot.transform.position, camera.transform.right,
                //        -deltaPos.y * Time.deltaTime * rotationSpeed);
                //}

                //// 위아래 이동 기능
                //float y = camera.transform.position.y + (deltaPos.y * Time.deltaTime * rotationSpeed);
                //camera.transform.position = new Vector3(camera.transform.position.x, Mathf.Clamp(y, 0.35f, 3.6f), camera.transform.position.z);

                //// 로테이션 기능
                //camera.transform.RotateAround(avatarRoot.transform.position, camera.transform.up,
                //    deltaPos.x * Time.deltaTime * rotationSpeed);
                //if(image != null) image.transform.RotateAround(avatarRoot.transform.position, camera.transform.up,
                //    deltaPos.x * Time.deltaTime * rotationSpeed);
                
                break;
            case 2:
            {
                isMultiTouching = true;

                if (Input.GetTouch(0).position.y > Input.GetTouch(1).position.y)
                {
                    touchZero = Input.GetTouch(0);
                    touchOne = Input.GetTouch(1);
                }
                else
                {
                    touchZero = Input.GetTouch(1);
                    touchOne = Input.GetTouch(0);
                }


                touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                //Zoom(camera.fieldOfView + deltaMagnitudeDiff * 0.1f);
                // zoom 기능인데 이미 실시간으로 미세 작동하므로 보간할 필요가 없음
                camera.fieldOfView += deltaMagnitudeDiff * 0.1f;
                camera.fieldOfView = Mathf.Clamp(camera.fieldOfView, 10, 60);

                //if (Z_axisLocked) return;
                
                //camera.transform.RotateAround(avatarRoot.transform.position, camera.transform.forward,
                //    (touchZero.deltaPosition - touchOne.deltaPosition).x * Time.deltaTime * rotationSpeed * 0.6f);
                //camera.transform.RotateAround(avatarRoot.transform.position, camera.transform.forward,
                //    (touchZero.deltaPosition - touchOne.deltaPosition).y * Time.deltaTime * rotationSpeed * 0.6f);
                break;
            }
        }
#endif
    }

    private void UpdateAngle()
    {
        /* Rotation Button Click Event*/
        angle = Mathf.Lerp(angle, modified_angle, 8 * Time.deltaTime);
        camera.transform.RotateAround(avatar.transform.position, camera.transform.up, angle - prev_angle);
        if(image != null) image.transform.RotateAround(avatar.transform.position, camera.transform.up, angle - prev_angle);
        prev_angle = angle;
    }

    private void UpdateRotateAndUpDown(Vector2 pos)
    {
        avatar.transform.rotation = Quaternion.Euler(X, Y, 0);

        deltaPos = pos;//Input.GetTouch(0).deltaPosition;

        if (!Z_axisLocked)
        {
            camera.transform.RotateAround(avatarRoot.transform.position, camera.transform.right,
                -deltaPos.y * Time.deltaTime * rotationSpeed);
        }

        if (!isUpDownLocked)
        {
            // 위아래 이동 기능
            float y = camera.transform.position.y - (deltaPos.y * Time.deltaTime);
            camera.transform.position = new Vector3(camera.transform.position.x, Mathf.Clamp(y, 0.35f, 3.6f), camera.transform.position.z);

        }
        // 로테이션 기능
        camera.transform.RotateAround(avatarRoot.transform.position, camera.transform.up,
            deltaPos.x * Time.deltaTime * rotationSpeed);
        if (image != null) image.transform.RotateAround(avatarRoot.transform.position, camera.transform.up,
             deltaPos.x * Time.deltaTime * rotationSpeed);
    }
    
    #region NativeCallMethod

    public void SwitchScene(string msg)
    {
        SwitchScene value = JsonConvert.DeserializeObject<SwitchScene>(msg);
        SceneManager.LoadSceneAsync(value.member.universityCode);
    }

    public void Zoom(float change)
    {
        StartCoroutine(LerpZoom(change));
        //camera.fieldOfView += -10;
        //camera.fieldOfView = Mathf.Clamp(camera.fieldOfView, 10, 60);
    }

    public void ZoomIn()
    {
        Zoom(camera.fieldOfView + -10f);
        //StartCoroutine(LerpZoom(camera.fieldOfView - 10f));
        //camera.fieldOfView += -10;
        //camera.fieldOfView = Mathf.Clamp(camera.fieldOfView, 10, 60);
    } 
    
    public void ZoomOut()
    {
        Zoom(camera.fieldOfView + 10f);
        //StartCoroutine(LerpZoom(camera.fieldOfView + 10f));
        //camera.fieldOfView += 10;
        //camera.fieldOfView = Mathf.Clamp(camera.fieldOfView, 10, 60);
    }

    private IEnumerator LerpZoom(float destFov)
    {
        float fov = camera.fieldOfView;
        while (!Mathf.Approximately(camera.fieldOfView, destFov))
        {
            fov = Mathf.Lerp(fov, destFov, 8 * Time.deltaTime);
            fov = Mathf.Clamp(fov, 10, 60);
            //y = Mathf.Clamp(y, 10, 60);
            camera.fieldOfView = fov;
            yield return new WaitForFixedUpdate();
        }
        Debug.Log("camera.fieldOfView " + camera.fieldOfView + " fov " + fov + " destY " + destFov);
        camera.fieldOfView = destFov;
        yield return null;
    }

    public void Rotation()
    {
        modified_angle += 45;
    }

    public void MoveHorizontal(float change)
    {
        StartCoroutine(LerpMoveHorizontal(change));
    }

    public void MoveUpHorizontal()
    {
        MoveHorizontal(camera.transform.position.y + 0.1f);
        //StartCoroutine(LerpMoveHorizontal(camera.transform.position.y + 0.1f));
    }

    public void MoveDownHorizontal()
    {
        MoveHorizontal(camera.transform.position.y + -0.1f);
        //StartCoroutine(LerpMoveHorizontal(camera.transform.position.y - 0.1f));
    }

    private IEnumerator LerpMoveHorizontal(float destY)
    {
        float y = camera.transform.position.y;
        while (!Mathf.Approximately(camera.transform.position.y, destY))
        {
            y = Mathf.Lerp(y, destY, 8 * Time.deltaTime);
            y = Mathf.Clamp(y, 0.35f, 3.6f);
            camera.transform.position = new Vector3(camera.transform.position.x, y, camera.transform.position.z);
            yield return new WaitForFixedUpdate();
        }
        Debug.Log("camera.transform.position.y " + camera.transform.position.y + " y " + y + " destY " + destY);
        camera.transform.position = new Vector3(camera.transform.position.x, destY, camera.transform.position.z);
        yield return null;
    }

    /*
    public void Select(string id)
    {
        //GameObject avatar = avatarContainer.transform.Find(id).gameObject;
        //SetAvatar(avatar);
    }
    */

    #endregion


    public void CreateAvatar()
    {
        if(!avatar.activeSelf) avatar.SetActive(true);
        //네이티브에서 받아온 변화 값이 저장된 PlayerData 적용
    }
    
    public void AvatarReset()
    {
        //네이티브에서 받아온 변화 값이 저장된 PlayerData 적용하여 변화값 초기화
    }

    public GameObject GetAvatar()
    {
        return avatar;
    }
    
    public void ResetZoom()
    {
        camera.fieldOfView = 30;
    }

    public void ResetRotation()
    {
        camera.transform.rotation = initialCameraRotation;
    }

    public void ResetPosition()
    {
        camera.transform.position = initialCameraPosition;
    }

    public void ResetPositionNoneY()
    {
        camera.transform.position = new Vector3(initialCameraPosition.x, camera.transform.position.y, initialCameraPosition.z);
    }

}
