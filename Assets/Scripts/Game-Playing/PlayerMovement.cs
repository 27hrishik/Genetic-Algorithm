using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.A))
        {
            //move left
            transform.Translate(Vector2.left * speed * Time.deltaTime);
        }
        if(Input.GetKey(KeyCode.D))
        {
            //move right
            transform.Translate(Vector2.right * speed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
            NeuroEvolution.instance.CurrentAgent--;
        }
    }
}
