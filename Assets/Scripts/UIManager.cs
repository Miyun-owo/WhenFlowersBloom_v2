using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    public GameObject[] MenuList;
    public GameObject[] Active1_Common;
    public GameObject[] Section1_Quiz;
    public GameObject[] Section1_Ans;
    public GameObject[] Section2;
    public GameObject[] Section3;
    void SetObjectsActive(GameObject[] objects, bool state)
    {
        foreach (GameObject obj in objects)
        {
            obj.SetActive(state);
        }
    }
    public void Homepage()
    {
        SetObjectsActive(MenuList, true);
        SetObjectsActive(Active1_Common, false);
        SetObjectsActive(Section1_Quiz, false);
        SetObjectsActive(Section1_Ans, false);
        SetObjectsActive(Section2, false);
        SetObjectsActive(Section3, false);
    }

    public void Active1()
    {
        SetObjectsActive(MenuList, false);
        SetObjectsActive(Active1_Common, true);
        SetObjectsActive(Section1_Quiz, true);
        SetObjectsActive(Section1_Ans, false);
        SetObjectsActive(Section2, false);
        SetObjectsActive(Section3, false);
    }

    public void Active1_1()
    {
        SetObjectsActive(MenuList, false);
        SetObjectsActive(Active1_Common, true);
        SetObjectsActive(Section1_Quiz, false);
        SetObjectsActive(Section1_Ans, true);
        SetObjectsActive(Section2, false);
        SetObjectsActive(Section3, false);
    }

    public void Active2()
    {
        SetObjectsActive(MenuList, false);
        SetObjectsActive(Active1_Common, false);
        SetObjectsActive(Section1_Quiz, false);
        SetObjectsActive(Section1_Ans, false);
        SetObjectsActive(Section2, true);
        SetObjectsActive(Section3, false);
    }

    public void Active3()
    {
        SetObjectsActive(MenuList, false);
        SetObjectsActive(Active1_Common, false);
        SetObjectsActive(Section1_Quiz, false);
        SetObjectsActive(Section1_Ans, false);
        SetObjectsActive(Section2, false);
        SetObjectsActive(Section3, true);
    }

    [Header("UI")]
    public Image targetIcon;
    public Sprite state1Sprite;
    public Sprite state2Sprite;
    public void ShowFail()
    {
        targetIcon.sprite = state2Sprite;
    }
    public void ResetUI()
    {
        targetIcon.sprite = state1Sprite;
    }
}
