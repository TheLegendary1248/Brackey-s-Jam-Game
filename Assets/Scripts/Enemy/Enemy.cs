using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Base class for all enemies
/// </summary>
public abstract class Enemy : MonoBehaviour, ITargetable, IHealth
{
    public Vector2 pos { get => transform.position; set => value = transform.position; }
    public ITargetable target;
    public enum WakeState
    {
        /// <summary>
        /// The enemy is awake and aware of it's surroundings. In this state, the enemy should behave 'casually' and free roam.
        /// </summary>
        Awake, 
        /// <summary>
        /// The enemy is asleep. In this state, the enemy should do nothing, but can be awoken if anything happens close.
        /// </summary>
        Asleep,
        /// <summary>
        /// The enemy is currently attacking something. Release all hell.
        /// </summary>
        Attacking
    }
    /// <summary>
    /// Describes the current wake state of the enemy
    /// </summary>
    public WakeState State { get; private set; } = WakeState.Asleep;
    public void Awake()
    {
        enabled = false;
    }
    /// <summary>
    /// This method is called when in range of noise
    /// </summary>
    public virtual void Wake()
    {
        if (State == WakeState.Asleep) { State = WakeState.Awake; WakeInternal(); enabled = true; }
    }
    protected abstract void WakeInternal();

    public abstract void Death();
    /// <summary>
    /// Calls Wake() on all entities within radius of the origin
    /// </summary>
    public static void Noise(Vector2 origin, float radius)
    {
        foreach (Collider2D c in Physics2D.OverlapCircleAll(origin, radius, LayerMask.NameToLayer("Enemy"))) if (c.GetComponent<Enemy>() is Enemy e) e.Wake();
    }
}
public interface ITargetable
{
    Vector2 pos { get; set; }
}
