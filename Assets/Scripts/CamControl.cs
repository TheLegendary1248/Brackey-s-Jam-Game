using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CamControl : MonoBehaviour
{
    // Update is called once per frame
    void FixedUpdate()
    {
        if(Player.tf != null)
        {
            transform.position = Vector2.Lerp(Player.tf.position, transform.position,0.9f);
        }
    }
}
