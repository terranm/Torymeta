using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerSetupMenu : MonoBehaviour
{

    /// <summary>
    /// Clears the saved log in credentials
    /// </summary>
    public void OnButtonEvent_LogOutExisting()
    {
        MMOPlayerPrefs.Clear();

        // SetUpLogOut();
    }

    private void SetUpLogOut()
    {
        // logOutText.gameObject.SetActive(string.IsNullOrEmpty(MMOPlayerPrefs.Email) == false && string.IsNullOrEmpty(MMOPlayerPrefs.Password) == false);
        // logOutText.text = $"Log Out {MMOPlayerPrefs.Email}";
    }
}
