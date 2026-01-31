using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float consumeSpeed = 20f;
    [SerializeField] private float regainSpeed = 15f;
    [SerializeField] private float regainDelay = 2f;
    
    [Header("UI Reference")]
    [SerializeField] private Slider staminaSlider;

    [Header("UI Stamina")]
    [SerializeField] private Image staminaFillImage;
    [SerializeField] private Gradient staminaGradient;
    [SerializeField] private float colorLerpSpeed = 5f;
    [SerializeField] private float pulseSpeed = 10f;
    [SerializeField] private float minAlpha = 0.3f;
    private Vector3 sliderOriginalPos;

    // SpecialPower
    private Coroutine weightCoroutine;
    private float currentStamina;
    private float lastTimeMaskDeactivated;
    private bool b_IsMaskOn = false;
    private int m_SpecialLayerIndex;
    
    // Components
    private Rigidbody2D m_Rb;
    private Vector2 m_MoveInput;
    private Animator m_Animator;

    // Events
    [SerializeField] private GameEvent OnMaskActivation;
    [SerializeField] private GameEvent OnMaskDeactivation;

    private int m_Lives = 3;

    void Awake()
    {
        m_Rb = GetComponent<Rigidbody2D>();
        m_Animator = GetComponent<Animator>();
        m_SpecialLayerIndex = m_Animator.GetLayerIndex("Special Layer");
    }

    void Start()
    {
        currentStamina = maxStamina;
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = maxStamina;
            sliderOriginalPos = staminaSlider.transform.localPosition;
        }

        if (staminaFillImage != null && staminaGradient != null)
        {
            staminaFillImage.color = staminaGradient.Evaluate(1f);
        }
    }

    void Update()
    {
        // stamina
        if (b_IsMaskOn)
        {
            currentStamina -= consumeSpeed * Time.deltaTime;
            
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                TakeDamage();
                DeactivatePower();
            }
        }
        else
        {
            if (Time.time > lastTimeMaskDeactivated + regainDelay)
            {
                currentStamina += regainSpeed * Time.deltaTime;
            }
        }

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        if (staminaSlider != null) staminaSlider.value = currentStamina;

        UpdateStaminaUI();
    }

    // UI
    private void UpdateStaminaUI()
    {
        if (staminaSlider == null || staminaFillImage == null) return;

        staminaSlider.value = currentStamina;
        float staminaPercent = currentStamina / maxStamina;

        Color targetColor = staminaGradient.Evaluate(staminaPercent);

        if (staminaPercent < 0.25f)
        {
            float lerpAlpha = Mathf.Lerp(minAlpha, 1f, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
            targetColor.a = lerpAlpha;
        }
        else
        {
            targetColor.a = 1f;
        }

        staminaFillImage.color = Color.Lerp(staminaFillImage.color, targetColor, Time.deltaTime * colorLerpSpeed);
            
            if (staminaPercent < 0.25f)
            {
                staminaSlider.transform.localPosition = sliderOriginalPos + new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    0);
            }
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
        if(value.isPressed)
        {
            if(!b_IsMaskOn && currentStamina > 20f)
            {
                b_IsMaskOn = true;
                ActivatePower();
                if(OnMaskActivation)
                    OnMaskActivation.Raise();
            }
            else if(b_IsMaskOn)
            {
                b_IsMaskOn = false;
                lastTimeMaskDeactivated = Time.time;
                DeactivatePower();
                if(OnMaskDeactivation)
                    OnMaskDeactivation.Raise();
            }

            if (weightCoroutine != null) StopCoroutine(weightCoroutine);
            float target = b_IsMaskOn ? 1f : 0f;
            weightCoroutine = StartCoroutine(AnimateLayerWeight(target, 1.0f));
        }
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
