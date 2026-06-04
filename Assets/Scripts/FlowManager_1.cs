using UnityEngine;
using System.Collections;

public class FlowManager_1 : MonoBehaviour
{
    public bool SignalSection;

    [Header("Managers")]
    public UIManager uiManager;
    public AudioManager_1 audioManager;
    public SpaceManager_1 spaceManager;
    public GameResult_1 gameResult;
    private bool isChecking = false;
    private bool hasSetPosition = false;

    public void StartSection1()
    {
        uiManager.Active1();
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
