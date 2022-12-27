using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
/// <summary>
/// Handles animation of explosions and summoning them
/// </summary>
public class Explosion : MonoBehaviour
{
    public AnimationCurve intensity;
    public AnimationCurve size;
    public float time;
    float stamp;
    public float radius;
    public new Light2D light;
    public ParticleSystem sys;
    int damageCarry, delay = 0;
    object owner;
    static Dictionary<string, GameObject> explosions = new Dictionary<string, GameObject>();
    static bool hasAlreadyLoaded = false;

    public void Start()
    {
        stamp = Time.fixedTime;
        if(!Level.CheckForTile(transform.position))sys.Play();
    }
    private void FixedUpdate()
    {
        if (delay > 0) --delay;
        else if (delay == 0) { SummonExplosion(transform.position, radius, damageCarry, delay, owner,""); delay = -10; }
        float dif = (Time.fixedTime - stamp) / time;
        light.pointLightOuterRadius = radius * size.Evaluate(dif);
        light.intensity = intensity.Evaluate(dif);
        if (sys.isStopped && dif > 1) Destroy(gameObject);
    }
    /// <summary>
    /// Creates an explosion
    /// </summary>
    /// <param name="reference">Name of explosion FX gameobject in resources folder</param>
    /// <param name="delay">Number of frames to delay explosion. Use to avoid instant explosion chains among tiles</param>
    public static void SummonExplosion(Vector2 pos, float radius, int dmg, int delay, object owner, string reference)
    {
        if (!hasAlreadyLoaded)
        {
            GameObject[] list = Resources.LoadAll<GameObject>(""); //ADDRESSABLES--------------!
            foreach (GameObject i in list)
            {
                explosions.Add(i.name, i);
            }
            hasAlreadyLoaded = true;
        }
        if(delay == 0)
        {   
            Level.CircleDamageTiles(pos, radius, dmg);
            if (explosions.ContainsKey(reference)) Instantiate(explosions[reference], pos, Quaternion.identity).GetComponent<Explosion>().radius = radius;
            else if (reference.Length != 0)Debug.LogError("Explosion object not found under name");
            foreach(Collider2D c in Physics2D.OverlapCircleAll(pos, radius * 2f, ~LayerMask.NameToLayer("Terrain"))) //Do some work for all colliders caught in blast or in range of noise
            {
                if (((Vector2)c.transform.position - pos).sqrMagnitude < radius * radius && c.GetComponent<Health>() is Health h) h.DealDamage(dmg, owner); 
                if (c.GetComponent<Enemy>() is Enemy e) e.Wake();
            }
        }
        else
        {
            if (explosions.ContainsKey(reference))
            {
                Explosion e = Instantiate(explosions[reference], pos, Quaternion.identity).GetComponent<Explosion>();
                e.radius = radius;
                e.damageCarry = dmg;
                e.delay = delay;
                e.owner = owner;
            }
            else Debug.LogError("Explosion object not found under name");
        }
        
    }

}
