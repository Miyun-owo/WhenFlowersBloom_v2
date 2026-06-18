using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    // Section I
    public GameObject[] MenuList;
    public GameObject[] Active1_Common;
    public GameObject[] Section1_Quiz;
    public GameObject[] Section1_Ans;
    public GameObject[] Section2_Wheel_05;
    public GameObject[] Section2_Wheel_06;
    public GameObject[] Section2_Wheel_07;
    public GameObject[] Section2_Wheel_08;
    public GameObject[] Section2_EmptyWheel;
    public GameObject[] Section3;

    // Section II
    //public GameObject[] 

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
        SetObjectsActive(Section2_Wheel_05, false);
        SetObjectsActive(Section2_Wheel_06, false);
        SetObjectsActive(Section2_Wheel_07, false);
        SetObjectsActive(Section2_Wheel_08, false);
        SetObjectsActive(Section2_EmptyWheel, false);
        SetObjectsActive(Section3, false);
    }

    public void Active1()
    {
        SetObjectsActive(MenuList, false);
        SetObjectsActive(Active1_Common, true);
        SetObjectsActive(Section1_Quiz, true);
        SetObjectsActive(Section1_Ans, false);
        SetObjectsActive(Section2_Wheel_05, false);
        SetObjectsActive(Section2_Wheel_06, false);
        SetObjectsActive(Section2_Wheel_07, false);
        SetObjectsActive(Section2_Wheel_08, false);
        SetObjectsActive(Section2_EmptyWheel, false);
        SetObjectsActive(Section3, false);
    }

    public void Active1_1()
    {
        SetObjectsActive(MenuList, false);
        SetObjectsActive(Active1_Common, true);
        SetObjectsActive(Section1_Quiz, false);
        SetObjectsActive(Section1_Ans, true);
        SetObjectsActive(Section2_Wheel_05, false);
        SetObjectsActive(Section2_Wheel_06, false);
        SetObjectsActive(Section2_Wheel_07, false);
        SetObjectsActive(Section2_Wheel_08, false);
        SetObjectsActive(Section2_EmptyWheel, false);
        SetObjectsActive(Section3, false);
    }

    public void Active2()
    {
        SetObjectsActive(MenuList, false);
        SetObjectsActive(Active1_Common, false);
        SetObjectsActive(Section1_Quiz, false);
        SetObjectsActive(Section1_Ans, false);
        SetObjectsActive(Section2_Wheel_05, false);
        SetObjectsActive(Section2_Wheel_06, false);
        SetObjectsActive(Section2_Wheel_07, false);
        SetObjectsActive(Section2_Wheel_08, false);
        SetObjectsActive(Section2_EmptyWheel, true);
        SetObjectsActive(Section3, false);
    }

    public void Active3()
    {
        SetObjectsActive(MenuList, false);
        SetObjectsActive(Active1_Common, false);
        SetObjectsActive(Section1_Quiz, false);
        SetObjectsActive(Section1_Ans, false);
        SetObjectsActive(Section2_Wheel_05, false);
        SetObjectsActive(Section2_Wheel_06, false);
        SetObjectsActive(Section2_Wheel_07, false);
        SetObjectsActive(Section2_Wheel_08, false);
        SetObjectsActive(Section2_EmptyWheel, false);
        SetObjectsActive(Section3, true);
    }

    public void Active2_Wheel_05()
    {
        SetObjectsActive(MenuList, false);
        SetObjectsActive(Active1_Common, false);
        SetObjectsActive(Section1_Quiz, false);
        SetObjectsActive(Section1_Ans, false);
        SetObjectsActive(Section2_Wheel_05, true);
        SetObjectsActive(Section2_Wheel_06, false);
        SetObjectsActive(Section2_Wheel_07, false);
        SetObjectsActive(Section2_Wheel_08, false);
        SetObjectsActive(Section2_EmptyWheel, false);
        SetObjectsActive(Section3, false);
    }

    public void Active2_Wheel_06()
    {
        SetObjectsActive(MenuList, false);
        SetObjectsActive(Active1_Common, false);
        SetObjectsActive(Section1_Quiz, false);
        SetObjectsActive(Section1_Ans, false);
        SetObjectsActive(Section2_Wheel_05, false);
        SetObjectsActive(Section2_Wheel_06, true);
        SetObjectsActive(Section2_Wheel_07, false);
        SetObjectsActive(Section2_Wheel_08, false);
        SetObjectsActive(Section2_EmptyWheel, false);
        SetObjectsActive(Section3, false);
    }

    public void Active2_Wheel_07()
    {
        SetObjectsActive(MenuList, false);
        SetObjectsActive(Active1_Common, false);
        SetObjectsActive(Section1_Quiz, false);
        SetObjectsActive(Section1_Ans, false);
        SetObjectsActive(Section2_Wheel_05, false);
        SetObjectsActive(Section2_Wheel_06, false);
        SetObjectsActive(Section2_Wheel_07, true);
        SetObjectsActive(Section2_Wheel_08, false);
        SetObjectsActive(Section2_EmptyWheel, false);
        SetObjectsActive(Section3, false);
    }

    public void Active2_Wheel_08()
    {
        SetObjectsActive(MenuList, false);
        SetObjectsActive(Active1_Common, false);
        SetObjectsActive(Section1_Quiz, false);
        SetObjectsActive(Section1_Ans, false);
        SetObjectsActive(Section2_Wheel_05, false);
        SetObjectsActive(Section2_Wheel_06, false);
        SetObjectsActive(Section2_Wheel_07, false);
        SetObjectsActive(Section2_Wheel_08, true);
        SetObjectsActive(Section2_EmptyWheel, false);
        SetObjectsActive(Section3, false);
    }

    public void Actice2_EmptyWheel()
    {
        SetObjectsActive(MenuList, false);
        SetObjectsActive(Active1_Common, false);
        SetObjectsActive(Section1_Quiz, false);
        SetObjectsActive(Section1_Ans, false);
        SetObjectsActive(Section2_Wheel_05, false);
        SetObjectsActive(Section2_Wheel_06, false);
        SetObjectsActive(Section2_Wheel_07, false);
        SetObjectsActive(Section2_Wheel_08, false);
        SetObjectsActive(Section2_EmptyWheel, true);
        SetObjectsActive(Section3, false);
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
