using System;
using UnityEngine;
using Whisper.Utils;
using System.Threading.Tasks;

public class RecorderManager : MonoBehaviour
{
    private AudioSource audioSource;
    private bool isRecording = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void ToggleRecording()
    {
        if (!isRecording)
        {
            isRecording = true;
            Debug.Log("Listening...");
            int minFreq, maxFreq;
            Microphone.GetDeviceCaps(null, out minFreq, out maxFreq);
            int recordingFreq = maxFreq > 0 ? maxFreq : 16000; // Fallback to 16k
            audioSource.clip = Microphone.Start(null, false, 10, recordingFreq);
        }
        else
        {
            isRecording = false;
            int lastPos = Microphone.GetPosition(null);
            Microphone.End(null);
            Debug.Log("Processing...");
            
            audioSource.clip = TrimClip(audioSource.clip, lastPos);

            var samples = new float[audioSource.clip.samples * audioSource.clip.channels];
            audioSource.clip.GetData(samples, 0);
            NetworkManager.SendAudio(audioSource);
        }
    }
    
    private AudioClip TrimClip(AudioClip clip, int lastSample) {
        float[] data = new float[lastSample * clip.channels];
        clip.GetData(data, 0);
        AudioClip newClip = AudioClip.Create(clip.name, lastSample, clip.channels, clip.frequency, false);
        newClip.SetData(data, 0);
        return newClip;
    }
}