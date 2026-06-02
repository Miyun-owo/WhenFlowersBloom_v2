using UnityEngine;
using Vuforia;

public class ScanDebug : MonoBehaviour
{
    ObserverBehaviour observer;

    bool isTracked = false;

    int lastDiscreteAngle = -1;

    void Start()
    {
        observer = GetComponent<ObserverBehaviour>();
    }

    void Update()
    {
        if (observer == null) return;

        var status = observer.TargetStatus.Status;

        if (status == Status.TRACKED)
        {
            if (!isTracked)
            {
                isTracked = true;

                if (s3_CardManager.Instance != null)
                {
                    s3_CardManager.Instance.OnCardEnter(observer.TargetName);
                }
                else
                {
                    Debug.LogError("No s3_CardManager found in the scene.");
                }

                Debug.Log("ENTER TRACKED: " + observer.TargetName);
            }

            // original angle
            float rawAngle = transform.eulerAngles.y;
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
            }
        }
    }
}
