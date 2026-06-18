using UnityEngine;
using Vuforia;

public class ScanDebug : MonoBehaviour
{
    [SerializeField] bool hideSceneCardVisual = true;

    ObserverBehaviour observer;

    bool isTracked = false;
    bool warnedMissingObserver = false;

    int lastDiscreteAngle = -1;
    int observedCardID = -1;
    UIManager uiManager;

    void Start()
    {
        observer = GetComponent<ObserverBehaviour>();

        if (observer == null)
        {
            observer = GetComponentInParent<ObserverBehaviour>();
        }

        if (observer == null)
        {
            observer = GetComponentInChildren<ObserverBehaviour>();
        }

        if (observer != null)
        {
            observedCardID = ParseCardID(observer.TargetName);
            HideSceneCardVisuals(observer.transform);
        }
        else
        {
            HideSceneCardVisuals(transform);
            WarnMissingObserver();
        }
    }

    void Update()
    {
        if (observer == null)
        {
            WarnMissingObserver();
            return;
        }

        var status = observer.TargetStatus.Status;

        if (IsTrackedStatus(status))
        {
            if (!isTracked)
            {
                isTracked = true;
                observedCardID = ParseCardID(observer.TargetName);

                if (ShouldUpdateSection3())
                {
                    if (s3_CardManager.Instance != null)
                    {
                        s3_CardManager.Instance.OnCardEnter(observer.TargetName);
                    }
                    else
                    {
                        Debug.LogError("No s3_CardManager found in the scene.");
                    }
                }

                Debug.Log("ENTER TRACKED: " + observer.TargetName);

                if (ShouldUpdateSection2(observedCardID))
                {
                    ShowSection2Wheel(observedCardID);
                }
            }

            // original angle
            float rawAngle = observer.transform.eulerAngles.y;
            float correctedAngle = (rawAngle - 90f) % 360f;
            int discreteAngle = Mathf.RoundToInt(correctedAngle / 90f) * 90;

            if (discreteAngle < 0)
                discreteAngle += 360;

            if (discreteAngle >= 360)
                discreteAngle = 0;

            if (discreteAngle != lastDiscreteAngle)
            {
                lastDiscreteAngle = discreteAngle;

                Debug.Log(
                    $"Card: {observer.TargetName} | Raw: {rawAngle:F1} | Discrete: {discreteAngle}"
                );
            }

            if (ShouldUpdateSection2(observedCardID))
            {
                UpdateSection2Wheel(observedCardID, correctedAngle, discreteAngle);
            }
        }
        else
        {
            if (isTracked)
            {
                isTracked = false;

                if (ShouldUpdateSection3())
                {
                    if (s3_CardManager.Instance != null)
                    {
                        s3_CardManager.Instance.OnCardExit(observer.TargetName);
                    }
                    else
                    {
                        Debug.LogError("No s3_CardManager found in the scene.");
                    }
                }

                Debug.Log("LOST: " + observer.TargetName);

                if (ShouldUpdateSection2(observedCardID) && s2_UIManager.Instance != null)
                {
                    s2_UIManager.Instance.ShowUnknownWheel();
                }
            }
        }
    }

    bool IsTrackedStatus(Status status)
    {
        return status == Status.TRACKED
            || status == Status.EXTENDED_TRACKED
            || status == Status.LIMITED;
    }

    bool ShouldUpdateSection2(int cardID)
    {
        return IsSection2Card(cardID) && GetCurrentSection() == 2;
    }

    bool ShouldUpdateSection3()
    {
        return GetCurrentSection() == 3;
    }

    int GetCurrentSection()
    {
        if (uiManager == null)
        {
            uiManager = UIManager.Instance;
        }

        return uiManager != null ? uiManager.CurrentSection : 0;
    }

    void WarnMissingObserver()
    {
        if (warnedMissingObserver)
        {
            return;
        }

        warnedMissingObserver = true;
        Debug.LogWarning($"ScanDebug on {name} cannot find a Vuforia ObserverBehaviour.");
    }

    void HideSceneCardVisuals(Transform root)
    {
        if (!hideSceneCardVisual || root == null || root.GetComponentInParent<Canvas>() != null)
        {
            return;
        }

        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            renderer.enabled = false;
        }

        foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = false;
        }

        foreach (Collider2D collider in root.GetComponentsInChildren<Collider2D>(true))
        {
            collider.enabled = false;
        }
    }

    int ParseCardID(string targetName)
    {
        if (string.IsNullOrEmpty(targetName))
        {
            return -1;
        }

        int value = 0;
        bool hasDigit = false;

        for (int i = 0; i < targetName.Length; i++)
        {
            char c = targetName[i];

            if (c < '0' || c > '9')
            {
                break;
            }

            hasDigit = true;
            value = value * 10 + (c - '0');
        }

        return hasDigit ? value : -1;
    }

    bool IsSection2Card(int cardID)
    {
        return cardID >= 5 && cardID <= 8;
    }

    void ShowSection2Wheel(int cardID)
    {
        if (s2_UIManager.Instance != null)
        {
            s2_UIManager.Instance.ShowWheelByCardID(cardID);
        }

        if (s2_WheelUIManager.Instance != null)
        {
            s2_WheelUIManager.Instance.SetActiveWheelByCardID(cardID);
        }
    }

    void UpdateSection2Wheel(int cardID, float continuousAngle, float discreteAngle)
    {
        bool isMarkedAnswer = false;

        if (s2_interactiveManager.Instance != null)
        {
            isMarkedAnswer = s2_interactiveManager.Instance.EvaluateIsMark(cardID, discreteAngle);
        }

        if (s2_WheelUIManager.Instance != null)
        {
            s2_WheelUIManager.Instance.UpdateWheel(continuousAngle, discreteAngle, isMarkedAnswer);
        }

        if (s2_HapticManager.Instance != null)
        {
            s2_HapticManager.Instance.UpdateHaptic(cardID, continuousAngle, discreteAngle, isMarkedAnswer);
        }
    }
}
