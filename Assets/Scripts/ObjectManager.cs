using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
/// <summary>
/// Deals with basic resource management with object pooling and summoning. Time to flex
/// </summary>
public static class ObjectManager 
{
    //ADD METHOD TO DEAL WITH 'BRAINLESS' GAMEOBJECTS SUCH AS PARTICLE SYSTEMS
    /// <summary>
    /// The object pools
    /// </summary>
    static readonly Dictionary<string, Queue<GameObject>> Pools = new Dictionary<string, Queue<GameObject>>();

    static readonly Dictionary<string, List<string>> Refs = new Dictionary<string, List<string>>();
    /// <summary>
    /// Pools the given object, using the caller's defined PoolProcess method. The caller must inherit from MonoBehaviour
    /// </summary>
    /// <param name="caller">Just use 'this'</param>
    /// <param name="poolName">The string used to identify which pool the object should go to</param>
    public static void Pool(GameObject gb, string poolName)//DEAL WITH BRAINLESS OBJECTS USING PATTERN MATCHING
    {
        if (gb.GetComponent<IPoolable>() is IPoolable p) p.PoolProcess();
        if (Pools.ContainsKey(poolName)) Pools[poolName].Enqueue(gb);
        else
        {
            Queue<GameObject> t = new Queue<GameObject>();
            t.Enqueue(gb);
            Pools.Add(poolName, t);
        }
    }
    /// <summary>
    /// Unpools an object from the given pool name. The object should have an IPoolable script on it.
    /// </summary>
    /// <returns>Returns a GameObject from the pool, or null if none exist or no pool exists</returns>
    public static GameObject UnPool(string poolName)
    {
        if (Pools.ContainsKey(poolName))
        {
            Queue<GameObject> pool = Pools[poolName];
            if (pool.Count > 0)
            {
                GameObject gb = pool.Dequeue();
                gb.GetComponent<IPoolable>()?.DePoolProcess();
            }
        }
        return null;
    }
    /// <summary>
    /// Instantiates a projectile with the name present
    /// </summary>
    /// <param name="owner">The owner of the projectile that is automatically set on the projectile. 
    /// This also allows the method automatically call to ignore the collision between the projectile and it's user</param>
    /// <returns></returns>
    public static GameObject CreateProjectile(object owner, string projName)
    {
        GameObject proj = new GameObject();
        if (owner is GameObject g) Physics2D.IgnoreCollision(proj.GetComponent<Collider2D>(), g.GetComponent<Collider2D>());
        return null;
    }
    /// <summary>
    /// Instantiates an object under given string
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static GameObject Instantiate(string name)
    {
        
        return null;
    }
    public static GameObject Instantiate(AssetReference refer)
    {
        
        return null;
    }
    //Imagine wasting time on all below. Can't be me.
#if false
    /// <summary>
    /// Adds a 'reference' to a pool.
    /// </summary>
    public static void AddRef(string refName, string poolName)
    {
        if (!Refs.ContainsKey(poolName))
        {
            if (!Refs[poolName].Contains(refName)) Refs[poolName].Add(refName);
        }
    }
    /// <summary>
    /// Removes a 'reference' to a pool. If a pool has no remaining references, it and it's containing GameObjects will be destroyed.
    /// </summary>
    public static void RemoveRef(string refName, string poolName)
    {
        if (Refs.ContainsKey(poolName))
        {
            List<string> l;
            if ((l = Refs[poolName]).Contains(refName))
            {
                l.Remove(refName);
            }
            if(l.Count == 0)
            {
                DestroyPool(poolName);
            }
        }
    }
    /// <summary>
    /// This method is automatically called when no 'references' to a pool exist. However, you can call this directly to force destruction of a pool and it's gameobjects.
    /// </summary>
    public static void DestroyPool(string poolName)
    {
        while (Pools[poolName].Count > 0)
        {
            Object.Destroy(Pools[poolName].Dequeue());
        }
        Pools.Remove(poolName);
    }
#endif
}
/// <summary>
/// Those that implement this interface can pool themselves
/// </summary>
public interface IPoolable
{
    /// <summary>
    /// This method will be called by ObjectPooler when pooling the object. Use this method to prepare the object to become idle and disable it
    /// </summary>
    void PoolProcess();
    /// <summary>
    /// This method will be called by ObjectPooler when unpooling the object. Use this method to prepare the object to become active and enable it
    /// </summary>
    void DePoolProcess();

}