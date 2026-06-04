using UnityEngine;

public class SpaceManager_1 : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;
    public AudioSource targetSource;
    public AudioSource[] otherSources;

    [Header("Explore Settings")]
    [Range(1f, 5f)] public float focusPower = 2f;

    public float targetMinVolume = 0.3f;
    public float targetMaxVolume = 0.6f;

    public float otherMinVolume = 0.2f;
    public float otherMaxVolume = 0.4f;

    [Header("Smoothing")]
    public float smoothTime = 0.2f;
    private float targetVel;
    private float currentTargetVolume;

    [Header("Confirm Settings")]
    public float confirmBoost = 1.5f;

    private bool isTracking = false;
    private bool isConfirmed = false;

    public float CurrentDot { get; private set; }

    public void StartTracking()
    {
        isTracking = true;
        isConfirmed = false;
    }

    public void StopTracking()
    {
        isTracking = false;
    }

    void Update()
    {
        if (!isTracking || isConfirmed) return;

        UpdateAudio();
    }

    void UpdateAudio()
    {
        Vector3 camForward = playerCamera.forward;
        Vector3 dirToTarget =
            (targetSource.transform.position - playerCamera.position).normalized;

        float dot = Vector3.Dot(camForward, dirToTarget);
        CurrentDot = dot;

        float focus = Mathf.Pow(Mathf.Clamp01(dot), focusPower);

        // Target volume + smoothing
        float targetVol = Mathf.Lerp(targetMinVolume, targetMaxVolume, focus);

        if (dot < 0.1f)
            targetVol = targetMinVolume;

        currentTargetVolume = Mathf.SmoothDamp(
            currentTargetVolume,
            targetVol,
            ref targetVel,
            smoothTime
        );

        targetSource.volume = currentTargetVolume;

        //Other sources
        float otherVol = Mathf.Lerp(otherMaxVolume, otherMinVolume, focus);

        foreach (var src in otherSources)
        {
            src.volume = Mathf.Lerp(src.volume, otherVol, Time.deltaTime * 8f);
        }

        Debug.Log($"dot: {CurrentDot:F2}");
    }

    public void Confirm(bool isCorrect)
    {
        isConfirmed = true;

        if (isCorrect)
        {
            targetSource.volume = 1f * confirmBoost;

            foreach (var src in otherSources)
                src.volume = 0f;
        }
        else
        {
            foreach (var src in otherSources)
                src.volume *= 0.5f;

            targetSource.volume *= 0.5f;
        }
    }

    public void ResetState()
    {
        isConfirmed = false;
    }
}
