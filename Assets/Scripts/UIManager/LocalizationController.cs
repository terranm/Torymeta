using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using TMPro;
using System.Threading.Tasks;
using System;

public class LocalizationController : Singleton<LocalizationController>
{
    public bool isChangeLocaleComplete = false;
    // Start is called before the first frame update
    void Start()
    {
        switch (Application.systemLanguage)
        {
            case SystemLanguage.English:
                ChangeLocale(0);
                break;
            case SystemLanguage.Korean:
                //default:
                ChangeLocale(1);
                break;
        }
    }
#if UNITY_EDITOR
    //// Update is called once per frame
    //async void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.J))
    //    {
    //        string entryKey = "test";

    //        string text = await GetLocalizeText(entryKey);

    //        Debug.Log(text);
    //    }

    //    if (Input.GetKeyDown(KeyCode.Alpha1))
    //    {
    //        ChangeLocale(0);
    //    }
    //    if (Input.GetKeyDown(KeyCode.Alpha2))
    //    {
    //        ChangeLocale(1);
    //    }
    //}
#endif

    //public string Result { set { _result = ""; } get { return _result; } }

    //public async Task<string> GetLocalizeText_11Async(string entryKey)
    //{
    //    await GetLocalizeText();


    //    return _result;
    //}

    //public IEnumerator GetLocalizeText(string entryKey)
    //{
    //    LocalizeText(entryKey);
    //    while (_result != "")
    //    {
    //        yield return new WaitForFixedUpdate();
    //    }

    //    yield return null;
    //}
    public async void WaitLocaleText(Action<string> callback, string entry)
    {
        //int cnt = 0;
        //Debug.Log("waitLocaleComplete" + cnt++ + LocalizationController.Instance.isChangeLocaleComplete);
        /*
        while (!isChangeLocaleComplete)
        {
            yield return new WaitForFixedUpdate();
            //Debug.Log("waitLocaleComplete" + cnt++);
        }
        string text = await GetLocalizeText(entry);
        while (_result == "")
        {
            yield return new WaitForFixedUpdate();
        }
        callback(_result);
        _result = "";

        yield return null;
        */
        
        string text = await GetLocalizeText(entry);
        callback(text);
    }

    private async Task<string> GetLocalizeText(string entryKey)
    {
        //const string tableName = "ToryMetaTable"; //"New Table";
        LocalizedStringDatabase stringdatabase = LocalizationSettings.StringDatabase;

        var task = await stringdatabase.GetDefaultTableAsync().Task;
        var result = task.GetEntry(entryKey);

        Debug.Log(result.Value);
        //Debug.Log(k.GetLocalizedString());

        //string temp = stringdatabase.GetLocalizedString(tableName, entryKey);
        //Debug.Log(temp);

        //_result = result.Value;

        return result.Value;
        //_result = result.Value;
        //// 非同期でEntry取得
        //var entry = (await LocalizationSettings.StringDatabase.GetTableEntryAsync(tableName, entryKey).Task).Entry;

        //Debug.Log(entry.Value);
        //Debug.Log(entry.GetLocalizedString());
    }

    bool isChanging = false;

    public void ChangeLocale(int index)
    {
        if (isChanging)
            return;

        StartCoroutine(LocaleChange(index));
    }

    IEnumerator LocaleChange(int index)
    {
        isChanging = true;

        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        Debug.Log("LocaleChange : " + LocalizationSettings.SelectedLocale.ToString());

        isChanging = false;

        isChangeLocaleComplete = true;
    }
}
