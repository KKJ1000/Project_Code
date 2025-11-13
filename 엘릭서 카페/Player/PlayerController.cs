using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    private float currentSpeed;
    Rigidbody rb;
    SpriteRenderer spriteRenderer;
    Animator anim;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (StoryManager.Instance != null && StoryManager.Instance.IsStoryPlaying())
        {
            rb.velocity = Vector3.zero;
            anim.SetBool("isWalking", false);
            return;
        }

        HandleMovement();
    }

    private void HandleMovement()
    {
        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;

        float moveInputX = Input.GetAxisRaw("Horizontal");
        float moveInputY = Input.GetAxisRaw("Vertical");

        Vector3 moveVelocity = new Vector3(moveInputX * currentSpeed, rb.velocity.y, moveInputY * currentSpeed);
        rb.velocity = moveVelocity;

        if (Input.GetButton("Horizontal"))
        {
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;
        }

        anim.SetBool("isWalking", rb.velocity.normalized.x != 0 || rb.velocity.normalized.z != 0);
    }
}
