using UnityEngine;

public class s2_WheelUIManager : MonoBehaviour
{
    public static s2_WheelUIManager Instance;

    [Header("Wheel Objects")]
    public Transform Wheel05;
    public Transform Wheel06;
    public Transform Wheel07;
    public Transform Wheel08;
    public Transform activeWheel;

    [Header("Rotation")]
    public float normalSpeed = 8f;
    public float reboundSpeed = 12f;
    public float stoppedThreshold = 0.1f;

    public bool IsCountering { get; private set; }
    public bool IsRebounding { get; private set; }

    private float previousContinuousAngle;
    private int counteringFrameCount;
    private bool hasPreviousAngle;

    void Awake()
    {
        Instance = this;
    }

    public void SetActiveWheelByCardID(int cardID)
    {
        if (cardID == 5) activeWheel = Wheel05;
        else if (cardID == 6) activeWheel = Wheel06;
        else if (cardID == 7) activeWheel = Wheel07;
        else if (cardID == 8) activeWheel = Wheel08;
        else activeWheel = null;

        ResetMotionState();
    }

    public void SetActiveWheel(Transform wheel)
    {
        activeWheel = wheel;
        ResetMotionState();
    }

    public bool UpdateWheel(float continuousAngle, float discreteAngle, bool isMarkedAnswer)
    {
        if (activeWheel == null)
        {
            ResetMotionTracking(continuousAngle);
            return false;
        }

        float frameDelta = GetFrameAngleDelta(continuousAngle);
        bool cardIsStopped = hasPreviousAngle && frameDelta <= stoppedThreshold;

        if (isMarkedAnswer)
        {
            UpdateCounteringState(frameDelta);
        }
        else
        {
            IsCountering = false;
            counteringFrameCount = 0;
            IsRebounding = false;
        }

        float currentZ = activeWheel.localEulerAngles.z;
        float targetAngle;
        float speed;

        if (!isMarkedAnswer)
        {
            targetAngle = continuousAngle;
            speed = normalSpeed;
        }
        else if (!IsRebounding && !cardIsStopped)
        {
            targetAngle = continuousAngle;
            speed = normalSpeed * 0.2f;
        }
        else
        {
            IsRebounding = true;
            targetAngle = discreteAngle;
            speed = reboundSpeed;
        }

        float nextZ = Mathf.LerpAngle(currentZ, targetAngle, Time.deltaTime * speed);
        Vector3 euler = activeWheel.localEulerAngles;
        euler.z = nextZ;
        activeWheel.localEulerAngles = euler;

        previousContinuousAngle = continuousAngle;
        hasPreviousAngle = true;

        return IsCountering;
    }

    private float GetFrameAngleDelta(float continuousAngle)
    {
        if (!hasPreviousAngle)
        {
            return 0f;
        }

        return Mathf.Abs(Mathf.DeltaAngle(previousContinuousAngle, continuousAngle));
    }

    private void UpdateCounteringState(float frameDelta)
    {
        if (frameDelta > stoppedThreshold)
        {
            counteringFrameCount++;
        }
        else
        {
            counteringFrameCount = 0;
        }

        IsCountering = counteringFrameCount >= 3;
    }

    private void ResetMotionState()
    {
        hasPreviousAngle = false;
        counteringFrameCount = 0;
        IsCountering = false;
        IsRebounding = false;
    }

    private void ResetMotionTracking(float continuousAngle)
    {
        previousContinuousAngle = continuousAngle;
        hasPreviousAngle = true;
        counteringFrameCount = 0;
        IsCountering = false;
        IsRebounding = false;
    }
}
