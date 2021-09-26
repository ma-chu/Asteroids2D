using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class UFO : SpaceBody, IDriven
{
    public static float FireRateMin;
    public static float FireRateMax;
    public static float BaseSpeed;
    
    public static event Action StartSound;
    public static event Action StopSound;
    
    [SerializeField] private Transform bulletSpawner;
    
    private Transform _player;
    private float _nextFire;

    protected override void Awake()
    {
        _player = Player.Instance.transform;

        base.Awake();
    }

    private void Start()
    {
        StartSound?.Invoke();
        _nextFire = Random.Range(FireRateMin, FireRateMax);
    }
    
    public float SetSpeed(float speed = 0f)
    {
        if (speed == 0f) return this.speed = BaseSpeed;
        return this.speed = speed;
    }
    
    
    
    protected override void FixedUpdate()
    {
        if (Time.time > _nextFire) 
        {
            var targetVector = _player.position - transform.position;
            bulletSpawner.rotation = Quaternion.LookRotation(Vector3.forward, targetVector);
            
            var bullet = pool.Get();
            bullet.transform.SetPositionAndRotation(transform.position, bulletSpawner.rotation);
            bullet.GetComponent<SpriteRenderer>().color = Color.red;
            bullet.tag = GameManager.UfoBulletTag;
            bullet.GetComponent<Bullet>().Move();

            _nextFire = Time.time + Random.Range(FireRateMin, FireRateMax);
        }
        
        base.FixedUpdate();
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(GameManager.UfoBulletTag)) return;
        
        base.OnTriggerEnter2D(other);
    }

    private void OnDestroy()
    {
        StopSound?.Invoke();
    }
}
