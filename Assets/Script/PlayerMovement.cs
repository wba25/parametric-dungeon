using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public float moveSpeed = 5f;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator anim;

    private Vector2 movement;

    void Start()
    {
        
    }

    void Update()
    {
        // Input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        // Animation
        anim.SetFloat("Horizontal", movement.x);
        anim.SetFloat("Vertical", movement.y);
        anim.SetFloat("Speed", movement.sqrMagnitude);
    }

    void FixedUpdate()
    {
        // Movement
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
