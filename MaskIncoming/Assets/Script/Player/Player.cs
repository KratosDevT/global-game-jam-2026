using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private PlayerControls controls;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        controls = new PlayerControls();
    }

    void Start()
    {
        
    }

    void OnEnable() => controls.Player.Enable();
    void OnDisable() => controls.Player.Disable();

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    void Update()
    {
        moveInput = controls.Player.Move.ReadValue<Vector2>();

        if (moveInput != Vector2.zero)
        {
            animator.SetFloat("MoveX", moveInput.x);
            animator.SetFloat("MoveY", moveInput.y);
                    Debug.Log("Potere Speciale!");
        }
        animator.SetFloat("Speed", moveInput.sqrMagnitude);

        if (controls.Player.MaskPower.triggered)
        {
            ActivatePower();
        }
    }

    // public void OnMove(InputValue value)
    // {
    //     moveInput = value.Get<Vector2>();

    //     if (moveInput != Vector2.zero)
    //     {
    //         animator.SetFloat("MoveX", moveInput.x);
    //         animator.SetFloat("MoveY", moveInput.y);
    //         Debug.Log("Move!");
    //     }

    //     animator.SetFloat("Speed", moveInput.sqrMagnitude);
    // }

    // public void OnMaskPower(InputValue value)
    // {
    //     if (value.isPressed)
    //     {
    //         animator.SetTrigger("SpecialPower");
    //         Debug.Log("Potere Speciale!");
    //     }
    // }

    void ActivatePower()
    {
        Debug.Log("Mask Power Activate");
    }
}
