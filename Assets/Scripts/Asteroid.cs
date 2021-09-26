using System;
using UnityEngine;
using Random = UnityEngine.Random;

public enum Asteroids 
{
    Small = 1,
    Medium = 2,
    Large = 4
}

public class Asteroid : SpaceBody, IDriven
{
    public static float SpeedMin;
    public static float SpeedMax;
    public static float Tumble;
    
    public static event Action<SoundTypes> ExplosionSound;
    public static event Action<Vector2, Vector2, Asteroids> SpawnDebris;

    public Asteroids type = Asteroids.Large;

    
    private void Start()
    {
        RandomRotator();
    }
    private void RandomRotator()
    {
        _rigidbody.angularVelocity = Random.value * Tumble;
    }

    public float SetSpeed(float speed = 0f)
    {
        if (speed == 0f)
        {
            return this.speed = Random.Range(SpeedMin, SpeedMax);;
        }
        return this.speed = speed;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(GameManager.AsteroidTag)) return;
        
        ExplosionSound?.Invoke((SoundTypes)type);

        if (other.CompareTag(GameManager.PlayerBulletTag) || other.CompareTag(GameManager.UfoBulletTag))
        {
            switch (type)
            {
                case Asteroids.Large:
                    SpawnDebris?.Invoke(transform.position, _rigidbody.velocity, Asteroids.Medium);
                    break;
                case Asteroids.Medium:
                    SpawnDebris?.Invoke(transform.position, _rigidbody.velocity, Asteroids.Small);
                    break;
            }
        }

        pool.Release(gameObject);
    }
}


