using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Collections;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    private CharacterController characterController;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 2f;

    [Header("Push/Pull")]
    [SerializeField] private float pushForce = 5f;
    [SerializeField] private float pullSpeed = 0.5f;
    [SerializeField] private float pullMinDistance = 1.5f; // prevents overlap where player teleports on top of cube

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private PlayerControls controls;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isSprinting;

    // Pulling state
    private bool isPulling = false;
    private Rigidbody pulledCube;

    // Pushing state
    private bool isCurrentlyPushing = false;
    private bool isMoving = false;
    private Vector3 lastPosition;

    

    private void Awake()
    {
        controls = new PlayerControls();
    }
    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }


    //companion follower script
    [SerializeField] private Follower follower;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        controls.Player.Jump.performed += ctx => Jump();
        controls.Player.Pull.performed += ctx => StartPull();
        controls.Player.Pull.canceled += ctx => StopPull();
        controls.Player.Whistle.performed += ctx => Whistle();
    }

    void Update()
    {

        // TRY TO REFACTOR UPDATE AS MUCH AS POSSIBELE


        // checking ground
        isGrounded = characterController.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        

        // movement
        moveInput = controls.Player.Move.ReadValue<Vector2>();
        Vector3 inputMove = new Vector3(moveInput.x, 0, moveInput.y);
        isSprinting = controls.Player.Sprint.ReadValue<float>() > 0;
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        if( isPulling)
        {
            inputMove = new Vector3(inputMove.x, 0, 0); //only pulls on z axis
            currentSpeed = pullSpeed;
        }

        isMoving = inputMove.magnitude > 0.1f; // checking if movement is happening
        lastPosition = transform.position; //seeing if player position is changing

        Vector3 move = inputMove; // default movement

        // pulling while player is moving backwards
        if (isPulling && pulledCube != null)
        {
            float dot = Vector3.Dot(transform.forward, inputMove);

            if (dot < 0f) // moving backward
            {
                // target position behind player
                Vector3 targetPos = transform.position - transform.forward * pullMinDistance;
                targetPos.y = pulledCube.position.y;

                // moving cube
                pulledCube.position = Vector3.Lerp(pulledCube.position, targetPos, pullSpeed * Time.deltaTime);

                // rotate cube to player
                pulledCube.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);

                // Force player backward
                move = -transform.forward.normalized;
                currentSpeed = pullSpeed;

                //normalize the pulling 
                //mini challenge get the pulling working like the pushing
                // pullingDir.y = 0;
                // pushPulling.Normalize();
            }
        }

        //move player
        characterController.Move(move * currentSpeed * Time.deltaTime + velocity * Time.deltaTime);

        //rotating player so they face the direction where they are going
        if (move != Vector3.zero && !isPulling)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }

        // animator
        float horizontalSpeed = move.magnitude * currentSpeed;
        float normalizedSpeed = horizontalSpeed / sprintSpeed;
        animator.SetFloat("Speed", normalizedSpeed, 0.1f, Time.deltaTime);

        //pushing animation
        animator.SetBool("isPushing", isCurrentlyPushing);
        isCurrentlyPushing = false; // reset for next frame
        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isPulling", isPulling);
    }

    void Jump()
    {
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("Jump");
        }
    }

    // pushing cubes
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isPulling) return; // don't push while pulling
        Debug.Log("Hit something: " + hit.collider.name);

        Rigidbody rb = hit.collider.attachedRigidbody;
        if (rb != null && !rb.isKinematic)
        {
            Debug.Log("Player has hit cube");
            // Keep cube upright
            rb.rotation = Quaternion.Euler(0, rb.rotation.eulerAngles.y, 0);

            // Push direction
            Vector3 pushDir = hit.transform.position - transform.position;
            //mini challenge get the pulling working like the pushing
            pushDir.y = 0;
            pushDir.Normalize();

            rb.AddForce(pushDir * pushForce, ForceMode.Force);

            // Check if player is moving toward the cube
            float dot = Vector3.Dot(pushDir, transform.forward);
            if (dot > 0.5f)
            {
                // threshold for pushing
                isCurrentlyPushing = true;
                Debug.Log("Pushing cube");
            }
        
        }
    }

    // pull functions
    private void StartPull()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 2f))
        {
            Rigidbody rb = hit.collider.attachedRigidbody;
            if (rb != null && !rb.isKinematic)
            {
                pulledCube = rb;
                isPulling = true;
            }
        }
    }

    private void StopPull()
    {
        isPulling = false;
        pulledCube = null;
    }

    private void Whistle()
    {
        if (follower != null)
        {
            follower.ToggleFollow();
            animator.SetTrigger("Whistle");
        }
    }
}







