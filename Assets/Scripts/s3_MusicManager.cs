using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class s3_MusicManager : MonoBehaviour
{
    public static s3_MusicManager Instance;

    [Header("Audio Mixer")]
    public AudioMixer mixer;
    public AudioMixerGroup[] mainGroups;
    public string[] mainVolumeParameters =
    {
        "S3_Main_0_dB",
        "S3_Main_1_dB",
        "S3_Main_2_dB"
    };

    [Header("Timing")]
    public float channelBlendDuration = 1.5f;

    [Header("Levels")]
    public float mainLevelDb = 0f;
    public float mutedLevelDb = -80f;

    readonly List<Voice> voices = new List<Voice>();

    void Awake()
    {
        Instance = this;
    }

    public void PlayCards(IReadOnlyList<NoteCardData> cards)
    {
        if (cards == null || cards.Count == 0)
        {
            StopAll();
            return;
        }

        StopAllCoroutines();
        StopRemovedVoices(cards);

        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            if (card == null) continue;

            var voice = GetVoice(card.cardName);
            if (voice == null)
            {
                voice = CreateVoice(card);
                voices.Add(voice);
            }
        }

        ApplyLayout(cards);
    }

    public void StopAll()
    {
        StopAllCoroutines();

        foreach (var voice in voices)
        {
            DestroyVoice(voice);
        }

        voices.Clear();
    }

    void StopRemovedVoices(IReadOnlyList<NoteCardData> cards)
    {
        for (int i = voices.Count - 1; i >= 0; i--)
        {
            if (!ContainsCard(cards, voices[i].cardName))
            {
                DestroyVoice(voices[i]);
                voices.RemoveAt(i);
            }
        }
    }

    bool ContainsCard(IReadOnlyList<NoteCardData> cards, string cardName)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] != null && cards[i].cardName == cardName)
                return true;
        }

        return false;
    }

    void ApplyLayout(IReadOnlyList<NoteCardData> cards)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            if (card == null) continue;

            var voice = GetVoice(card.cardName);
            if (voice == null) continue;

            ConfigureVoiceSlot(voice, i);
            StartMainVoice(voice);

            if (cards.Count == 2)
            {
                float separatedPan = i == 0 ? -1f : 1f;
                voice.mainSource.panStereo = separatedPan;
                StartCoroutine(BlendPanToCenter(voice, separatedPan, channelBlendDuration));
            }
            else
            {
                StartCoroutine(AnimatePan(voice, voice.mainSource.panStereo, 0f, channelBlendDuration));
            }
        }
    }

    void ConfigureVoiceSlot(Voice voice, int slot)
    {
        voice.slot = slot;

        if (voice.mainSource != null)
        {
            voice.mainSource.outputAudioMixerGroup = GetGroup(mainGroups, slot);
        }
    }

    Voice CreateVoice(NoteCardData card)
    {
        int slot = voices.Count;
        var voice = new Voice(card.cardName, card.frequency, slot);

        voice.mainSource = CreateToneSource(
            $"{card.cardName}_main",
            card.frequency,
            GetGroup(mainGroups, slot)
        );

        SetMainLevel(voice, mutedLevelDb);

        return voice;
    }

    AudioSource CreateToneSource(string sourceName, float frequency, AudioMixerGroup group)
    {
        var sourceObject = new GameObject(sourceName);
        sourceObject.transform.SetParent(transform);

        var source = sourceObject.AddComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.volume = 0f;
        source.outputAudioMixerGroup = group;
        source.clip = ToneClip.Create(sourceName, frequency);

        return source;
    }

    AudioMixerGroup GetGroup(AudioMixerGroup[] groups, int index)
    {
        if (groups == null || index < 0 || index >= groups.Length)
            return null;

        return groups[index];
    }

    void StartMainVoice(Voice voice)
    {
        SetMainLevel(voice, mainLevelDb);

        if (!voice.mainSource.isPlaying)
        {
            voice.mainSource.Play();
        }
    }

    IEnumerator BlendPanToCenter(Voice voice, float separatedPan, float duration)
    {
        yield return AnimatePan(voice, separatedPan, 0f, duration);
    }

    IEnumerator AnimatePan(Voice voice, float startPan, float targetPan, float duration)
    {
        if (voice == null || voice.mainSource == null) yield break;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            voice.mainSource.panStereo = Mathf.Lerp(startPan, targetPan, t);

            yield return null;
        }

        voice.mainSource.panStereo = targetPan;
    }

    void DestroyVoice(Voice voice)
    {
        if (voice == null) return;

        SetMainLevel(voice, mutedLevelDb);

        if (voice.mainSource != null)
            Destroy(voice.mainSource.gameObject);
    }

    Voice GetVoice(string cardName)
    {
        for (int i = 0; i < voices.Count; i++)
        {
            if (voices[i].cardName == cardName)
                return voices[i];
        }

        return null;
    }

    void SetMainLevel(Voice voice, float db)
    {
        voice.mainDb = db;
        if (SetMixerLevel(GetParameter(mainVolumeParameters, voice.slot), db))
        {
            if (voice.mainSource != null)
            {
                voice.mainSource.volume = 1f;
            }
        }
        else if (voice.mainSource != null)
        {
            voice.mainSource.volume = DbToLinear(db);
        }
    }

    bool SetMixerLevel(string parameter, float db)
    {
        if (mixer == null || string.IsNullOrEmpty(parameter))
            return false;

        return mixer.SetFloat(parameter, db);
    }

    string GetParameter(string[] parameters, int index)
    {
        if (parameters == null || index < 0 || index >= parameters.Length)
            return null;

        return parameters[index];
    }

    float DbToLinear(float db)
    {
        if (db <= mutedLevelDb)
            return 0f;

        return Mathf.Pow(10f, db / 20f);
    }

    class Voice
    {
        public readonly string cardName;
        public readonly float frequency;
        public int slot;
        public AudioSource mainSource;
        public float mainDb;

        public Voice(string cardName, float frequency, int slot)
        {
            this.cardName = cardName;
            this.frequency = frequency;
            this.slot = slot;
        }
    }

    class ToneClip
    {
        float phase;
        readonly float frequency;

        ToneClip(float frequency)
        {
            this.frequency = frequency;
        }

        public static AudioClip Create(string name, float frequency)
        {
            var generator = new ToneClip(frequency);
            int sampleRate = AudioSettings.outputSampleRate;

            return AudioClip.Create(
                name,
                sampleRate,
                1,
                sampleRate,
                true,
                generator.OnAudioRead
            );
        }

        void OnAudioRead(float[] data)
        {
            float sampleRate = AudioSettings.outputSampleRate;

            for (int i = 0; i < data.Length; i++)
            {
                float waveSample = Mathf.Sin(phase) >= 0f ? 1f : -1f;
                data[i] = waveSample * 0.2f;
                phase += 2f * Mathf.PI * frequency / sampleRate;

                if (phase > 2f * Mathf.PI)
                {
                    phase -= 2f * Mathf.PI;
                }
            }
        }
    }
}
