using UnityEngine;

public class GameManager : MonoBehaviour
{
    public UIManager uiManager;
    public s1_FlowManager flowManager;

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
