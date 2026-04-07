using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class AudioSender : MonoBehaviour {
    public static AudioSender Instance;
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

        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        // "audio_file" must match the name in your Node.js upload.single('audio_file')
        formData.Add(new MultipartFormFileSection("audio_file", wavBytes, "clip.wav", "audio/wav"));

        UnityWebRequest www = UnityWebRequest.Post(url, formData);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError("Error: " + www.error);
        } else {
            Debug.Log("Server Response: " + www.downloadHandler.text);
        }
    }
}