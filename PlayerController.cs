using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator;

    private float moveSpeed = 4f;

    [Header("Movement System")]
    public float walkSpeed = 4f;
    public float runSpeed = 8f;
    private float gravity = 9.81f;

    //Interaction components
    PlayerInteraction playerInteraction;

    void Start()
    {
        //Get movement components
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        //Get interaction component
        playerInteraction = GetComponentInChildren<PlayerInteraction>();
    }

    void Update()
    {
        // Runs the function that handles all movement
        Move();
        //Runs the function that handles all interaction
        Interact();
        //Debugging purposes only
        //Skip the time when the right square bracket is pressed
        if (Input.GetKey(KeyCode.RightBracket))
        {
            TimeManager.Instance.Tick();
        }

        //Toggle relationship panel
        if (Input.GetKeyDown(KeyCode.R))
        {
            UIManager.Instance.ToggleRelationshipPanel();
        }
    }

    public void Interact()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            playerInteraction.Interact();
        }

        if (Input.GetButtonDown("Fire2"))
        {
            playerInteraction.ItemInteract();
        }

        if (Input.GetButtonDown("Fire3"))
        {
            playerInteraction.ItemKeep();
        }
    }

    public void Move()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 dir = new Vector3(horizontal, 0f, vertical).normalized;

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        // Ignore vertical camera tilt
        cameraForward.y = 0f;

        Vector3 moveDirection = cameraForward * vertical + cameraRight * horizontal;
        moveDirection = moveDirection.normalized;

        Vector3 velocity = moveDirection * moveSpeed * Time.deltaTime;

        if (controller.isGrounded)
        {
            velocity.y = 0;
        }

        velocity.y -= gravity * Time.deltaTime;

        if (Input.GetButton("Sprint"))
        {
            moveSpeed = runSpeed;
            animator.SetBool("Running", true);
        }
        else
        {
            moveSpeed = walkSpeed;
            animator.SetBool("Running", false);
        }

        if (dir.magnitude >= 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
            controller.Move(velocity);
        }

        animator.SetFloat("Speed", dir.magnitude);
    }
}