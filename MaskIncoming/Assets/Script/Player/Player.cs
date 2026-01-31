using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    
    private Rigidbody2D m_Rb;
    private Vector2 m_MoveInput;
    private Animator m_Animator;

    void Awake()
    {
        m_Rb = GetComponent<Rigidbody2D>();
        m_Animator = GetComponent<Animator>();
    }

    void Start()
    {
        
    }

    void Update()
    {

    }

    void FixedUpdate()
    {
        m_Rb.linearVelocity = m_MoveInput * moveSpeed;
    }

    public void OnMove(InputValue value)
    {
        m_MoveInput = value.Get<Vector2>();

        if (m_MoveInput != Vector2.zero)
        {
            m_Animator.SetFloat("MoveX", m_MoveInput.x);
            m_Animator.SetFloat("MoveY", m_MoveInput.y);
        }

        m_Animator.SetFloat("Speed", m_MoveInput.sqrMagnitude);
    }

    public void OnMaskPower(InputValue value)
    {
        if (value.isPressed)
        {
            m_Animator.SetTrigger("SpecialPower");
            Debug.Log("Potere Speciale!");
        }
    }

    void ActivatePower()
    {
        Debug.Log("Mask Power Activate");
    }
}
