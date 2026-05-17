using UnityEngine;

public class TurrentScript : MonoBehaviour
{
    public GameObject TankShellPrefab;
    public GameObject fireEffectPrefab;
    public float fireEffectDuration = 0.5f;
    public Transform spawnPoint;
    public float angleOffset = 0f;

    private PlayerStats stats;
    private float nextFireTime = 0f;

    void Start()
    {
        if (PlayerStats.instance != null)
            stats = PlayerStats.instance;
        else
            Debug.LogWarning("TurretScript: PlayerStats.instance nebyl nalezen!");
        nextFireTime = 0f;
    }

    void Update()
    {
        Turn();
        Fire();
    }

    public void Turn()
    {
        if (Camera.main == null) return;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + angleOffset;
        transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
    }

    public void Fire()
    {
        if (Input.GetMouseButtonDown(0))
        {
            float useReload = (stats != null) ? stats.reloadTime : 0.75f;
            float useShellSpeed = (stats != null) ? stats.shellSpeed : 10f;
            if (Time.time < nextFireTime) return;
            if (TankShellPrefab != null && spawnPoint != null)
            {
                GameObject shell = Instantiate(TankShellPrefab, spawnPoint.position, spawnPoint.rotation);
                Rigidbody2D rbShell = shell.GetComponent<Rigidbody2D>();
                shell.layer = LayerMask.NameToLayer("TankShell");
                if (rbShell != null)
                    rbShell.linearVelocity = spawnPoint.up * useShellSpeed;
                if (fireEffectPrefab != null)
                {
                    GameObject effect = Instantiate(fireEffectPrefab, spawnPoint.position, spawnPoint.rotation);
                    Destroy(effect, fireEffectDuration);
                }
                // ZVUK VYSTRELU
                if (TankAudioManager.Instance != null)
                    TankAudioManager.Instance.PlayCannonFire();
            }
            nextFireTime = Time.time + useReload;
        }
    }

    public float RemainingReloadTime()
    {
        return Mathf.Max(0f, nextFireTime - Time.time);
    }
}
