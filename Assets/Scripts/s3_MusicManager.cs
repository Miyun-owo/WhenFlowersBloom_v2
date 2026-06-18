using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class s3_MusicManager : MonoBehaviour
{
    public static s3_MusicManager Instance;

    public enum RelationType { Harmonious, Conflicting, Ambiguous }

    [Header("Granular Source")]
    public AudioClip[] sourceClips;
    [Range(0f, 1f)] public float masterGain = 0.45f;
    [Range(0f, 1f)] public float defaultCardDistance = 0.5f;

    [Header("Output")]
    public AudioMixerGroup outputGroup;
    public int outputChannels = 2;

    const int MaxGrains = 64;
    const float TwoPi = 6.28318530718f;

    AudioSource audioSource;
    Grain[] grainPool;

    float[] sourceBuffer;
    int[] sourceStartFrames;
    int[] sourceFrameCounts;
    int sourceClipCount;
    int totalSourceFrames;

    int sampleRate;
    int runtimeChannels;
    float samplesUntilNextGrain;
    float sourceScanPosition;
    uint randomState = 0x12345678u;
    bool isPlaying;

    volatile float grainDurationSeconds = 0.1f;
    volatile float grainDensity = 22f;
    volatile float pitchScatter;
    volatile float positionJitterSeconds;
    volatile int relationMode = (int)RelationType.Harmonious;

    readonly List<string> activeCardNames = new List<string>(2);

    struct Grain
    {
        public bool isActive;
        public float sourcePosition;
        public float pitchIncrement;
        public int sourceStartFrame;
        public int sourceFrameCount;
        public int ageFrames;
        public int durationFrames;
        public float amplitude;
    }

    void Awake()
    {
        Instance = this;

        sampleRate = AudioSettings.outputSampleRate;
        runtimeChannels = Mathf.Max(1, outputChannels);
        grainPool = new Grain[MaxGrains];

        audioSource = GetComponent<AudioSource>();

        if ((sourceClips == null || sourceClips.Length == 0) && audioSource.clip != null)
        {
            sourceClips = new AudioClip[] { audioSource.clip };
        }

        PreloadSourceBuffers();

        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 1f;
        audioSource.outputAudioMixerGroup = outputGroup;
        audioSource.clip = AudioClip.Create(
            "S3_GranularRelationSynth",
            sampleRate,
            runtimeChannels,
            sampleRate,
            true,
            OnAudioRead,
            OnAudioSetPosition
        );
    }

    public void PlayCards(IReadOnlyList<NoteCardData> cards)
    {
        activeCardNames.Clear();

        if (cards == null || cards.Count == 0)
        {
            StopAll();
            return;
        }

        for (int i = 0; i < cards.Count && i < 2; i++)
        {
            if (cards[i] != null && !string.IsNullOrEmpty(cards[i].cardName))
            {
                activeCardNames.Add(cards[i].cardName);
            }
        }

        if (activeCardNames.Count >= 2)
        {
            ApplyGranularRelation(
                GetRelation(activeCardNames[0], activeCardNames[1]),
                defaultCardDistance
            );
        }
        else
        {
            ApplyGranularRelation(RelationType.Harmonious, defaultCardDistance);
        }

        isPlaying = true;

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void StopAll()
    {
        isPlaying = false;

        for (int i = 0; i < MaxGrains; i++)
        {
            grainPool[i].isActive = false;
        }

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public void ApplyGranularRelation(RelationType type, float cardDistance)
    {
        float distance = Mathf.Clamp01(cardDistance);
        float closeness = 1f - distance;

        relationMode = (int)type;

        switch (type)
        {
            case RelationType.Conflicting:
                grainDurationSeconds = Mathf.Lerp(0.03f, 0.015f, closeness);
                pitchScatter = 0.85f;
                grainDensity = Mathf.Lerp(24f, 120f, closeness);
                positionJitterSeconds = 0.012f;
                break;

            case RelationType.Harmonious:
                grainDurationSeconds = Mathf.Lerp(0.08f, 0.15f, closeness);
                pitchScatter = 0f;
                grainDensity = Mathf.Lerp(18f, 42f, closeness);
                positionJitterSeconds = 0f;
                break;

            case RelationType.Ambiguous:
                grainDurationSeconds = Mathf.Lerp(0.045f, 0.09f, closeness);
                pitchScatter = 0.28f;
                grainDensity = Mathf.Lerp(16f, 56f, closeness);
                positionJitterSeconds = Mathf.Lerp(0.02f, 0.11f, closeness);
                break;
        }
    }

    public void ApplyCurrentCardDistance(float cardDistance)
    {
        RelationType type = (RelationType)relationMode;

        if (activeCardNames.Count >= 2)
        {
            type = GetRelation(activeCardNames[0], activeCardNames[1]);
        }

        ApplyGranularRelation(type, cardDistance);
    }

    public RelationType GetRelation(string firstCardName, string secondCardName)
    {
        CardKind a = ParseCardKind(firstCardName);
        CardKind b = ParseCardKind(secondCardName);

        if (IsCharacter(a) && IsEvent(b))
        {
            return GetCharacterEventRelation(a, b);
        }

        if (IsEvent(a) && IsCharacter(b))
        {
            return GetCharacterEventRelation(b, a);
        }

        if (IsCharacter(a) && IsCharacter(b))
        {
            return GetCharacterCharacterRelation(a, b);
        }

        return RelationType.Ambiguous;
    }

    void PreloadSourceBuffers()
    {
        sourceClipCount = sourceClips != null ? sourceClips.Length : 0;

        if (sourceClipCount == 0)
        {
            sourceBuffer = new float[1];
            sourceStartFrames = new int[1];
            sourceFrameCounts = new int[1];
            totalSourceFrames = 1;
            return;
        }

        sourceStartFrames = new int[sourceClipCount];
        sourceFrameCounts = new int[sourceClipCount];
        totalSourceFrames = 0;

        for (int i = 0; i < sourceClipCount; i++)
        {
            AudioClip clip = sourceClips[i];
            sourceStartFrames[i] = totalSourceFrames;

            int frames = clip != null ? clip.samples : 0;
            sourceFrameCounts[i] = frames;
            totalSourceFrames += frames;
        }

        if (totalSourceFrames <= 0)
        {
            sourceBuffer = new float[1];
            totalSourceFrames = 1;
            return;
        }

        sourceBuffer = new float[totalSourceFrames];

        for (int clipIndex = 0; clipIndex < sourceClipCount; clipIndex++)
        {
            AudioClip clip = sourceClips[clipIndex];
            if (clip == null || sourceFrameCounts[clipIndex] <= 0)
            {
                continue;
            }

            int frames = clip.samples;
            int channels = clip.channels;
            clip.LoadAudioData();
            float[] clipData = new float[frames * channels];
            clip.GetData(clipData, 0);

            int writeOffset = sourceStartFrames[clipIndex];

            for (int frame = 0; frame < frames; frame++)
            {
                float sum = 0f;
                int baseIndex = frame * channels;

                for (int ch = 0; ch < channels; ch++)
                {
                    sum += clipData[baseIndex + ch];
                }

                sourceBuffer[writeOffset + frame] = sum / channels;
            }
        }
    }

    void OnAudioRead(float[] data)
    {
        if (!isPlaying || totalSourceFrames <= 1)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = 0f;
            }

            return;
        }

        int channels = runtimeChannels;
        int frames = data.Length / channels;

        for (int frame = 0; frame < frames; frame++)
        {
            TrySpawnGrainsForFrame();

            float sample = 0f;

            for (int i = 0; i < MaxGrains; i++)
            {
                if (grainPool[i].isActive)
                {
                    sample += ProcessGrain(ref grainPool[i]);
                }
            }

            sample *= masterGain;

            int outputIndex = frame * channels;
            for (int ch = 0; ch < channels; ch++)
            {
                data[outputIndex + ch] = sample;
            }
        }
    }

    void TrySpawnGrainsForFrame()
    {
        samplesUntilNextGrain -= 1f;

        float density = grainDensity;
        if (density < 1f)
        {
            density = 1f;
        }

        float interval = sampleRate / density;

        while (samplesUntilNextGrain <= 0f)
        {
            SpawnGrain();
            samplesUntilNextGrain += interval;
        }
    }

    void SpawnGrain()
    {
        int poolIndex = -1;

        for (int i = 0; i < MaxGrains; i++)
        {
            if (!grainPool[i].isActive)
            {
                poolIndex = i;
                break;
            }
        }

        if (poolIndex < 0)
        {
            return;
        }

        int clipIndex = NextRandomInt(sourceClipCount);
        int clipStart = sourceStartFrames[clipIndex];
        int clipFrames = sourceFrameCounts[clipIndex];

        if (clipFrames <= 1)
        {
            return;
        }

        int durationFrames = (int)(grainDurationSeconds * sampleRate);
        if (durationFrames < 8)
        {
            durationFrames = 8;
        }

        float jitterFrames = positionJitterSeconds * sampleRate;
        float jitter = NextRandomSigned() * jitterFrames;

        sourceScanPosition += durationFrames * 0.37f;
        while (sourceScanPosition >= clipFrames)
        {
            sourceScanPosition -= clipFrames;
        }

        float startPosition = sourceScanPosition + jitter;
        while (startPosition < 0f)
        {
            startPosition += clipFrames;
        }

        while (startPosition >= clipFrames)
        {
            startPosition -= clipFrames;
        }

        float pitch = 1f + NextRandomSigned() * pitchScatter;
        if (pitch < 0.25f)
        {
            pitch = 0.25f;
        }

        grainPool[poolIndex].isActive = true;
        grainPool[poolIndex].sourcePosition = startPosition;
        grainPool[poolIndex].pitchIncrement = pitch;
        grainPool[poolIndex].sourceStartFrame = clipStart;
        grainPool[poolIndex].sourceFrameCount = clipFrames;
        grainPool[poolIndex].ageFrames = 0;
        grainPool[poolIndex].durationFrames = durationFrames;
        grainPool[poolIndex].amplitude = 1f / Mathf.Sqrt(MaxGrains);
    }

    float ProcessGrain(ref Grain grain)
    {
        if (grain.ageFrames >= grain.durationFrames)
        {
            grain.isActive = false;
            return 0f;
        }

        int localFrame0 = (int)grain.sourcePosition;
        int localFrame1 = localFrame0 + 1;

        if (localFrame0 >= grain.sourceFrameCount)
        {
            localFrame0 -= grain.sourceFrameCount;
        }

        if (localFrame1 >= grain.sourceFrameCount)
        {
            localFrame1 -= grain.sourceFrameCount;
        }

        float fraction = grain.sourcePosition - (int)grain.sourcePosition;
        float a = sourceBuffer[grain.sourceStartFrame + localFrame0];
        float b = sourceBuffer[grain.sourceStartFrame + localFrame1];
        float sourceSample = a + (b - a) * fraction;

        float phase = grain.durationFrames > 1
            ? (float)grain.ageFrames / (grain.durationFrames - 1)
            : 1f;
        float hanning = 0.5f - 0.5f * Mathf.Cos(TwoPi * phase);
        float output = sourceSample * hanning * grain.amplitude;

        grain.sourcePosition += grain.pitchIncrement;
        while (grain.sourcePosition >= grain.sourceFrameCount)
        {
            grain.sourcePosition -= grain.sourceFrameCount;
        }

        grain.ageFrames++;

        if (grain.ageFrames >= grain.durationFrames)
        {
            grain.isActive = false;
        }

        return output;
    }

    void OnAudioSetPosition(int newPosition)
    {
        samplesUntilNextGrain = 0f;
        sourceScanPosition = newPosition % Mathf.Max(1, totalSourceFrames);
    }

    RelationType GetCharacterEventRelation(CardKind character, CardKind eventCard)
    {
        if (character == CardKind.HuiIng)
        {
            if (eventCard == CardKind.GetMarried) return RelationType.Conflicting;
            if (eventCard == CardKind.Learning) return RelationType.Ambiguous;
            return RelationType.Harmonious;
        }

        if (character == CardKind.SuiEn)
        {
            if (eventCard == CardKind.GetMarried) return RelationType.Conflicting;
            if (eventCard == CardKind.Learning) return RelationType.Harmonious;
            if (eventCard == CardKind.Travel) return RelationType.Ambiguous;
            return RelationType.Ambiguous;
        }

        if (character == CardKind.ChuYin)
        {
            if (eventCard == CardKind.GetMarried || eventCard == CardKind.Travel)
            {
                return RelationType.Harmonious;
            }

            return RelationType.Ambiguous;
        }

        if (eventCard == CardKind.Learning || eventCard == CardKind.Employment)
        {
            return RelationType.Ambiguous;
        }

        return RelationType.Harmonious;
    }

    RelationType GetCharacterCharacterRelation(CardKind a, CardKind b)
    {
        if (a == b)
        {
            return RelationType.Harmonious;
        }

        if (a > b)
        {
            CardKind temp = a;
            a = b;
            b = temp;
        }

        if (a == CardKind.HuiIng && b == CardKind.MissHsieh) return RelationType.Ambiguous;
        if (a == CardKind.SuiEn && b == CardKind.MissHsieh) return RelationType.Ambiguous;
        if (a == CardKind.ChuYin && b == CardKind.MissHsieh) return RelationType.Ambiguous;

        return RelationType.Harmonious;
    }

    bool IsCharacter(CardKind kind)
    {
        return kind == CardKind.HuiIng
            || kind == CardKind.SuiEn
            || kind == CardKind.ChuYin
            || kind == CardKind.MissHsieh;
    }

    bool IsEvent(CardKind kind)
    {
        return kind == CardKind.GetMarried
            || kind == CardKind.Learning
            || kind == CardKind.Employment
            || kind == CardKind.Travel;
    }

    CardKind ParseCardKind(string cardName)
    {
        if (string.IsNullOrEmpty(cardName)) return CardKind.Unknown;

        string normalized = cardName.ToLowerInvariant();

        if (normalized.Contains("hui-ing")) return CardKind.HuiIng;
        if (normalized.Contains("sui-en")) return CardKind.SuiEn;
        if (normalized.Contains("chu-yin") || normalized.Contains("chu_yin")) return CardKind.ChuYin;
        if (normalized.Contains("miss-hsieh") || normalized.Contains("miss_hsieh")) return CardKind.MissHsieh;
        if (normalized.Contains("get-married") || normalized.Contains("get_married")) return CardKind.GetMarried;
        if (normalized.Contains("learning")) return CardKind.Learning;
        if (normalized.Contains("employment")) return CardKind.Employment;
        if (normalized.Contains("travel")) return CardKind.Travel;

        return CardKind.Unknown;
    }

    uint NextRandomUInt()
    {
        uint x = randomState;
        x ^= x << 13;
        x ^= x >> 17;
        x ^= x << 5;
        randomState = x;
        return x;
    }

    int NextRandomInt(int maxExclusive)
    {
        if (maxExclusive <= 1)
        {
            return 0;
        }

        return (int)(NextRandomUInt() % (uint)maxExclusive);
    }

    float NextRandom01()
    {
        return (NextRandomUInt() & 0x00FFFFFFu) * (1f / 16777215f);
    }

    float NextRandomSigned()
    {
        return NextRandom01() * 2f - 1f;
    }

    enum CardKind
    {
        Unknown,
        HuiIng,
        SuiEn,
        ChuYin,
        MissHsieh,
        GetMarried,
        Learning,
        Employment,
        Travel
    }
}
