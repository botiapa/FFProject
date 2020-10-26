using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody rb;
    float moveSpeed = 2000;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed;
        float moveVertical = Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed;
        var movement = new Vector3(moveHorizontal, 0, moveVertical);

        rb.AddForce(movement);
    }
}
