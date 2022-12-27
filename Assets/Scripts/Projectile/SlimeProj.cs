using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeProj : Projectile
{
    Rigidbody2D rb;
    float startYScale; //Used to scale the object with speed
    float birthTime; //The time when the projectile was summoned 
    public float startSpeed;
    public int bounces;
    public override float RemainingLifetime { get => Time.time - birthTime; set { } }
    override public float Speed { get => rb.velocity.magnitude; set => rb.velocity = rb.velocity.normalized * value; }
    override public Vector2 Delta { get => rb.velocity; set => rb.velocity = value; }
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startYScale = transform.lossyScale.y;
        rb.velocity = new Vector2(Mathf.Cos((transform.eulerAngles.z + 90f) * Mathf.Deg2Rad), Mathf.Sin((transform.eulerAngles.z + 90f) * Mathf.Deg2Rad)) * startSpeed;
        StartCoroutine(Life());
    }
    protected override void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Map")) Level.DamageTileFromPoint(col.GetContact(0).point, 1);
        if (col.gameObject.GetComponent<Health>() is Health h) h.DealDamage(Damage, Owner);
        if (bounces-- <= 0) Destroy(gameObject);
        UpdateAngle();
    }
    void UpdateAngle()
    {
        transform.eulerAngles = new Vector3(0, 0, Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg - 90f);
        transform.localScale = new Vector2(transform.localScale.x, ((startYScale - transform.localScale.x) * (Speed / startSpeed)) + transform.localScale.x);
    }
    IEnumerator Life()
    {
        yield return new WaitForSeconds(Lifetime);
        Destroy(gameObject);
    }       
}
