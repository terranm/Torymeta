// using System;
// using System.Collections;
// using System.Collections.Generic;
// using Newtonsoft.Json;
// using UnityEngine;
// using UnityEngine.SceneManagement;
//
// public class NativeCall_Select : MonoBehaviour
// {
//     private GameObject controllerObject;
//     private TouchController touchController;
//
//     private void Start()
//     {
//         controllerObject = GameObject.FindGameObjectWithTag("AvatarContainer");
//         touchController = controllerObject.GetComponent<TouchController>();
//         
//         SelectView_Select("");
//     }
//
//     public void SwitchScene(string msg, string test = "")
//     {
//         Debug.Log("SwitchScene");
//         msg = "{\"member\":{ \"birth\":\"19940809\",\"emailId\":\"testaos@torymeta.io\",\"imgUrl\":\"https://test-torymeta-member.s3.ap-northeast-2.amazonaws.com/229412182654_img_230213_161149.jpg\",\"locationExposeYn\":1,\"memberId\":229412182654,\"name\":\"TempName\",\"temp\" : \"tt\",\"phoneNum\":\"01021976830\",\"profileName\":\"Test\",\"universityCode\":\"CAU\"},\"scene\":\"university\",\"characterId\":\"100_10\"}";
//         SwitchScene value = JsonConvert.DeserializeObject<SwitchScene>(msg);
//
//         PlayerData.myPlayerinfo.universityCode = test;
//         PlayerData.myPlayerinfo.userName = value.member.profileName;
//         PlayerData.myPlayerinfo.avatar = value.characterId;
//         
//         Debug.Log("json : " + msg);
//         Debug.Log("value : " + value.scene + " , " + value.member.universityCode + " , " + value.member.profileName + " , " + value.characterId +" , " + PlayerData.myPlayerinfo.universityCode);
//         
//         if (!(PlayerData.myPlayerinfo.universityCode.Equals("AvatarView") || PlayerData.myPlayerinfo.universityCode.Equals("MyRoom")))
//         {
//             SceneManager.LoadSceneAsync("TempScene");
//         }
//         else
//             SceneManager.LoadSceneAsync(PlayerData.myPlayerinfo.universityCode);
//     }
//
//     public void SelectView_Select(string msg)
//     {
//         Debug.Log("AvatarSelect_Select");
//         // msg = "{ \"characterId\": \"100_6\"}";
//         CharacterSelect value = JsonConvert.DeserializeObject<CharacterSelect>(msg);
//
//         touchController.Select(string.IsNullOrEmpty(PlayerData.myPlayerinfo.avatar)
//             ? value.characterId
//             : PlayerData.myPlayerinfo.avatar);
//
//         /*if(!string.IsNullOrEmpty(value.characterId))
//             touchController.Select(value.characterId);
//         else
//         {
//             touchController.Select(PlayerData.myPlayerinfo.avatar);
//         }*/
//
//         Debug.Log(value.characterId);
//         
//         touchController.ResetZoom();
//         touchController.ResetRotation();
//     }
//
//     public void SelectView_ZoomIn(string msg)
//     {
//         Debug.Log("AvatarSelect_ZoomIn");
//         touchController.ZoomIn();
//     }
//
//     public void SelectView_ZoomOut(string msg)
//     {
//         Debug.Log("AvatarSelect_ZoomOut");
//         touchController.ZoomOut();
//     }
//
//     public void SelectView_Rotation(string msg)
//     {
//         Debug.Log("AvatarSelect_Rotation");
//         touchController.Rotation();
//     }
//
// #if UNITY_EDITOR
//     private void Update()
//     {
//         if (Input.GetKeyDown(KeyCode.Q))
//         {
//             SelectView_Select("{ \"characterId\": \"100_6\"}");
//         }
//         
//         if (Input.GetKeyDown(KeyCode.A))
//         {
//             SelectView_Select("{ \"characterId\": \"\"}");
//         }
//
//         if (Input.GetKeyDown(KeyCode.E))
//         {
//             SelectView_ZoomIn("");
//         }
//
//         if (Input.GetKeyDown(KeyCode.R))
//         {
//             SelectView_ZoomOut("");
//         }
//
//         if (Input.GetKeyDown(KeyCode.T))
//         {
//             SelectView_Rotation("");
//         }
//         
//         if (Input.GetKeyDown(KeyCode.W))
//         {
//             SwitchScene("","AvatarView");
//         }if (Input.GetKeyDown(KeyCode.Q))
//         {
//             SwitchScene("","MyRoom");
//         }
//     }
// #endif
// }