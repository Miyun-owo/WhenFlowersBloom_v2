using UnityEngine;
using Vuforia;

public class ScanDebug : MonoBehaviour
{
    ObserverBehaviour observer;

    bool isTracked = false;
    bool warnedMissingObserver = false;

    int lastDiscreteAngle = -1;
    int observedCardID = -1;

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
        }
        else
        {
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

                if (s3_CardManager.Instance != null)
                {
                    s3_CardManager.Instance.OnCardEnter(observer.TargetName);
                }
                else
                {
                    Debug.LogError("No s3_CardManager found in the scene.");
                }

                Debug.Log("ENTER TRACKED: " + observer.TargetName);

                if (IsSection2Card(observedCardID))
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

            if (IsSection2Card(observedCardID))
            {
                UpdateSection2Wheel(observedCardID, correctedAngle, discreteAngle);
            }
        }
        else
        {
            if (isTracked)
            {
                isTracked = false;

                if (s3_CardManager.Instance != null)
                {
                    s3_CardManager.Instance.OnCardExit(observer.TargetName);
                }
                else
                {
                    Debug.LogError("No s3_CardManager found in the scene.");
                }

                Debug.Log("LOST: " + observer.TargetName);

                if (IsSection2Card(observedCardID) && s2_UIManager.Instance != null)
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

    void WarnMissingObserver()
    {
        if (warnedMissingObserver)
        {
            return;
        }

        warnedMissingObserver = true;
        Debug.LogWarning($"ScanDebug on {name} cannot find a Vuforia ObserverBehaviour.");
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
