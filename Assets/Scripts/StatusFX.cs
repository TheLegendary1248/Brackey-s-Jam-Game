using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class StatusFX : MonoBehaviour
{
    /// <summary>
    /// The effect StatusFX uses on the entity
    /// </summary>
    public delegate void Effect(ref int ct, GameObject host);
    static Dictionary<string, Effect> effectLib = new Dictionary<string, Effect>();
    public Effect effect;
    public float duration;
    float timeStamp;
    int counter;
    Coroutine lifetime;
    /// <summary>
    /// Adds a status effect to a GameObject, if possible
    /// </summary>
    /// <param name="victim">GameObject to add effect to</param>
    /// <param name="duration">Duration of the effect in seconds</param>
    /// <param name="effect">Name of the effect</param>
    public static bool AddEffect(GameObject victim, float duration, string effect)
    {
        if (!effectLib.ContainsKey(effect)) { Debug.LogError($"Effect {effect} not found"); return false; }
        else
        {
            StatusFX e = victim.AddComponent<StatusFX>();
            e.effect = effectLib[effect];
            e.duration = duration;
            return true;
        }
    }
    public void Start()
    {
        lifetime = StartCoroutine(KillEffect(duration));
    }
    public void FixedUpdate()
    {
        effect(ref counter, gameObject);
    }
    public float GetRemainingDuration() => Time.time - timeStamp;
    /// <summary>
    /// Set the new duration of the effect
    /// </summary>
    public void SetNewDuration(float time)
    {
        StopCoroutine(lifetime);
        lifetime = StartCoroutine(KillEffect(time));
    }
    IEnumerator KillEffect(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(this);
    }
    [StatusEffect]
    void Poison(ref int i, GameObject gb) { }
    [StatusEffect]
    void Burning(ref int i, GameObject gb) { }
    [StatusEffect]
    void Unlucky(ref int i, GameObject gb) { }
    [StatusEffect]
    void Lucky(ref int i, GameObject gb) { }
    [StatusEffect]
    void Regeneration(ref int i, GameObject gb) { }
    [StatusEffect]
    void Vertigo(ref int i, GameObject gb) { }
    [StatusEffect]
    void Sluggish(ref int i, GameObject gb) { }
    [StatusEffect]
    void Caffienated(ref int i, GameObject gb) { }
    [StatusEffect]
    void FastTime(ref int i, GameObject gb) { }
    [StatusEffect]
    void SlowTime(ref int i, GameObject gb) { }
    [StatusEffect]
    void FastForward(ref int i, GameObject gb) { }

}
/// <summary>
/// Use this on methods that are to be added to the effect dictionary upon initialization
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
sealed class StatusEffectAttribute : System.Attribute { }
