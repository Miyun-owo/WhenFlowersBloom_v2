using UnityEngine;

public class AudioManager_1 : MonoBehaviour
{
    public AudioSource frontSource;
    public AudioSource rightSource;
    public AudioSource backSource;
    public AudioSource leftSource;


    public void PlayAudio()
    {
        if (!frontSource.isPlaying) frontSource.Play();
        if (!rightSource.isPlaying) rightSource.Play();
        if (!backSource.isPlaying) backSource.Play();
        if (!leftSource.isPlaying) leftSource.Play();
    }

    public void PauseAudio()
    {
        frontSource.Pause();
        rightSource.Pause();
        backSource.Pause();
        leftSource.Pause();
    }

    public void ResumeAudio()
    {
        frontSource.UnPause();
        rightSource.UnPause();
        backSource.UnPause();
        leftSource.UnPause();
    }

    public void StopAudio()
    {
        frontSource.Stop();
        rightSource.Stop();
        backSource.Stop();
        leftSource.Stop();
    }

    public void SetDirectionalVolume(float front, float right, float back, float left)
    {
        frontSource.volume = front;
        rightSource.volume = right;
        backSource.volume = back;
        leftSource.volume = left;
    }
}
