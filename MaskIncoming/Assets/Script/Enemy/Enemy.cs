using System.Collections;
using Script.Enums;
using Script.Level;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float patrolSpeed = 2.0f;
    [SerializeField] private float chaseSpeed = 6.0f;
    [SerializeField] private float chaseRadius = 6.0f;

    [Header("Chase")]
    [SerializeField] private float blinkInterval = 0.15f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private GameObject player;
    private Vector2 movement;

    private Tile previousTile = null;
    private Tile currentTile;
    private Tile targetTile;

    private IMaze maze;

    private Vector2 startPosition;
    private Vector2 currentPosition;
    private Vector2 targetPosition;

    private const float arrivalThreshold = 0.1f;
    private enum EnemyState { Patrol, Chase, Idle }

    private EnemyState currentState;

    float distanceToPlayer = 0.0f;
    float distanceToTarget = 0.0f;

    [SerializeField] private GameEvent onPlayerDamage;

    private bool isPlayerMasked = false;
    private List<Tile> pathToPlayer = new List<Tile>();
    private Coroutine blinkCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody2D non trovato su " + gameObject.name);
            return;
        }

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player non trovato! Assicurati che abbia il tag 'Player'");
            return;
        }

        startPosition = transform.position;
        targetPosition = startPosition;
        currentState = EnemyState.Idle;
        Debug.Log($"Enemy " + gameObject.name + " inizializzato. Start: {startPosition}");
    }

    void Update()
    {
        if (maze == null)
        {
            Debug.LogError($"Maze not setted into enemy!!");
            return;
        }

        if (player == null)
        {
            Debug.LogError("Player non trovato! Assicurati che abbia il tag 'Player'");
            return;
        }

        currentPosition = transform.position;
        distanceToPlayer = Vector2.Distance(currentPosition, player.transform.position);
        distanceToTarget = Vector2.Distance(currentPosition, targetPosition);

        switch (currentState)
        {
            case EnemyState.Patrol:
                PatrolBehavior(currentPosition);
                break;

            case EnemyState.Idle:
                IdleBehaviour(currentPosition);
                break;

            case EnemyState.Chase:
                ChaseBehavior(currentPosition);
                break;

            default:
                break;
        }
        UpdateAnimator();
    }

    private bool ShouldChase()
    {
        return distanceToPlayer <= chaseRadius && isPlayerMasked;
    }

    void PatrolBehavior(Vector2 currentPos)
    {
        moveSpeed = patrolSpeed;
        Vector2 direction = (targetPosition - currentPos).normalized;
        movement = direction;

        if (ShouldChase())
        {
            Vector2Int playerCoords = maze.WorldToTile(player.transform.position);
            Tile playerTile = maze.GetTile(playerCoords.x, playerCoords.y);
            pathToPlayer = maze.Pathfinding(currentTile, playerTile);

            if (pathToPlayer.Count > 0)
            {
                targetTile = pathToPlayer[0];
                targetPosition = maze.TileToWorld(targetTile);
                EnterChase();
                return;
            }
        }

        if (distanceToTarget < arrivalThreshold)
        {
            previousTile = currentTile;
            currentTile = targetTile;
            currentState = EnemyState.Idle;
        }
    }

    void IdleBehaviour(Vector2 currentPos)
    {
        moveSpeed = 0.0f;
        movement = Vector2.zero;

        CalculateTargetTile();

        currentState = EnemyState.Patrol;
    }

    void ChaseBehavior(Vector2 currentPos)
    {
        moveSpeed = chaseSpeed;

        if (!ShouldChase())
        {
            ExitChase();
            return;
        }

        Vector2 direction = (targetPosition - currentPos).normalized;
        movement = direction;

        if (distanceToTarget < arrivalThreshold)
        {
            previousTile = currentTile;
            currentTile = targetTile;

            // Ricalcola il percorso ogni volta che arriva al centro di un tile
            Vector2Int playerCoords = maze.WorldToTile(player.transform.position);
            Tile playerTile = maze.GetTile(playerCoords.x, playerCoords.y);
            pathToPlayer = maze.Pathfinding(currentTile, playerTile);

            if (pathToPlayer.Count == 0)
            {
                ExitChase();
                return;
            }

            targetTile = pathToPlayer[0];
            targetPosition = maze.TileToWorld(targetTile);
        }
    }

    void EnterChase()
    {
        currentState = EnemyState.Chase;
        blinkCoroutine = StartCoroutine(BlinkLoop());
    }

    void ExitChase()
    {
        pathToPlayer.Clear();
        currentState = EnemyState.Patrol;

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        spriteRenderer.enabled = true;
    }

    IEnumerator BlinkLoop()
    {
        while (true)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(blinkInterval);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(blinkInterval);
        }
    }
    void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetFloat("Speed", movement.magnitude);
        animator.SetFloat("MoveX", movement.x);
        animator.SetFloat("MoveY", movement.y);
    }

    void FixedUpdate()
    {
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D non trovato in FixedUpdate su " + gameObject.name);
            return;
        }

        Vector2 newPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

    void CalculateTargetTile()
    {
        List<Tile> list = maze.GetNeighborsMinusPrevious(currentTile, previousTile).ToList();

        if (list.Count > 0)
        {
            targetTile = list[Random.Range(0, list.Count)];
            targetPosition = maze.TileToWorld(targetTile);
        }
        else
        {
            targetTile = currentTile;
            targetPosition = currentPosition;
            Debug.LogError("La lista per il prossimo Tile è vuota per l'enemy: " + gameObject.name);
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (onPlayerDamage)
            {
                onPlayerDamage.Raise();
                Destroy(this, 0.1f);
            }
            else
            {
                Debug.LogError("onPlayerDamage non settato per l'enemy: " + gameObject.name);
            }
        }
    }

    public void InitializeMazeData(IMaze _maze)
    {
        Vector2Int coords = _maze.WorldToTile(transform.position);
        currentTile = _maze.GetTile(coords.x, coords.y);
        maze = _maze;
    }

    public void OnPlayerMask()
    {
        isPlayerMasked = true;
    }

    public void OnPlayerUnmask()
    {
        isPlayerMasked = false;
    }
}