using UnityEngine;

public class BossTankTurretScript : MonoBehaviour
{
    private Transform target;
    public Transform firePoint;
    public GameObject shellPrefab;
    [Header("Animation / VFX")]
    public Animator animator;
    public string fireTriggerName = "Fire";
    public GameObject muzzleFlashPrefab;
    public float muzzleFlashDuration = 0.6f;
    public float shellSpeed = 8f;
    public int damage = 1;

    public float rotationSpeed = 80f;
    public float angleOffset = -90f;
    public float shootInterval = 7f;
    private float nextFireTime = 0f;

    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player) target = player.transform;
        }
        if (firePoint == null) firePoint = transform;
        nextFireTime = Time.time + Random.Range(0f, shootInterval);
    }

    void Update()
    {
        if (!target) return;

        Vector2 dir = (Vector2)(target.position - transform.position);
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + angleOffset;
        float newAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetAngle, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, newAngle);

        Vector2 origin = firePoint.position;
        Vector2 toPlayer = (Vector2)(target.position - firePoint.position);
        float dist = toPlayer.magnitude;
        if (dist <= 0f) return;

        RaycastHit2D hit = Physics2D.Raycast(origin, toPlayer.normalized, dist);
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            if (Time.time >= nextFireTime)
            {
                Shoot();
                PlayFireAnimation(firePoint);
                nextFireTime = Time.time + shootInterval;
            }
        }
    }

    void Shoot()
    {
        if (shellPrefab == null || firePoint == null) return;

        GameObject shell = Instantiate(shellPrefab, firePoint.position, firePoint.rotation);
        var shellScript = shell.GetComponent<TankShellScript>();
        if (shellScript != null) shellScript.damage = damage;

        Rigidbody2D rb = shell.GetComponent<Rigidbody2D>();
        shell.layer = LayerMask.NameToLayer("Enemy");
        if (rb != null) rb.linearVelocity = (Vector2)firePoint.up * shellSpeed;

        Destroy(shell, 8f);

        // Zvuk vystrelu bosse
        if (TankAudioManager.Instance != null)
            TankAudioManager.Instance.PlayCannonFire();
    }

    public void PlayFireAnimation(Transform fp)
    {
        if (animator != null && !string.IsNullOrEmpty(fireTriggerName))
            animator.SetTrigger(fireTriggerName);

        if (muzzleFlashPrefab != null && fp != null)
        {
            GameObject fx = Instantiate(muzzleFlashPrefab, fp.position, fp.rotation);
            var ps = fx.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
            Destroy(fx, muzzleFlashDuration);
        }
    }
}
