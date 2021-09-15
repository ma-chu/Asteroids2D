using System;
using UnityEngine;
using Random = UnityEngine.Random;

public enum Asteroids 
{
    Small = 1,
    Medium = 2,
    Large = 4
}

public class Asteroid : Enemy
{
    private const float SpeedMin = 1f;
    private const float SpeedMax = 3f;
    private const float Tumble = 150f;
    
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

    public override float SetSpeed(float speed = 0f)
    {
        if (speed == 0f)
        {
            return this.speed = Random.Range(SpeedMin, SpeedMax);;
        }
        return this.speed = speed;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Asteroid")) return;
        
        ExplosionSound?.Invoke((SoundTypes)type);

        if (other.CompareTag("Bullet"))
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


