using UnityEngine;
using Solo.MOST_IN_ONE;

public class s2_HapticManager : MonoBehaviour
{
    [Header("Marked Answer")]
    public float maxStrength = 100f;
    public float markedPulseDurationMs = 35f;
    public float markedPulseSharpness = 0.6f;
    public float markedPulseCooldown = 0.05f;

    [Header("Unmarked Answer")]
    public float boundaryTolerance = 5f;
    public float shortPulseStrength = 20f;
    public long shortPulseDurationMs = 35L;
    public long shortPulseDelayMs = 80L;

    private readonly float[] triggerAngles = { 45f, 135f, 225f, 315f };
    private readonly bool[] boundaryLocks = new bool[4];

    void Awake()
    {
        MOST_HapticFeedback.Prewarm();
    }

    void OnDisable()
    {
        MOST_HapticFeedback.Stop();
    }

    public void UpdateHaptic(int cardID, float continuousAngle, float discreteAngle, bool isMarkedAnswer)
    {
        continuousAngle = NormalizeAngle(continuousAngle);
        discreteAngle = NormalizeAngle(discreteAngle);

        if (isMarkedAnswer)
        {
            ResetBoundaryLocks();
            float x = Mathf.Abs(Mathf.DeltaAngle(discreteAngle, continuousAngle));

            if (x <= 45f)
            {
                float strength = Mathf.Clamp(0.015f * x * x, 0f, maxStrength);
                PlayStrengthPulse(strength);
            }

            return;
        }

        CheckBoundaryPulses(continuousAngle);
    }

    private void CheckBoundaryPulses(float continuousAngle)
    {
        for (int i = 0; i < triggerAngles.Length; i++)
        {
            bool isInside = Mathf.Abs(Mathf.DeltaAngle(triggerAngles[i], continuousAngle)) <= boundaryTolerance;

            if (isInside && !boundaryLocks[i])
            {
                boundaryLocks[i] = true;
                PlayDoubleShortPulse();
            }
            else if (!isInside)
            {
                boundaryLocks[i] = false;
            }
        }
    }

    private void PlayStrengthPulse(float strength)
    {
        float normalizedStrength = NormalizeStrength(strength);

        if (normalizedStrength <= 0f)
        {
            return;
        }

        AnimationCurve intensity = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.15f, normalizedStrength),
            new Keyframe(0.8f, normalizedStrength),
            new Keyframe(1f, 0f)
        );

        MOST_HapticFeedback.HapticCurve curve = new MOST_HapticFeedback.HapticCurve(
            intensity,
            Mathf.Max(MOST_HapticFeedback.MinCurveDurationMs, markedPulseDurationMs),
            markedPulseSharpness,
            4,
            GetPresetForStrength(strength),
            0f,
            MOST_HapticFeedback.DefaultAndroidMaxAmplitude
        );

        MOST_HapticFeedback.GenerateCurveWithCooldown(curve, markedPulseCooldown);
    }

    private void PlayDoubleShortPulse()
    {
        int androidStrength = StrengthToAndroidAmplitude(shortPulseStrength);

        MOST_HapticFeedback.CustomHapticPattern pattern = new MOST_HapticFeedback.CustomHapticPattern(
            new[]
            {
                new MOST_HapticFeedback.IOS_Haptic(MOST_HapticFeedback.HapticTypes.LightImpact, 0f),
                new MOST_HapticFeedback.IOS_Haptic(MOST_HapticFeedback.HapticTypes.LightImpact, shortPulseDelayMs)
            },
            new[]
            {
                new MOST_HapticFeedback.Android_Haptic(0L, shortPulseDurationMs, androidStrength),
                new MOST_HapticFeedback.Android_Haptic(shortPulseDelayMs, shortPulseDurationMs, androidStrength)
            }
        );

        MOST_HapticFeedback.GeneratePattern(pattern);
    }

    private int StrengthToAndroidAmplitude(float strength)
    {
        float normalizedStrength = NormalizeStrength(strength);
        return Mathf.Clamp(Mathf.RoundToInt(normalizedStrength * MOST_HapticFeedback.DefaultAndroidMaxAmplitude), 0, MOST_HapticFeedback.DefaultAndroidMaxAmplitude);
    }

    private MOST_HapticFeedback.HapticTypes GetPresetForStrength(float strength)
    {
        float normalizedStrength = NormalizeStrength(strength);

        if (normalizedStrength < 0.25f)
        {
            return MOST_HapticFeedback.HapticTypes.LightImpact;
        }

        if (normalizedStrength < 0.65f)
        {
            return MOST_HapticFeedback.HapticTypes.MediumImpact;
        }

        return MOST_HapticFeedback.HapticTypes.HeavyImpact;
    }

    private float NormalizeStrength(float strength)
    {
        return Mathf.Clamp01(strength / Mathf.Max(0.001f, maxStrength));
    }

    private void ResetBoundaryLocks()
    {
        for (int i = 0; i < boundaryLocks.Length; i++)
        {
            boundaryLocks[i] = false;
        }
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360f;

        if (angle < 0f)
        {
            angle += 360f;
        }

        return angle;
    }
}
