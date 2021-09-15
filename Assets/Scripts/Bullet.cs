using UnityEngine;
using System;
public class Bullet : SpaceBody
{
    public static event Action<int> ScoreChanges;
    
    private const float BaseSpeed = 4f;

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
        if (other.CompareTag("UFO") && gameObject.CompareTag("UfoBullet")) return;
        if (gameObject.CompareTag("Bullet"))
        {
            if (other.CompareTag("Player")) return;
            if (other.CompareTag("UFO")) ScoreChanges?.Invoke(200);
            if (other.CompareTag("Asteroid"))
            {
                int scoreValue = 0;
                switch (other.GetComponent<Asteroid>().type)
                {
                    case Asteroids.Large:
                        scoreValue = 20;
                        break;
                    case Asteroids.Medium:
                        scoreValue = 50;
                        break;
                    case Asteroids.Small:
                        scoreValue = 100;
                        break;
                }
                ScoreChanges?.Invoke(scoreValue);
            }
        }

        Destroy();
    }
}
