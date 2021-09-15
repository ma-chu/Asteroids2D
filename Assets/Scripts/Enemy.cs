using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : SpaceBody
{
    protected float speed;
    
    public abstract float SetSpeed(float speed = 0f);
    
    public void Move(Vector2 direction)
    {
        _rigidbody.velocity = direction * speed;
    }
}
