using UnityEngine;
using Whisper;
using Whisper.Utils;
using System.Threading.Tasks;

public class SpeechToEmotion : MonoBehaviour
{
    public WhisperManager whisper;
    public AudioSource audioSource;
    private bool isRecording = false;

    // --- 1. INITIALIZE ON START ---
    async void Start()
    {
        Debug.Log("Loading Whisper Model...");
        // This ensures the model in StreamingAssets is actually loaded into memory
        await whisper.InitModel(); 
        Debug.Log("Whisper Model Ready!");
    }

    public async void ToggleRecording()
    {
        // Check if model is loaded before doing anything
        if (whisper.IsLoading)
        {
            Debug.LogError("Model is still loading, please wait a moment!");
            return;
        }

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
            AudioSender.SendAudio(audioSource);

            // Send to Local Whisper
            var result = await whisper.GetTextAsync(samples, audioSource.clip.frequency, audioSource.clip.channels);
            
            if (result != null)
            {
                string finalSpeech = result.Result;
                Debug.Log($"You said: {finalSpeech}");
            }
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