using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

[Serializable]
public class AudioData
{
    public string audio;
}

public class NetworkManager : MonoBehaviour {
    public static NetworkManager Instance;
    private AudioSource audioSource = null;  
    private string url = "http://localhost:3000/upload";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public static void SendAudio(AudioSource audioSource)
    {
        Instance._SendAudio(audioSource);
    }
    
    private void _SendAudio(AudioSource _audioSource) {
        audioSource = _audioSource;
        StartCoroutine(UploadRoutine());
    }

    IEnumerator UploadRoutine() {
        byte[] wavBytes = WavUtility.FromAudioClip(audioSource.clip);
        string base64Audio = Convert.ToBase64String(wavBytes);
        
        AudioData data = new AudioData { audio = base64Audio };
        string json = JsonUtility.ToJson(data);
        
        // Save to JSON file
        string filePath = Path.Combine(Application.persistentDataPath, "audio.json");
        File.WriteAllText(filePath, json);
        Debug.Log("JSON saved to: " + filePath);
        
        UnityWebRequest www = new UnityWebRequest(url, "POST");
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError("Error: " + www.error);
        } else {
            Debug.Log("Server Response: " + www.downloadHandler.text);
        }
    }
}