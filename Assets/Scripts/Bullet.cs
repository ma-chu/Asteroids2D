using UnityEngine;
using System;
public class Bullet : SpaceBody
{
    public static float BaseSpeed;
    
    public static int UfoValue;
    public static int LargeAsteroidValue;
    public static int MediumAsteroidValue;
    public static int SmallAsteroidValue;

    public static event Action<int> ScoreChanges;
    

    private void OnEnable()
    {
        Invoke(nameof(Destroy), GameManager.ScreenResInWorld.x * 2 / BaseSpeed);
    }

    private void Destroy()
    {
        CancelInvoke();
        pool.Release(gameObject);
    }

    public void Move()
    {
        _rigidbody.velocity = transform.up * BaseSpeed;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(GameManager.UfoTag) && gameObject.CompareTag(GameManager.UfoBulletTag)) return;
        if (gameObject.CompareTag(GameManager.PlayerBulletTag))
        {
            if (other.CompareTag(GameManager.PlayerTag)) return;
            if (other.CompareTag(GameManager.UfoBulletTag)) ScoreChanges?.Invoke(UfoValue);
            if (other.CompareTag(GameManager.AsteroidTag))
            {
                int scoreValue = 0;
                switch (other.GetComponent<Asteroid>().type)
                {
                    case Asteroids.Large:
                        scoreValue = LargeAsteroidValue;
                        break;
                    case Asteroids.Medium:
                        scoreValue = MediumAsteroidValue;
                        break;
                    case Asteroids.Small:
                        scoreValue = SmallAsteroidValue;
                        break;
                }
                ScoreChanges?.Invoke(scoreValue);
            }
        }

        Destroy();
    }
}
