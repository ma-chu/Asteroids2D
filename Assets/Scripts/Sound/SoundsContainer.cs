using System;
using System.Collections.Generic;
using UnityEngine;


public enum SoundTypes 
{
    None = 0,
    SmallAsteroidExplosion = 1,
    MediumAsteroidExplosion = 2,
    UFO = 3,
    LargeAsteroidExplosion = 4,
    PlayerFire = 5,
    PlayerThrust = 6,
    ExtraPlayer = 7,
    SpawnWave = 8
}
    
[Serializable]
public class SoundTypePair
{
    public SoundTypes Key;
    public AudioClip Value;
        
    public SoundTypePair(SoundTypes key, AudioClip value)
    {
        Key = key;
        Value = value;
    }
}
    
    
[CreateAssetMenu(fileName = "SoundsContainer", menuName = "_Asteroids/Sounds container", order = 1)]
public class SoundsContainer : ScriptableObject
{
    [SerializeField] private List<SoundTypePair> _sounds;

    private static SoundsContainer _instance;
        
    public static SoundsContainer Instance 
    {
        get
        {
            if (_instance != null) return _instance;
            _instance = Resources.Load<SoundsContainer>("SoundsContainer");
            return _instance;
        }
    }

    public static AudioClip GetAudioClip(SoundTypes soundType)
    {
        return Instance._sounds.Find(m => m.Key == soundType)?.Value;
    }
}
