using UnityEngine;

public class s2_UIManager : MonoBehaviour
{
    public static s2_UIManager Instance;

    public GameObject[] Section2_Wheel_05;
    public GameObject[] Section2_Wheel_06;
    public GameObject[] Section2_Wheel_07;
    public GameObject[] Section2_Wheel_08;
    public GameObject[] Section2_EmptyWheel;

    void Awake()
    {
        Instance = this;
    }

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
                GameObject controlledObject = ResolveControlledObject(obj);
                controlledObject.SetActive(state);
            }
        }
    }

    private GameObject ResolveControlledObject(GameObject obj)
    {
        Transform current = obj.transform;

        while (current != null)
        {
            if (IsSection2Root(current.name))
            {
                return current.gameObject;
            }

            current = current.parent;
        }

        return obj;
    }

    private bool IsSection2Root(string objectName)
    {
        string normalizedName = objectName.ToLowerInvariant().Replace("_", "-");

        return normalizedName == "05-employment"
            || normalizedName == "06-get-married"
            || normalizedName == "07-learning"
            || normalizedName == "08-travel"
            || normalizedName == "emptywheel";
    }
}
