using UnityEngine;
using UnityEngine.Pool;

public abstract class SpaceBody : MonoBehaviour
{
    protected Rigidbody2D _rigidbody;
    
    public IObjectPool<GameObject> pool; 
    
    protected virtual void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    protected virtual void FixedUpdate()
    {
        CheckBoundaries();
    }

    protected void CheckBoundaries()
    {
        if (transform.position.x > GameManager.ScreenResInWorld.x)
        {
            transform.position = new Vector3(-GameManager.ScreenResInWorld.x, transform.position.y, 0f);
        } 
        else if (transform.position.x < -GameManager.ScreenResInWorld.x)
        {
            transform.position = new Vector3(GameManager.ScreenResInWorld.x, transform.position.y, 0f);
        }
        
        if (transform.position.y > GameManager.ScreenResInWorld.y)
        {
            transform.position = new Vector3(transform.position.x, -GameManager.ScreenResInWorld.y, 0f);
        } 
        else if (transform.position.y < -GameManager.ScreenResInWorld.y)
        {
            transform.position = new Vector3(transform.position.x, GameManager.ScreenResInWorld.y, 0f);
        } 
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        Destroy(gameObject);
    }
}
