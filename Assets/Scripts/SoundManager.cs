using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource effectsSource;  // Set in Inspector

    [Header("Sound Clips")]
    public AudioClip flipClip;
    public AudioClip matchClip;
    public AudioClip mismatchClip;
    public AudioClip gameOverClip;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void PlayFlipSound()
    {
        PlayClip(flipClip);
    }

    public void PlayMatchSound()
    {
        PlayClip(matchClip);
    }

    public void PlayMismatchSound()
    {
        PlayClip(mismatchClip);
    }

    public void PlayGameOverSound()
    {
        PlayClip(gameOverClip);
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip != null && effectsSource != null)
        {
            effectsSource.PlayOneShot(clip);
        }
    }
}
