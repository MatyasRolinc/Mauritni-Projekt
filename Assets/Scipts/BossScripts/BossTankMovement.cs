using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossTankMovement : MonoBehaviour
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
        tracksAudio.volume = 0.7f;
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

        // Zvuk pohybu bosse
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

    void StopAndTurn() { isMoving = false; timer = 0f; targetAngle = Random.Range(0f, 360f); }
    void PickNewDirection()
    {
        timer = 0f;
        isMoving = false;

        const int maxAttempts = 20;
        for (int i = 0; i < maxAttempts; i++)
        {
            float newAngle = Random.Range(0f, 360f);
            if (!IsObstacleAhead(AngleToDirection(newAngle)))
            {
                targetAngle = newAngle;
                isMoving = true;
                return;
            }
        }

        // Pokud je cesta blokována ve všech směrech, zůstaň stát a zkus to znovu později.
        targetAngle = Random.Range(0f, 360f);
    }

    void RotateTowardsTarget()
    {
        float current = transform.eulerAngles.z;
        float newAngle = Mathf.MoveTowardsAngle(current, targetAngle, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, 0, newAngle);
    }

    bool IsObstacleAhead()
    {
        return IsObstacleAhead(transform.up);
    }

    bool IsObstacleAhead(Vector2 direction)
    {
        float radius = 0.4f;
        Vector2 origin = rb.position + (Vector2)transform.up * 0.2f;
        RaycastHit2D hit = Physics2D.CircleCast(origin, radius, direction, obstacleCheckDistance, obstacleMask);
        Debug.DrawRay(origin, direction * obstacleCheckDistance, Color.red);
        return hit.collider != null;
    }

    Vector2 AngleToDirection(float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Sin(radians), Mathf.Cos(radians));
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("TankShell"))
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("TankShell"))
            {
                Destroy(collision.gameObject);
                float dmg = 0.5f;
                if (PlayerStats.instance != null) dmg = PlayerStats.instance.damage;
                health -= dmg;
                if (healthBar != null) healthBar.SetHealth(health, maxHealth);

                // Zvuk zasahu bosse
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
