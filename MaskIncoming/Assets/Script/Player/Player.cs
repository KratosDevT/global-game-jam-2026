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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        
    }

    void Update()
    {

    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        if (moveInput != Vector2.zero)
        {
            animator.SetFloat("MoveX", moveInput.x);
            animator.SetFloat("MoveY", moveInput.y);
        }

        animator.SetFloat("Speed", moveInput.sqrMagnitude);
    }

    public void OnMaskPower(InputValue value)
    {
        if (value.isPressed)
        {
            animator.SetTrigger("SpecialPower");
            Debug.Log("Potere Speciale!");
        }
    }

    void ActivatePower()
    {
        Debug.Log("Mask Power Activate");
    }
}
