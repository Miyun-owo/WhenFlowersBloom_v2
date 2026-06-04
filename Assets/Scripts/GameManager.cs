using UnityEngine;

public class GameManager : MonoBehaviour
{
    public UIManager uiManager;
    public FlowManager_1 flowManager;

    void Start()
    {
        InitGame();
    }

    public void InitGame()
    {
        GoHome();
    }

    public void GoHome()
    {
        uiManager.Homepage();
        flowManager.ResetState();
    }
}
