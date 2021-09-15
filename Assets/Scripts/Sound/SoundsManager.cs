using UnityEngine;
using UnityEngine.Audio;

public class SoundsManager : MonoBehaviour
{
    private static SoundsManager _instance;
    public static SoundsManager Instance => _instance;
        
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource audioSourceUfo;

    private void Awake()
    {
        if (_instance == null) _instance = this;
    }

    private void OnEnable()
    {
        Asteroid.ExplosionSound += OnAction;
        UFO.StartSound += PlayUfo;
        UFO.StopSound += StopUfo;
    }

    private void OnDisable()
    {
        Asteroid.ExplosionSound -= OnAction;
        UFO.StartSound -= PlayUfo;
        UFO.StopSound -= StopUfo;
    }

    public void PlaySound(AudioClip clip, float delay=0f)
    {
        audioSource.clip = clip;
        audioSource.PlayDelayed(delay);
    }
    
    private void PlayUfo()
    {
        audioSourceUfo.Play();
    }
    private void StopUfo()
    {
        audioSourceUfo.Stop();
    }
    
    private void OnAction(SoundTypes type)  
    {
        if (type == SoundTypes.None) return;

        PlaySound(SoundsContainer.GetAudioClip(type));
    }
}