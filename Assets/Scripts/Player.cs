using System;
using System.Collections;
using UnityEngine;
using TMPro;
public class Player: SpaceBody
{
    private static Player _instance;
    public static Player Instance => _instance;
    
    [SerializeField] private float acceleration = 0.2f;
    [SerializeField] private float maxSpeed = 3f;
    [SerializeField] private float turnSpeed = 120f;
    [SerializeField] private float fireRate = 0.33f;

    [SerializeField] private Transform bulletSpawner;
    [SerializeField] private PoolOfObjects bulletPool;
    
    [HideInInspector] public int lives;
    
    private float _nextFire;
    private Collider2D _collider;
    private SpriteRenderer _renderer;

    protected override void Awake()
    {
        if (_instance == null) _instance = this;

        _collider = GetComponent<Collider2D>();
        _renderer = GetComponent<SpriteRenderer>();

        base.Awake();
    }

    private void OnEnable()
    {
        lives = 3;
    }

    private void Update()
    {
        var key = MenuManager.Instance.UseMouse ? Input.GetButtonDown("Fire1") : Input.GetKeyDown(KeyCode.Space);
        if (MenuManager.Instance.Paused || !key) return;
        {
            if (Time.time < _nextFire) return;
            _nextFire = Time.time + fireRate;
            
            var bullet = bulletPool.pool.Get();
            bullet.transform.SetPositionAndRotation(bulletSpawner.position, transform.rotation);
            bullet.GetComponent<SpriteRenderer>().color = Color.green;
            bullet.tag = "Bullet";
            bullet.GetComponent<Bullet>().Move();

            SoundsManager.Instance.PlaySound(SoundsContainer.GetAudioClip(SoundTypes.PlayerFire));
        }
    }

    protected override void FixedUpdate()
    {
        float lh = Input.GetAxisRaw("Horizontal");
        float lv = Input.GetAxisRaw("Vertical");
        
        Rotate(lh);
        
        var key = (lv != 0f)  || (MenuManager.Instance.UseMouse && Input.GetMouseButton(1));
        if (key)  
        {
            Move();
        }
        
        base.FixedUpdate();
    }
    
    private void Rotate(float lh)
    {
        float targetRotation;
        if (MenuManager.Instance.UseMouse)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 targetVector = mousePos - new Vector2(transform.position.x, transform.position.y);
            var angle = Vector2.SignedAngle(transform.up, targetVector);
            targetRotation = _rigidbody.rotation + Mathf.Clamp(angle, -1, 1) * turnSpeed * Time.deltaTime;
        }
        else
        {
            targetRotation = _rigidbody.rotation - lh * turnSpeed * Time.deltaTime;
        }
        
        _rigidbody.MoveRotation(targetRotation);
    }
    
    
    private void Move()
    {
        Vector2 addedVelocity = transform.up * acceleration;
        var currentVelocity = _rigidbody.velocity + addedVelocity;
        
        _rigidbody.velocity = Vector2.ClampMagnitude(currentVelocity, maxSpeed);
        
        SoundsManager.Instance.PlaySound(SoundsContainer.GetAudioClip(SoundTypes.PlayerThrust));

    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(GameManager.PlayerBulletTag)) return;

        if (GameManager.Instance.CheckForGameOver()) return;

        StartCoroutine (Respawn());
    }

    private IEnumerator Respawn()
    {
        _rigidbody.velocity = Vector2.zero;
        transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        
        _collider.enabled = false;
        
        var rate = new WaitForSeconds(0.25f);
        for (int i = 0; i < 12; i++)
        {
            _renderer.enabled = !_renderer.enabled;
            yield return rate; 
        }
        
        _collider.enabled = true;
    }
    
}