using UnityEngine;

public class s2_UIManager : MonoBehaviour
{
    public GameObject[] Section2_Wheel_05;
    public GameObject[] Section2_Wheel_06;
    public GameObject[] Section2_Wheel_07;
    public GameObject[] Section2_Wheel_08;
    public GameObject[] Section2_EmptyWheel;

    public void ShowWheelByCardID(int cardID)
    {
        SetObjectsActive(Section2_Wheel_05, false);
        SetObjectsActive(Section2_Wheel_06, false);
        SetObjectsActive(Section2_Wheel_07, false);
        SetObjectsActive(Section2_Wheel_08, false);
        SetObjectsActive(Section2_EmptyWheel, false);

        if (cardID == 5)
        {
            SetObjectsActive(Section2_Wheel_05, true);
        }
        else if (cardID == 6)
        {
            SetObjectsActive(Section2_Wheel_06, true);
        }
        else if (cardID == 7)
        {
            SetObjectsActive(Section2_Wheel_07, true);
        }
        else if (cardID == 8)
        {
            SetObjectsActive(Section2_Wheel_08, true);
        }
        else
        {
            ShowUnknownWheel();
        }
    }

    public void ShowWheelByInteractiveResult(s2_interactiveManager interactiveManager)
    {
        if (interactiveManager == null || !interactiveManager.IsKnownCard)
        {
            ShowUnknownWheel();
            return;
        }

        ShowWheelByCardID(interactiveManager.CurrentCardID);
    }

    public void ShowUnknownWheel()
    {
        SetObjectsActive(Section2_Wheel_05, false);
        SetObjectsActive(Section2_Wheel_06, false);
        SetObjectsActive(Section2_Wheel_07, false);
        SetObjectsActive(Section2_Wheel_08, false);
        SetObjectsActive(Section2_EmptyWheel, true);
    }

    private void SetObjectsActive(GameObject[] objects, bool state)
    {
        if (objects == null) return;

        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                obj.SetActive(state);
            }
        }
    }
}
