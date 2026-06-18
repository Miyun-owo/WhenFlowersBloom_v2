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
    private Transform activeWheelVisual;
    private readonly System.Collections.Generic.List<WheelPart> activeWheelParts =
        new System.Collections.Generic.List<WheelPart>();

    private Vector3 activePivotLocalPosition;
    private float lastAppliedGroupAngle;
    private bool hasAppliedGroupAngle;

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

        activeWheelVisual = ResolveWheelVisual(activeWheel, cardID);
        CaptureWheelParts();
        ResetMotionState();
    }

    public void SetActiveWheel(Transform wheel)
    {
        activeWheel = wheel;
        activeWheelVisual = ResolveWheelVisual(activeWheel, -1);
        CaptureWheelParts();
        ResetMotionState();
    }

    public bool UpdateWheel(float continuousAngle, float discreteAngle, bool isMarkedAnswer)
    {
        if (activeWheelVisual == null)
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

        float currentZ = hasAppliedGroupAngle ? lastAppliedGroupAngle : activeWheelVisual.localEulerAngles.z;
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
        ApplyWheelGroupRotation(nextZ);

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
        lastAppliedGroupAngle = activeWheelVisual != null ? activeWheelVisual.localEulerAngles.z : 0f;
        hasAppliedGroupAngle = activeWheelVisual != null;
    }

    private void ResetMotionTracking(float continuousAngle)
    {
        previousContinuousAngle = continuousAngle;
        hasPreviousAngle = true;
        counteringFrameCount = 0;
        IsCountering = false;
        IsRebounding = false;
    }

    private void CaptureWheelParts()
    {
        activeWheelParts.Clear();

        if (activeWheel == null || activeWheelVisual == null)
        {
            return;
        }

        activePivotLocalPosition = activeWheel.InverseTransformPoint(activeWheelVisual.position);

        foreach (Transform child in activeWheel)
        {
            if (!ShouldRotateWithWheel(child))
            {
                continue;
            }

            activeWheelParts.Add(new WheelPart
            {
                transform = child,
                baseLocalPosition = child.localPosition,
                baseLocalRotation = child.localRotation
            });
        }
    }

    private bool ShouldRotateWithWheel(Transform child)
    {
        if (child == null)
        {
            return false;
        }

        string childName = child.name;

        if (child == activeWheelVisual)
        {
            return true;
        }

        return childName.StartsWith("000_")
            || childName.StartsWith("090_")
            || childName.StartsWith("180_")
            || childName.StartsWith("270_");
    }

    private void ApplyWheelGroupRotation(float angle)
    {
        if (activeWheelParts.Count == 0)
        {
            Vector3 euler = activeWheelVisual.localEulerAngles;
            euler.z = angle;
            activeWheelVisual.localEulerAngles = euler;
            return;
        }

        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        foreach (WheelPart part in activeWheelParts)
        {
            if (part.transform == null)
            {
                continue;
            }

            Vector3 offset = part.baseLocalPosition - activePivotLocalPosition;
            part.transform.localPosition = activePivotLocalPosition + rotation * offset;
            part.transform.localRotation = rotation * part.baseLocalRotation;
        }

        lastAppliedGroupAngle = angle;
        hasAppliedGroupAngle = true;
    }

    private Transform ResolveWheelVisual(Transform wheelRoot, int cardID)
    {
        if (wheelRoot == null)
        {
            return null;
        }

        string childName = GetWheelChildName(cardID);
        if (!string.IsNullOrEmpty(childName))
        {
            Transform visual = wheelRoot.Find(childName);
            if (visual != null)
            {
                return visual;
            }
        }

        foreach (Transform child in wheelRoot)
        {
            if (child.name.EndsWith("_Wheel"))
            {
                return child;
            }
        }

        return wheelRoot;
    }

    private string GetWheelChildName(int cardID)
    {
        if (cardID == 5) return "05_Wheel";
        if (cardID == 6) return "06_Wheel";
        if (cardID == 7) return "07_Wheel";
        if (cardID == 8) return "08_Wheel";

        return null;
    }

    private struct WheelPart
    {
        public Transform transform;
        public Vector3 baseLocalPosition;
        public Quaternion baseLocalRotation;
    }
}
