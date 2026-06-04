using UnityEngine;
using System.Collections;
public class GameResult_1 : MonoBehaviour
{
    [Header("Reference")]
    public SpaceManager_1 spaceManager;
    public AudioManager_1 audioManager;

    [Header("Settings")]
    public float successThreshold = 0.85f;
    public bool CheckResult(float dot)
    {
        return dot > successThreshold;
    }


}
