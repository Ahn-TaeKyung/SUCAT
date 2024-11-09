using System;
using System.Collections;
using System.Collections.Generic;
using _Singleton;
using UnityEngine;

#if UNITY_WEBGL
using System.Runtime.InteropServices;
#endif

[System.Serializable]
public class ScoreData
{
    public int score;
}

public class WebClient11 : Singleton<WebClient11>
{
    #if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void SendData(string json_data);
    #endif

    public void Send(int score)
    {
        ScoreData score_data = new ScoreData();
        score_data.score = score;

        string jsonData = JsonUtility.ToJson(score_data);
        Debug.Log("Unity send: " + jsonData);

        #if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            if (!string.IsNullOrEmpty(jsonData))
            {
                SendData(jsonData);
                Debug.Log("Data sent successfully to JavaScript.");
            }
            else
                Debug.LogError("Serialized JSON data is empty or null.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error occurred while calling SendData: " + ex.Message);
        }
        #endif
    }
}
