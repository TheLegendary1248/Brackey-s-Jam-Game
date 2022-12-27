using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// Possibly the base class for all slimes, if not, then the normal one
/// </summary>
public class Slime : Enemy
{
    public float chargeForce;
    public GameObject projectile;
    public AssetReference refer;
    Coroutine chargeCoroutine;
    Rigidbody2D rb2d;
    Vector2 startScale;
    bool shakeViolently = false;
    private void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        startScale = transform.localScale;
    }
    protected override void WakeInternal()
    {
        chargeCoroutine = StartCoroutine(Behaviour());
    }
    private void FixedUpdate()
    {
        if (shakeViolently) transform.localScale = startScale * new Vector2(Random.Range(0.8f,1.2f), Random.Range(0.8f,1.2f));
    }
    IEnumerator Behaviour()
    {
        yield return new WaitForSeconds(3f);
        shakeViolently = true;
        yield return new WaitForSeconds(0.75f);
        shakeViolently = false;
        rb2d.velocity = -(transform.position - (Vector3)Player.pos).normalized * chargeForce;
        Stretch();
        yield return new WaitForSeconds(0.2f);
        Shoot();
        yield return new WaitForSeconds(0.2f);
        Shoot();
        chargeCoroutine = StartCoroutine(Behaviour());
    }
    void Shoot() //Add in difficulty and switch to using the ObjectPooler
    {
        for (int i = 0; i < 6; i++)
        {
            Instantiate(projectile, transform.position, Quaternion.Euler(0, 0, 360 / 6 * i) * transform.rotation);
        }
    }
    void Stretch()
    {
        transform.localScale = startScale * new Vector2(0.7f, 1.15f);
        Vector2 dif = Player.pos - (Vector2)transform.position;
        transform.eulerAngles = new Vector3(0, 0, (Mathf.Atan2(dif.y, dif.x) * Mathf.Rad2Deg) + 90f);
        rb2d.angularVelocity = 0;
    }
    private void OnCollisionEnter2D() => transform.localScale = startScale;
    public override void Death()
    {
        Destroy(gameObject);
    }
}
