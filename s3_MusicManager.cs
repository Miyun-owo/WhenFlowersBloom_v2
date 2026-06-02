using UnityEngine;
using System.Collections.Generic;

public class s3_MusicManager : MonoBehaviour
{
    public static s3_MusicManager Instance;

    private AudioSource audioSource;

    public static readonly Dictionary<string, float> NoteFrequencies = new Dictionary<string, float>
    {
        { "C4", 261.63f },
        { "E4", 329.63f },
        { "F4", 349.23f },
        { "F#4", 369.99f },
        { "G4", 392.00f },
        { "A4", 440.00f },
        { "A#4", 466.16f },
        { "C5", 523.25f }
    };

    private float frequencyA = 0f;
    private float frequencyB = 0f;

    private float volumeA = 0f;
    private float volumeB = 0f;

    private float phaseA = 0f;
    private float phaseB = 0f;
    private readonly float samplingRate = 44100f;

    private bool isPlaying = false;

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
    }

    public void PlayInterval(NoteCardData a, NoteCardData b)
    {
        frequencyA = a.frequency;
        frequencyB = b.frequency;

        volumeA = 0.4f;
        volumeB = 0.4f;

        isPlaying = true;

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void StopAll()
    {
        isPlaying = false;

        frequencyA = 0f;
        frequencyB = 0f;
        volumeA = 0f;
        volumeB = 0f;

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (AudioListener.pause || !isPlaying) return;

        float phaseIncrementA = (2f * Mathf.PI * frequencyA) / samplingRate;
        float phaseIncrementB = (2f * Mathf.PI * frequencyB) / samplingRate;

        for (int i = 0; i < data.Length; i += channels)
        {
            float sampleA = Mathf.Sin(phaseA) * volumeA;
            float sampleB = Mathf.Sin(phaseB) * volumeB;
            float combinedSample = sampleA + sampleB;

            for (int c = 0; c < channels; c++)
            {
                data[i + c] = combinedSample;
            }

            phaseA += phaseIncrementA;
            if (phaseA > 2f * Mathf.PI) phaseA -= 2f * Mathf.PI;

            phaseB += phaseIncrementB;
            if (phaseB > 2f * Mathf.PI) phaseB -= 2f * Mathf.PI;
        }
    }
}

