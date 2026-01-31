using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    
    private Rigidbody2D m_Rb;
    private Vector2 m_MoveInput;
    private Animator m_Animator;
    private Coroutine weightCoroutine;

    private int m_Lives = 3;
    private bool b_IsMaskOn = false;
    private int m_SpecialLayerIndex;

    void Awake()
    {
        m_Rb = GetComponent<Rigidbody2D>();
        m_Animator = GetComponent<Animator>();
        m_SpecialLayerIndex = m_Animator.GetLayerIndex("Special Layer");
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    // Movement

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

    // MaskPower

    public void OnMaskPower(InputValue value)
    {
        b_IsMaskOn = !b_IsMaskOn;
        
        if (weightCoroutine != null) StopCoroutine(weightCoroutine);
        
        float target = b_IsMaskOn ? 1f : 0f;
        weightCoroutine = StartCoroutine(AnimateLayerWeight(target, 0.5f));

        if (b_IsMaskOn) ActivatePower(); else DeactivatePower();
    }

    private IEnumerator AnimateLayerWeight(float targetWeight, float duration)
    {
        float startWeight = m_Animator.GetLayerWeight(m_SpecialLayerIndex);
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float newWeight = Mathf.Lerp(startWeight, targetWeight, time / duration);
            m_Animator.SetLayerWeight(m_SpecialLayerIndex, newWeight);
            yield return null;
        }

        m_Animator.SetLayerWeight(m_SpecialLayerIndex, targetWeight);
    }

    void ActivatePower()
    {
        
    }

    void DeactivatePower()
    {
        
    }

    // Lifes

    public void TakeDamage()
    {
        if(m_Lives != 1)
        {
            m_Lives -= 1;
        }
        else
        {
            // todo: death event
        }
    }
}
