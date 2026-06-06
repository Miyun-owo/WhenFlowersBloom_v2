using UnityEngine;
using System.Collections;

public class s1_FlowManager : MonoBehaviour
{
    public bool SignalSection;

    [Header("Managers")]
    public UIManager uiManager;
    public s1_AudioManager audioManager;
    public s1_SpaceManager spaceManager;
    public s1_GameResult gameResult;
    private bool isChecking = false;
    private bool hasSetPosition = false;

    public void StartSection1()
    {
        uiManager.Active1();
    }

    public void StartSection2()
    {
        uiManager.Active2();
    }

    public void OnSetPosition()
    {
        hasSetPosition = true;
        spaceManager.StartTracking();
        audioManager.PlayAudio();
    }

    IEnumerator SelectCooldown()
    {
        isChecking = true;

        yield return new WaitForSeconds(3f);

        isChecking = false;
    }

    public void OnSelect()
    {
        if (!hasSetPosition) return;
        if (isChecking) return;

        float dot = spaceManager.CurrentDot;
        bool isCorrect = gameResult.CheckResult(dot);
        spaceManager.Confirm(isCorrect);

        if (isCorrect)
        {
            uiManager.Active1_1();
        }
        else
        {
            StartCoroutine(SelectCooldown());
        }
    }

    public void ResetState()
    {
        hasSetPosition = false;
        isChecking = false;

        audioManager.StopAudio();
        spaceManager.ResetState();
    }

}
