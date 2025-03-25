using UnityEngine;
using System.IO;
using System;

public static class SavWav
{
    const int HEADER_SIZE = 44;

    public static bool Save(string filepath, AudioClip clip)
    {
        try
        {
            string directory = Path.GetDirectoryName(filepath);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            using (var fileStream = new FileStream(filepath, FileMode.Create))
            using (var writer = new BinaryWriter(fileStream))
            {
                float[] samples = new float[clip.samples * clip.channels];
                clip.GetData(samples, 0);

                WriteHeader(writer, clip.samples, clip.channels, clip.frequency);

                Int16[] intData = new Int16[samples.Length];
                byte[] bytesData = new byte[samples.Length * 2];
                int rescaleFactor = 32767;

                for (int i = 0; i < samples.Length; i++)
                {
                    intData[i] = (short)(samples[i] * rescaleFactor);
                    byte[] byteArr = BitConverter.GetBytes(intData[i]);
                    byteArr.CopyTo(bytesData, i * 2);
                }

                writer.Write(bytesData);
                Debug.Log($"Audio saved to: {filepath}");
                return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save WAV file: {e.Message}");
            return false;
        }
    }

    private static void WriteHeader(BinaryWriter writer, int sampleCount, int channels, int frequency)
    {
        writer.Write("RIFF".ToCharArray());
        writer.Write((UInt32)(36 + sampleCount * channels * 2));
        writer.Write("WAVE".ToCharArray());
        writer.Write("fmt ".ToCharArray());
        writer.Write((UInt32)16);
        writer.Write((UInt16)1);
        writer.Write((UInt16)channels);
        writer.Write((UInt32)frequency);
        writer.Write((UInt32)(frequency * channels * 2));
        writer.Write((UInt16)(channels * 2));
        writer.Write((UInt16)16);
        writer.Write("data".ToCharArray());
        writer.Write((UInt32)(sampleCount * channels * 2));
    }
}