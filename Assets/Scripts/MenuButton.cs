using UnityEngine;

public class MenuButton : MonoBehaviour
{
    public FlowManager_1 flowManager;

    public bool SignalSection { get; private set; }

    public void StartSingleSection1()
    {
        SignalSection = false;
        flowManager.SignalSection = SignalSection;
        flowManager.StartSection1();
    }

    public void StartFullGame()
    {
        SignalSection = true;
        flowManager.SignalSection = SignalSection;
        flowManager.StartSection1();
    }
}