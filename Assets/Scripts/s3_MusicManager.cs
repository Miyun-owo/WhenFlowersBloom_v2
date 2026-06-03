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
    public AudioMixerGroup[] introGroups;
    public string[] mainVolumeParameters =
    {
        "S3_Main_0_dB",
        "S3_Main_1_dB",
        "S3_Main_2_dB"
    };
    public string[] introVolumeParameters =
    {
        "S3_Intro_0_dB",
        "S3_Intro_1_dB",
        "S3_Intro_2_dB"
    };

    [Header("Timing")]
    public float introDuration = 1f;
    public float introFadeOutDuration = 0.3f;
    public float balanceDuration = 0.5f;

    [Header("Levels")]
    public float mainLevelDb = 0f;
    public float mutedLevelDb = -80f;
    public float introLevelDb = -6f;

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

        bool startedSingleCardIntro = false;

        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            if (card == null) continue;

            var voice = GetVoice(card.cardName);
            if (voice == null)
            {
                voice = CreateVoice(card);
                voices.Add(voice);

                if (cards.Count == 1)
                {
                    ConfigureVoiceSlot(voice, 0);
                    StartCoroutine(PlaySingleCardIntro(voice));
                    startedSingleCardIntro = true;
                }
                else
                {
                    StartMainVoice(voice, mutedLevelDb);
                }
            }
        }

        if (startedSingleCardIntro)
            return;

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

            float pan = GetPan(i, cards.Count);
            voice.introSource.Stop();
            SetIntroLevel(voice, mutedLevelDb);
            StartMainVoice(voice, mutedLevelDb);
            StartCoroutine(AnimateMainVoice(voice, pan, mainLevelDb, balanceDuration));
        }
    }

    float GetPan(int index, int count)
    {
        if (count == 1) return 0f;
        if (index == 0) return -1f;
        if (index == 1) return 1f;

        return 0f;
    }

    void ConfigureVoiceSlot(Voice voice, int slot)
    {
        voice.slot = slot;

        if (voice.mainSource != null)
        {
            voice.mainSource.outputAudioMixerGroup = GetGroup(mainGroups, slot);
        }

        if (voice.introSource != null)
        {
            voice.introSource.outputAudioMixerGroup = GetGroup(introGroups, slot);
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
        voice.introSource = CreateToneSource(
            $"{card.cardName}_intro",
            card.frequency * 0.5f,
            GetGroup(introGroups, slot)
        );

        SetMainLevel(voice, mutedLevelDb);
        SetIntroLevel(voice, mutedLevelDb);

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

    IEnumerator PlaySingleCardIntro(Voice voice)
    {
        if (voice == null) yield break;

        StartMainVoice(voice, mutedLevelDb);
        voice.introSource.Play();
        SetIntroLevel(voice, introLevelDb);

        yield return new WaitForSeconds(introDuration);

        float elapsed = 0f;
        while (elapsed < introFadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / introFadeOutDuration);

            SetIntroLevel(voice, Mathf.Lerp(introLevelDb, mutedLevelDb, t));
            SetMainLevel(voice, Mathf.Lerp(mutedLevelDb, mainLevelDb, t));

            yield return null;
        }

        SetIntroLevel(voice, mutedLevelDb);
        SetMainLevel(voice, mainLevelDb);
        voice.introSource.Stop();
    }

    void StartMainVoice(Voice voice, float startDb)
    {
        if (!voice.mainSource.isPlaying)
        {
            SetMainLevel(voice, startDb);
            voice.mainSource.Play();
        }
    }

    IEnumerator AnimateMainVoice(Voice voice, float targetPan, float targetDb, float duration)
    {
        if (voice == null) yield break;

        float startPan = voice.mainSource.panStereo;
        float startDb = GetMainLevel(voice);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            voice.mainSource.panStereo = Mathf.Lerp(startPan, targetPan, t);
            voice.introSource.panStereo = voice.mainSource.panStereo;
            SetMainLevel(voice, Mathf.Lerp(startDb, targetDb, t));

            yield return null;
        }

        voice.mainSource.panStereo = targetPan;
        voice.introSource.panStereo = targetPan;
        SetMainLevel(voice, targetDb);
    }

    void DestroyVoice(Voice voice)
    {
        if (voice == null) return;

        SetMainLevel(voice, mutedLevelDb);
        SetIntroLevel(voice, mutedLevelDb);

        if (voice.mainSource != null)
            Destroy(voice.mainSource.gameObject);

        if (voice.introSource != null)
            Destroy(voice.introSource.gameObject);
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

    void SetIntroLevel(Voice voice, float db)
    {
        if (SetMixerLevel(GetParameter(introVolumeParameters, voice.slot), db))
        {
            if (voice.introSource != null)
            {
                voice.introSource.volume = 1f;
            }
        }
        else if (voice.introSource != null)
        {
            voice.introSource.volume = DbToLinear(db);
        }
    }

    float GetMainLevel(Voice voice)
    {
        return voice.mainDb;
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
        public AudioSource introSource;
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
                data[i] = Mathf.Sin(phase) * 0.5f;
                phase += 2f * Mathf.PI * frequency / sampleRate;

                if (phase > 2f * Mathf.PI)
                {
                    phase -= 2f * Mathf.PI;
                }
            }
        }
    }
}
