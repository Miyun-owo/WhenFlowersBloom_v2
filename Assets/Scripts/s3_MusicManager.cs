using UnityEngine;
using System.Collections.Generic;

public class s3_MusicManager : MonoBehaviour
{
    public static s3_MusicManager Instance;

    AudioSource audioSource;
    readonly List<float> frequencies = new List<float>();
    readonly List<float> phases = new List<float>();
    readonly object frequencyLock = new object();
    const float VolumePerCard = 0.25f;

    void Awake()
    {
        Instance = this;

        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.clip = AudioClip.Create(
            "s3 tone generator",
            AudioSettings.outputSampleRate,
            1,
            AudioSettings.outputSampleRate,
            false
        );
    }

    public void PlayCards(IReadOnlyList<NoteCardData> cards)
    {
        if (cards == null || cards.Count == 0)
        {
            StopAll();
            return;
        }

        lock (frequencyLock)
        {
            frequencies.Clear();

            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] != null && cards[i].frequency > 0f)
                {
                    frequencies.Add(cards[i].frequency);
                }
            }

            while (phases.Count < frequencies.Count)
            {
                phases.Add(0f);
            }
        }

        if (GetFrequencyCount() == 0)
        {
            StopAll();
            return;
        }

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void StopAll()
    {
        lock (frequencyLock)
        {
            frequencies.Clear();
        }

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        float sampleRate = AudioSettings.outputSampleRate;

        for (int i = 0; i < data.Length; i += channels)
        {
            float sample = 0f;

            lock (frequencyLock)
            {
                for (int tone = 0; tone < frequencies.Count; tone++)
                {
                    sample += Mathf.Sin(phases[tone]) * VolumePerCard;
                    phases[tone] += 2f * Mathf.PI * frequencies[tone] / sampleRate;

                    if (phases[tone] > 2f * Mathf.PI)
                    {
                        phases[tone] -= 2f * Mathf.PI;
                    }
                }
            }

            for (int channel = 0; channel < channels; channel++)
            {
                data[i + channel] = sample;
            }
        }
    }

    int GetFrequencyCount()
    {
        lock (frequencyLock)
        {
            return frequencies.Count;
        }
    }
}
