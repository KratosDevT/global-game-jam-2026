using UnityEngine;

public class Enemy : MonoBehaviour
{    

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float detectionRange = 5.0f;
    
    [Header("Combat")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackCooldown = 2.0f;
    
    // private GameObject player;
    // private Transform playerTransform;
    private Rigidbody2D rb;
    private Vector2 movement;
    private float lastAttackTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // player = GameObject.FindGameObjectWithTag("Player");
        // if (player)
        // {
        //     playerTransform = player.Transform;
        // }
    }

    void Update()
    {
        // if(playerTransform)
        // {
        //     float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            
        //     if (distanceToPlayer <= detectionRange)
        //     {
        //         Vector2 direction = (playerTransform.position - transform.position).normalized;
        //         movement = direction;
        //     }
        //     else
        //     {
        //         movement = Vector2.zero;
        //     }
        // }
    }

    void FixedUpdate()
    {
        // rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                //PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                // if (playerHealth != null)
                // {
                //     playerHealth.TakeDamage(damage);
                //     lastAttackTime = Time.time;
                // }
            }
        }
    }
}
