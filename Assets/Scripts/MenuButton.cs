using UnityEngine;

public class MenuButton : MonoBehaviour
{
    public s1_FlowManager flowManager;

    public bool SignalSection { get; private set; }

    public void StartSingleSection1()
    {
        SignalSection = false;
        flowManager.SignalSection = SignalSection;
        flowManager.StartSection1();
    }

    public void StartSingleSection2()
    {
        SignalSection = false;
        flowManager.SignalSection = SignalSection;
        flowManager.StartSection2();
    }

    public void StartSingleSection3()
    {
        SignalSection = false;
        flowManager.SignalSection = SignalSection;
        flowManager.StartSection3();
    }

    public void StartFullGame()
    {
        SignalSection = true;
        flowManager.SignalSection = SignalSection;
        flowManager.StartSection1();
    }
}