using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// All projectiles derive from this class
/// </summary>
public abstract class Projectile : MonoBehaviour, IPoolable
{
    /// <summary>
    /// The amount of time left the projectile stays alive
    /// </summary>
    public abstract float RemainingLifetime { get; set; }
    public float Lifetime;
    public ushort Damage;
    /// <summary>
    /// The owner of the projectile
    /// </summary>
    [HideInInspector]
    public object Owner;
    /// <summary>
    /// The speed of the projectile. 
    /// </summary>
    public abstract float Speed { get; set; }
    /// <summary>
    /// The delta vector of the projectile. Aka, the separate x,y movement of the projectile
    /// </summary>
    public abstract Vector2 Delta { get; set; }
    public virtual void PoolProcess()
    {
        gameObject.SetActive(false);
    }
    public virtual void DePoolProcess()
    {
        gameObject.SetActive(true);
    }
    protected virtual void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Map")) Level.DamageTileFromPoint(col.GetContact(0).point, Damage);
    }
}
