using UnityEngine;
using System.Collections;
public class s1_GameResult : MonoBehaviour
{
    [Header("Reference")]
    public s1_SpaceManager spaceManager;
    public s1_AudioManager audioManager;

    [Header("Settings")]
    public float successThreshold = 0.85f;
    public bool CheckResult(float dot)
    {
        return dot > successThreshold;
    }


}
