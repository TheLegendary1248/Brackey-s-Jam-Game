using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Player : MonoBehaviour, IHealth
{
    static Player self;
    public static Transform tf { get { return self.transform; } }
    public Rigidbody2D rb;
    public float Speed = 1f;
    public static Vector2 pos { get { return self.transform.position; } }
    Coroutine pickaxeCo;
    public float pickTime = 0.5f;
    Vector2 mapContact;
    void Start()
    {
        self = this;
        rb = GetComponent<Rigidbody2D>();
    }
    void FixedUpdate()
    {
        rb.MovePosition((Vector2)transform.position + new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) * Time.fixedDeltaTime * Speed);
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Level.DamageTile(Camera.main.ScreenToWorldPoint(Input.mousePosition), 3);
        }
        if (Input.GetMouseButtonDown(0))
        {
            Explosion.SummonExplosion(Camera.main.ScreenToWorldPoint(Input.mousePosition), 3, 30, 0, null,"Normal");
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Instantiate((GameObject)Resources.Load("Enemies/Dummy"), transform.position, Quaternion.identity);
       
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Map") && pickaxeCo == null)
        {
            pickaxeCo = StartCoroutine(Pickaxe());
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Map"))
        {
            mapContact = collision.GetContact(0).point;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Map"))
        {
            StopCoroutine(pickaxeCo); pickaxeCo = null;
        }
    }
    IEnumerator Pickaxe()
    {
        yield return new WaitForSeconds(pickTime);
        Level.DamageTileFromPoint(mapContact, 3);
        Enemy.Noise(mapContact, 1f);
        pickaxeCo = StartCoroutine(Pickaxe());
    }
    public void Death()
    {
        //ya no
    }
}
