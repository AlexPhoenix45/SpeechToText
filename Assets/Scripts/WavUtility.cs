using UnityEngine;
using System;
using System.IO;

public static class WavUtility {
    public static byte[] FromAudioClip(AudioClip clip) {
        using (var stream = new MemoryStream()) {
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            // Write WAV Header
            AddHeader(stream, clip);
            
            // Convert float samples to 16-bit PCM
            foreach (var sample in samples) {
                short value = (short)(sample * 32767);
                stream.Write(BitConverter.GetBytes(value), 0, 2);
            }
            return stream.ToArray();
        }
    }

    private static void AddHeader(Stream stream, AudioClip clip) {
        var hz = clip.frequency;
        var channels = clip.channels;
        var samples = clip.samples;

        stream.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"), 0, 4);
        stream.Write(BitConverter.GetBytes(stream.Length + 36), 0, 4);
        stream.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"), 0, 4);
        stream.Write(System.Text.Encoding.UTF8.GetBytes("fmt "), 0, 4);
        stream.Write(BitConverter.GetBytes(16), 0, 4); // Subchunk1Size
        stream.Write(BitConverter.GetBytes((ushort)1), 0, 2); // AudioFormat (PCM)
        stream.Write(BitConverter.GetBytes((ushort)channels), 0, 2);
        stream.Write(BitConverter.GetBytes(hz), 0, 4);
        stream.Write(BitConverter.GetBytes(hz * channels * 2), 0, 4); // ByteRate
        stream.Write(BitConverter.GetBytes((ushort)(channels * 2)), 0, 2); // BlockAlign
        stream.Write(BitConverter.GetBytes((ushort)16), 0, 2); // BitsPerSample
        stream.Write(System.Text.Encoding.UTF8.GetBytes("data"), 0, 4);
        stream.Write(BitConverter.GetBytes(samples * channels * 2), 0, 4);
    }
}