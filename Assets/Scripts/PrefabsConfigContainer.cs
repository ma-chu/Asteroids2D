using UnityEngine;
using System;


[Serializable]
public class PrefabsConfig
{
    //UFO
    public float ufoFireRateMin = 2f;
    public float ufoFireRateMax = 5f;
    public float ufoBaseSpeed = 2f;
    //Asteroid
    public float asteroidSpeedMin = 1f;
    public float asteroidSpeedMax = 3f;
    public float asteroidTumble = 150f;
    //Bullet
    public float bulletBaseSpeed = 4f;
    
    public int ufoValue = 200;
    public int largeAsteroidValue = 20;
    public int mediumAsteroidValue = 50;
    public int smallAsteroidValue = 100;
}


[CreateAssetMenu(fileName = "PrefabsConfigContainer", menuName = "_Asteroids/PrefabsConfigContainer", order = 1)]
public class PrefabsConfigContainer : ScriptableObject
{
    public PrefabsConfig config;

    private static PrefabsConfigContainer _instance;
        
    public static PrefabsConfigContainer Instance 
    {
        get
        {
            if (_instance != null) return _instance;
            _instance = Resources.Load<PrefabsConfigContainer>("PrefabsConfigContainer");
            return _instance;
        }
    }
}