using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour //Implement Enemy Pooling and damage types
{
    public int health;
    public int maxHealth;
    int startingHealth;
    public int armor;
    public int mana;
    public float hpPercent => health / startingHealth;
    /// <summary>
    /// Deals damage to entity
    /// </summary>
    /// <param name="owner">The dealer of the damage. For most cases, use this. If cause is unknown, refer to MiscSrc, or use a string so at least the death screen can be correct :/</param>
    /// <returns>Returns if entity was killed</returns>
    public bool DealDamage(int dmg, object owner)
    {
        if (dmg - armor > health)
        {
            if (GetComponent<IHealth>() is IHealth h) h.Death();
            else Destroy(gameObject);
            return true;
        }
        else if (dmg > armor) health -= dmg;
        return false;
    }
    /// <summary>
    /// This enumeration can be passed as the owner of damage when the cause is unclear
    /// </summary>
    public enum MiscSrc
    {
        Explosion,
        Tile,
        WhoTFKnows
    }
    
}
/// <summary>
/// Classes that inherit this interface can have the Death() method called to control removal of object
/// </summary>
public interface IHealth
{
    /// <summary>
    /// Method to be called by Health when health reaches zero, or probably insta-kill
    /// </summary>
    void Death();
}
