using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float moveDuration = 2f;
    public float stopDuration = 0.5f;
    public float rotationSpeed = 180f;
    public float health = 3f;

    public HealthBarController healthBar;
    public Animator[] allTrackAnimators;

    private float maxHealth;
    public float obstacleCheckDistance = 2f;
    public LayerMask obstacleMask;

    private Rigidbody2D rb;
    private float timer;
    private bool isMoving = true;
    private bool wasMoving = false;
    private float targetAngle;

    private AudioSource tracksAudio;
    private const float TRACKS_START = 3f;
    private const float TRACKS_END   = 8f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        maxHealth = health;

        if (allTrackAnimators == null || allTrackAnimators.Length == 0)
            allTrackAnimators = GetComponentsInChildren<Animator>();
        if (healthBar == null)
            healthBar = GetComponentInChildren<HealthBarController>();
        if (healthBar != null)
            healthBar.SetHealth(health, maxHealth);

        
        tracksAudio = gameObject.AddComponent<AudioSource>();
        tracksAudio.loop = false;
        tracksAudio.playOnAwake = false;
        tracksAudio.volume = 0.5f;
        if (TankAudioManager.Instance != null)
            tracksAudio.clip = TankAudioManager.Instance.tracksRollingClip;

        PickNewDirection();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (isMoving)
        {
            if (IsObstacleAhead()) StopAndTurn();
            if (timer >= moveDuration) StopAndTurn();
        }
        else
        {
            if (timer >= stopDuration)
            {
                PickNewDirection();
                if (IsObstacleAhead())
                    StopAndTurn();
            }
        }

        RotateTowardsTarget();

        foreach (Animator anim in allTrackAnimators)
            if (anim != null) anim.SetBool("isMoving", isMoving);

       
        if (tracksAudio != null)
        {
            if (isMoving && !wasMoving)
            {
                tracksAudio.time = TRACKS_START;
                tracksAudio.Play();
            }
            else if (!isMoving && wasMoving)
            {
                tracksAudio.Stop();
            }
            if (tracksAudio.isPlaying && tracksAudio.time >= TRACKS_END)
                tracksAudio.time = TRACKS_START;
        }
        wasMoving = isMoving;
    }

    void FixedUpdate()
    {
        if (isMoving) rb.linearVelocity = transform.up * moveSpeed;
        else rb.linearVelocity = Vector2.zero;
    }

    void StopAndTurn() { isMoving = false; timer = 0f; targetAngle = Random.Range(20f, 220f); }
    void PickNewDirection() { timer = 0f; isMoving = true; targetAngle = Random.Range(20f, 220f); }

    void RotateTowardsTarget()
    {
        float current = transform.eulerAngles.z;
        float newAngle = Mathf.MoveTowardsAngle(current, targetAngle, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, 0, newAngle);
    }

    bool IsObstacleAhead()
    {
        float radius = 0.4f;
        Vector2 origin = rb.position + (Vector2)transform.up * 0.2f;
        RaycastHit2D hit = Physics2D.CircleCast(origin, radius, transform.up, obstacleCheckDistance, obstacleMask);
        Debug.DrawRay(origin, transform.up * obstacleCheckDistance, Color.red);
        return hit.collider != null;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("TankShell"))
        {
            // Pouze střela hráče (layer TankShell) = layer 9
            if (collision.gameObject.layer == LayerMask.NameToLayer("TankShell"))
            {
                Destroy(collision.gameObject);
                float dmg = 0.5f;
                if (PlayerStats.instance != null) dmg = PlayerStats.instance.damage;
                health -= dmg;
                Debug.Log($"Enemy hit: -{dmg} HP ({health} remaining)");
                if (healthBar != null) healthBar.SetHealth(health, maxHealth);
                // ZVUK ZASAHU NEPRITELE
                if (TankAudioManager.Instance != null)
                    TankAudioManager.Instance.PlayArmorHit();
                if (health <= 0f) Die();
            }
            else
            {
                Destroy(collision.gameObject);
            }
        }
    }

    void Die()
    {
        if (tracksAudio != null && tracksAudio.isPlaying) tracksAudio.Stop();
        EnemyReward reward = GetComponent<EnemyReward>();
        if (reward != null) reward.GiveReward();
        if (LevelManager.Instance != null) LevelManager.Instance.EnemyKilled();
        if (healthBar != null) healthBar.SetHealth(0f, maxHealth);
        Destroy(gameObject);
    }
}
