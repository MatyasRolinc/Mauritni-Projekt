using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UpgradeMenuUIScript : MonoBehaviour
{
    private PlayerStats playerStats;
    private bool initialRefreshDone = false;

    [Header("UI Texty")]
    public TextMeshProUGUI hpTMP;
    public TextMeshProUGUI speedTMP;
    public TextMeshProUGUI reloadTMP;
    public TextMeshProUGUI shellSpeedTMP;
    public TextMeshProUGUI damageTMP;

    [Header("Ceny")]
    public int healthCost = 50;
    public int speedCost = 40;
    public int reloadCost = 50;
    public int shellSpeedCost = 40;
    public int damageCost = 100;

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Zavolá se po dokončení načtení scény – PlayerStats už existuje
        initialRefreshDone = false;
        Invoke(nameof(TryRefresh), 0.05f);
    }

    void Start()
    {
        initialRefreshDone = false;
        Invoke(nameof(TryRefresh), 0.05f);
    }

    void Update()
    {
        if (!initialRefreshDone && PlayerStats.instance != null)
        {
            playerStats = PlayerStats.instance;
            RefreshUI();
            initialRefreshDone = true;
        }
    }

    void TryRefresh()
    {
        if (initialRefreshDone) return;
        if (PlayerStats.instance != null)
        {
            playerStats = PlayerStats.instance;
            RefreshUI();
            initialRefreshDone = true;
        }
        else
        {
            // Zkusit znovu za dalších 0.1s
            Invoke(nameof(TryRefresh), 0.1f);
        }
    }

    public void RefreshUI()
    {
        if (playerStats == null) playerStats = PlayerStats.instance;
        if (playerStats == null) return;

        if (hpTMP != null)         hpTMP.text         = playerStats.maxHealth.ToString();
        if (speedTMP != null)      speedTMP.text      = playerStats.moveSpeed.ToString("F1");
        if (reloadTMP != null)     reloadTMP.text     = playerStats.reloadTime.ToString("F2");
        if (shellSpeedTMP != null) shellSpeedTMP.text = playerStats.shellSpeed.ToString("F1");
        if (damageTMP != null)     damageTMP.text     = playerStats.damage.ToString();

        playerStats.UpdateUI();
    }

    public void BuyHealthUpgrade()
    {
        if (CheckStats()) playerStats.UpgradeMaxHealth(healthCost);
        RefreshUI();
    }

    public void BuySpeedUpgrade()
    {
        if (CheckStats()) playerStats.UpgradeMoveSpeed(speedCost);
        RefreshUI();
    }

    public void BuyReloadUpgrade()
    {
        if (CheckStats()) playerStats.UpgradeReloadTime(reloadCost);
        RefreshUI();
    }

    public void BuyShellSpeedUpgrade()
    {
        if (CheckStats()) playerStats.UpgradeShellSpeed(shellSpeedCost);
        RefreshUI();
    }

    public void BuyDamageUpgrade()
    {
        if (CheckStats()) playerStats.UpgradeDamage(damageCost);
        RefreshUI();
    }

    public void ClickNextLevel()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.LoadNextLevel();
        else
        {
            LevelManager lm = FindFirstObjectByType<LevelManager>();
            if (lm != null) lm.LoadNextLevel();
        }
    }

    private bool CheckStats()
    {
        if (playerStats == null) playerStats = PlayerStats.instance;
        return playerStats != null;
    }
}
