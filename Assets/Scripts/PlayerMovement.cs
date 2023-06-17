using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerMovementState
{
    Idle,
    Walking,
    Running,
    Jumping,
    FreeThrowing
}
public class PlayerMovement : MonoBehaviour
{
    //variables
    [SerializeField] private float m_walkSpeed = 5f;
    [SerializeField] private float m_runSpeed = 10f;
    [SerializeField] private float m_mouseSensitivity = 1f;
    [SerializeField] private float m_jumpForce = 5f;
    [SerializeField] private float m_gravity = 9.81f;
    private Vector3 m_forwardDirection;
    private Vector3 m_moveDirection;
    private Vector3 m_moveSpeed;
    private bool m_isGrounded;
    private float m_fallVelocity=0;
    private PlayerMovementState m_playerMovementState = PlayerMovementState.Idle;

    //teleport through portals
    private bool m_teleportable = true;

    //references
    // private CharacterController m_charactorController;
    [SerializeField] private Transform m_cameraTransform;

    //PUBLIC METHOD
    public PlayerMovementState GetPlayerMovementState()
    {
        return m_playerMovementState;
    }

    public void Teleport(Vector3 position, Vector3 rotation)
    {
        this.transform.position = position;
        this.transform.eulerAngles = rotation;
    }

    //PRIVATE METHOD
    private void Start() 
    {
        m_isGrounded = true;
        // m_charactorController = GetComponent<CharacterController>();

        //hide cursor
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update() 
    {
        HandleInput();
        ApplyGravity();
        ApplyMovement();
    }

    private void OnCollisionEnter(Collision other) 
    {
        if(other.gameObject.tag == "Ground")
        {
            m_isGrounded = true;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit) 
    {
        if (hit.collider.CompareTag("Ground"))
        {
            m_isGrounded = true;
        }
    }
    private void ApplyMovement()
    {
        // move the player
        // m_charactorController.Move(m_moveSpeed * Time.deltaTime);
        transform.position = transform.position + m_moveSpeed * Time.deltaTime;
    }
    private void ApplyGravity()
    {
        if (!m_isGrounded)
        {
            m_fallVelocity += m_gravity * Time.deltaTime;
            // m_charactorController.Move(Vector3.down * m_fallVelocity * Time.deltaTime);
            transform.position = transform.position + Vector3.down * m_fallVelocity * Time.deltaTime;
        }
        else
        {
            m_fallVelocity = 0;
        }
    }

    private void UpdateForwardDirection()
    {
        // get the forward direction of the camera
        m_forwardDirection = Camera.main.transform.forward;
        // set the y to 0
        m_forwardDirection.y = 0;
        // normalize the vector
        m_forwardDirection.Normalize();
    }

    private void HandleInput()
    {
        #region Look around with mouse
        // look around with mouse
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // rotate the camera
        m_cameraTransform.Rotate(-mouseY * m_mouseSensitivity, 0f, 0f);
        // rotate the player
        transform.Rotate(0f, mouseX * m_mouseSensitivity, 0f);
        #endregion

        UpdateForwardDirection();

        #region Movement
        // update move direction
        m_moveDirection = m_forwardDirection * Input.GetAxis("Vertical");

        // move right direction depend on the m_forwardDirection
        Vector3 moveRight = Quaternion.AngleAxis(90f, Vector3.up) * m_forwardDirection;
        Vector3 moveHorizontal = moveRight * Input.GetAxis("Horizontal");

        m_moveDirection += moveHorizontal;
        m_moveDirection.Normalize();

        // move the player if the move direction is not zero
        if ( m_isGrounded) 
        {
            if ( m_moveDirection != Vector3.zero )
            {
                // if the player is holding shift
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    Run(m_moveDirection);
                }
                else
                {
                    Walk(m_moveDirection);
                }
            }
            else
            {
                Idle();
            }
        }
        else
        {
            FreeThrow();
        }
        #endregion

        #region Jump
        if (Input.GetKeyDown(KeyCode.Space) && m_isGrounded)
        {
            Jump();
        }
        #endregion
    }

    private void Jump()
    {
        if (!m_isGrounded)    return;

        m_isGrounded = false;
        m_fallVelocity = -m_jumpForce;
        m_playerMovementState = PlayerMovementState.Jumping;
    }
    private void FreeThrow()
    {
        // update the move speed
        // m_moveSpeed.x = m_charactorController.velocity.x;
        // m_moveSpeed.z = m_charactorController.velocity.z;

        m_playerMovementState = PlayerMovementState.FreeThrowing;
    }
    private void Idle()
    {
        m_moveSpeed = Vector3.zero;

        m_playerMovementState = PlayerMovementState.Idle;
    }
    private void Walk(Vector3 moveDirection)
    {
        //m_charactorController.Move(moveDirection * m_walkSpeed * Time.deltaTime);
        m_moveSpeed = moveDirection * m_walkSpeed;

        m_playerMovementState = PlayerMovementState.Walking;
    }
    private void Run( Vector3 moveDirection)
    {
        // m_charactorController.Move(moveDirection * m_runSpeed * Time.deltaTime);
        m_moveSpeed = moveDirection * m_runSpeed;

        m_playerMovementState = PlayerMovementState.Running;
    }

    //Teleport through the portal
    // private void OnTriggerEnter(Collider other) 
    // {

    //     if (m_teleportable && (other.tag == "PortalHole" || other.tag == "Portal"))
    //     {
    //         m_teleportable = false;
    //         //get the parent object of other
    //         GameObject portalGO = other.gameObject;
    //         Portal portal = portalGO.GetComponent<Portal>();
    //         portal.SetEnableHole(false);
    //         Vector3 telePos = portal.GetTeleportPosition(this.transform.position);
    //         Vector3 teleForw = portal.GetTeleportForward(this.transform.forward);

    //         //teleport the player
    //         this.transform.position = telePos;
    //         this.transform.forward = teleForw;
    //     }
    // }
    // private void OnTriggerStay(Collider other) 
    // {
    //     if (other.tag == "PortalHole" || other.tag == "Portal")
    //     {
    //         m_teleportable = false;
    //         Portal portal = other.gameObject.GetComponent<Portal>();
    //         portal.SetEnableHole(true);
    //     }
    // }
    // private void OnTriggerExit(Collider other) 
    // {
    //     if (other.tag == "PortalHole" || other.tag == "Portal")
    //     {
    //         m_teleportable = true;
    //         Portal portal = other.gameObject.GetComponent<Portal>();
    //         portal.SetEnableHole(true);
    //     }
    // }
}
