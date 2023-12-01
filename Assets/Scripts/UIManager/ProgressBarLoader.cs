using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class ProgressBarLoader : MonoBehaviour
{
    [SerializeField] private Button enterBtn;
    [SerializeField] private ScrollRect scrollRect;

    [SerializeField] private Slider progressBar;
    [SerializeField] Text Percentage;
    [SerializeField] private GameObject loadingIcon;

    private float rotationSpeed = 400;
    private float rotZ = 0;
    private Coroutine co;

    private readonly long megaByte = 1048576;
    
    private void Start()
    {
        StartCoroutine(UpdateUI());
    }

    IEnumerator UpdateUI()
    {
        while (true)
        {
            if (PlayerData.myPlayerinfo.universityCode.Equals("Lobby"))
                yield break;
            
            UpdateLoadingIcon();
            UpdateProgressBar(Addressables.GetDownloadSizeAsync(PlayerData.myPlayerinfo.universityCode).PercentComplete);

            yield return null;
        }
    }

    private void UpdateLoadingIcon()
    {
        if (loadingIcon.activeSelf == false)
            loadingIcon.gameObject.SetActive(true);

        rotZ += rotationSpeed * Time.deltaTime;

        loadingIcon.transform.localRotation = Quaternion.Euler(0, 0, rotZ);
    }

    private void UpdateProgressBar(float percentage)
    {
        if (progressBar.gameObject.activeSelf == false)
            progressBar.gameObject.SetActive(true);

        progressBar.value = percentage;
        Percentage.text = Math.Round(percentage * 100f) + " %";
    }
}