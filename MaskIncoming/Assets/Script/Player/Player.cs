using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
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

    [Header("UI Health")]
    [SerializeField] private Image[] heartIcons;
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;
    [SerializeField] private Color deadHeartColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

    [Header("Collectables UI")]
    [SerializeField] private GameObject keyIconUI;
    [SerializeField] private GameObject[] salvationToolIcons;

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

    private int m_CurrentHealth;
    private float targetLayerWeight = 0f;
    private bool b_HasKeyCollectable = false;
    private int m_SalvationTools = 0;
    private Well currentWell;
    private bool m_CanEscape = false;

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

        m_CurrentHealth = heartIcons.Length;
        UpdateHealthUI();

        if (keyIconUI != null) keyIconUI.SetActive(false);
        UpdateSalvationUI();
    }

    void Update()
    {
        // stamina
        if (b_IsMaskOn)
        {
            currentStamina -= consumeSpeed * Time.deltaTime;
            
            if (currentStamina <= 0)
            {
                // todo: no more mask animation
                currentStamina = 0;
                DeactivatePower();

                targetLayerWeight = 0f;

                TakeDamage();
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

        float currentW = m_Animator.GetLayerWeight(m_SpecialLayerIndex);

        float newW = Mathf.MoveTowards(currentW, targetLayerWeight, Time.deltaTime * 5f);
        m_Animator.SetLayerWeight(m_SpecialLayerIndex, newW);

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
                ActivatePower();
            }
            else if(b_IsMaskOn)
            {
                b_IsMaskOn = false;
                lastTimeMaskDeactivated = Time.time;
                DeactivatePower();
                if(OnMaskDeactivation)
                    OnMaskDeactivation.Raise();
            }

            targetLayerWeight = b_IsMaskOn ? 1f : 0f;
        }
    }

    void ActivatePower()
    {
        b_IsMaskOn = true;
        if(OnMaskActivation)
            OnMaskActivation.Raise();
    }

    void DeactivatePower()
    {
        b_IsMaskOn = false;
        lastTimeMaskDeactivated = Time.time;
        if(OnMaskDeactivation)
            OnMaskDeactivation.Raise();
    }

    // Lifes and Deaths

    public void TakeDamage()
    {
        GetComponent<SpriteRenderer>().color = Color.red;
        Invoke("ResetColor", 0.5f); 

        if (m_CurrentHealth > 0)
        {
            m_CurrentHealth -= 1;
            UpdateHealthUI();
        }
        if (m_CurrentHealth <= 0)
        {
            if(b_IsMaskOn) 
            {
                // todo: Special death
                Debug.Log("Morte speciale con maschera!");
                SpecialDeath();
            } 
            else 
            {
                // todo: normal death
                Debug.Log("Morte normale!");
                NormalDeath();
            }
        }
    }

    void ResetColor() { GetComponent<SpriteRenderer>().color = Color.white; }

    private void UpdateHealthUI()
    {
        Color horrorRed;
        ColorUtility.TryParseHtmlString("#5a0000", out horrorRed);
        
        for (int i = 0; i < heartIcons.Length; i++)
        {
            if (i < m_CurrentHealth)
            {
                heartIcons[i].sprite = fullHeartSprite;
                heartIcons[i].color = horrorRed;
            }
            else
            {
                heartIcons[i].sprite = emptyHeartSprite;
                heartIcons[i].color = deadHeartColor;
            }
        }
    }

    private void SpecialDeath()
    {
        
    }

    private void NormalDeath()
    {

    }

    // Collectables

    public void PickKey()
    {
        b_HasKeyCollectable = true;
        if (keyIconUI != null) keyIconUI.SetActive(true);
    }

    public bool HasKey()
    {
        return b_HasKeyCollectable;
    }

    public void OnInteract(InputValue value)
    {
        if (value.isPressed && m_CanEscape)
        {
            TryEscape();
        }
    }

    public void SetCurrentWell(Well well) { currentWell = well; }

    public void PickSalvationTool()
    {
        if (m_SalvationTools < 3)
        {
            m_SalvationTools += 1;
            UpdateSalvationUI();
        }
    }

    private void UpdateSalvationUI()
    {
        for (int i = 0; i < salvationToolIcons.Length; i++)
        {
            if (i < m_SalvationTools)
            {
                salvationToolIcons[i].SetActive(true);
            }
            else
            {
                salvationToolIcons[i].SetActive(false);
            }
        }
    }

    // Wins

    private void TryEscape()
    {
        if (b_HasKeyCollectable)
        {
            Debug.Log("VITTORIA! Il Player ha usato la chiave ed è scappato.");
        }
        else
        {
            Debug.Log("Il Player prova ad aprire ma non ha la chiave...");
        }
    }

    public void SetCanEscape(bool canEscape)
    {
        m_CanEscape = canEscape;
    }
}
