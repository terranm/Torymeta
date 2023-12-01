using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WarpZone : MonoBehaviour
{
    private readonly string TEMPSCENE = "TempScene";
    private string zoneName = "Lobby";
    [SerializeField] private GameObject panel;
    
    private void Start()
    {
        zoneName = name;
    }

    private void OnTriggerEnter(Collider other)
    {
        string layer = LayerMask.LayerToName(other.gameObject.layer);
        
        if (!layer.Equals("Player")) return;
        
        PlayerData.myPlayerinfo.universityCode = this.name;
        panel.gameObject.SetActive(true);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("OnCollisionEnter");
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        string layer = LayerMask.LayerToName(hit.gameObject.layer);
        Debug.Log("진입");
        if (layer.Equals("Player"))
        {
            PlayerData.myPlayerinfo.universityCode = zoneName;
            LeaveRoom();
        }
    }
    
    private void LeaveRoom()
    {
        MMOManager.Instance.LeaveRooms();
        SceneManager.LoadSceneAsync(TEMPSCENE);
    }
}