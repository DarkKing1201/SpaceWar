using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class MainBossStats : MonoBehaviour, IHasHealth
{
    [Header("Health Settings")]
    [Tooltip("Maximum hit points of the boss.")]
    public float maxHP = 500000f;
    [SerializeField, Tooltip("Current HP at runtime.")]
    private float currentHP;
    [Header("Events")]
    public UnityEvent onDeath;
    [Header("Death VFX")]
    [Tooltip("Prefab to spawn when the boss is destroyed.")]
    public GameObject deathVFX;
    [Header("Weapon Control Reference")]
    [Tooltip("Reference to the WeaponDmgControl managing this boss's weapons.")]
    public WeaponDmgControl weaponDmgControl;
    [Header("Side Ships (must be destroyed before boss can take damage)")]
    // No need for a local sideShips list; will use GameManager's activeEnemyShips
    [Header("Boss Shield GameObject (disable to allow damage)")]
    public GameObject bossShield;
    private float lastHPThreshold = 0f;
    // --- Force respawn timer ---
    private float forceRespawnTimer = -1f;
    private const float FORCE_RESPAWN_DELAY = 10f;
    // --- Side ship respawn logic at specific HP thresholds ---
    private float[] sideShipRespawnThresholds = new float[] { 250000f, 100000f };
    private int nextSideShipRespawnIndex = 0;
    void Start()
    {
        currentHP = maxHP;
        // Set initial threshold to the next threshold below max HP
        lastHPThreshold = Mathf.Floor(maxHP / 100000f) * 100000f;
        // Ensure shield status is synced at start
        UpdateShieldStatus();
        nextSideShipRespawnIndex = 0;
    }
    private float lastWeaponCheckTime = 0f;
    private const float WEAPON_CHECK_INTERVAL = 1f; // Check weapons every 1 second instead of every frame
    private bool allWeaponsInactive = true; // Cache the result
    
    void Update()
    {
        UpdateShieldStatus();
        CheckWeaponRespawnByHP();
        CheckSideShipRespawnByHP();
        
        // DEBUG: Press 'K' to destroy all enemy side ships instantly
        if (Input.GetKeyDown(KeyCode.K))
        {
            DestroyAllSideShips();
        }
        
        // Only check weapons periodically to reduce performance impact
        if (Time.time - lastWeaponCheckTime >= WEAPON_CHECK_INTERVAL)
        {
            lastWeaponCheckTime = Time.time;
            
            // Check if all weapons are inactive - optimized with early exit
            bool weaponsInactive = true;
            if (weaponDmgControl != null)
            {
                // Small canons - early exit if any weapon is active
                if (weaponDmgControl.smallCanonManager != null)
                {
                    foreach (var canon in weaponDmgControl.smallCanonManager.canons)
                    {
                        if (canon != null && canon.gameObject.activeInHierarchy)
                        {
                            weaponsInactive = false;
                            break; // Exit early if we find an active weapon
                        }
                    }
                }
                
                // Only check turrets if small canons are inactive
                if (weaponsInactive && weaponDmgControl.turretsManager != null)
                {
                    foreach (var turret in weaponDmgControl.turretsManager.turrets)
                    {
                        if (turret != null && turret.gameObject.activeInHierarchy)
                        {
                            weaponsInactive = false;
                            break; // Exit early if we find an active weapon
                        }
                    }
                }
                
                // Only check big canons if other weapons are inactive
                if (weaponsInactive)
                {
                    BigCanon[] bigCanons = GetComponentsInChildren<BigCanon>(true);
                    foreach (var bigCanon in bigCanons)
                    {
                        if (bigCanon != null && bigCanon.gameObject.activeInHierarchy)
                        {
                            weaponsInactive = false;
                            break; // Exit early if we find an active weapon
                        }
                    }
                }
            }
            
            // Update cached result
            allWeaponsInactive = weaponsInactive;
            
            // Start or update the force respawn timer
            if (allWeaponsInactive && forceRespawnTimer < 0f)
            {
                forceRespawnTimer = FORCE_RESPAWN_DELAY;
            }
            else if (!allWeaponsInactive)
            {
                forceRespawnTimer = -1f; // Reset timer if any weapon is revived
            }
        }
        
        // Countdown and trigger force respawn if needed (still needs to run every frame for smooth countdown)
        if (forceRespawnTimer > 0f)
        {
            forceRespawnTimer -= Time.deltaTime;
            if (forceRespawnTimer <= 0f)
            {
                forceRespawnTimer = -1f;
                if (weaponDmgControl != null)
                {
                    weaponDmgControl.ReviveAllTurrets();
                    weaponDmgControl.ReviveAllCanons();
                    weaponDmgControl.ReviveAllBigCanons();
                }
            }
        }
    }
    public void TakeDamage(float amount)
    {
        if (amount <= 0 || currentHP <= 0)
            return;
        if (!AreAllSideShipsDestroyed())
        {
            return;
        }
        if (weaponDmgControl != null)
        {
            bool allWeaponsInactive = true;
            // Check small canons
            if (weaponDmgControl.smallCanonManager != null)
            {
                foreach (var canon in weaponDmgControl.smallCanonManager.canons)
                {
                    if (canon != null && canon.gameObject.activeInHierarchy)
                        allWeaponsInactive = false;
                }
            }
            // Check turrets
            if (weaponDmgControl.turretsManager != null)
            {
                foreach (var turret in weaponDmgControl.turretsManager.turrets)
                {
                    if (turret != null && turret.gameObject.activeInHierarchy)
                        allWeaponsInactive = false;
                }
            }
            // Check big canons
            BigCanon[] bigCanons = GetComponentsInChildren<BigCanon>(true);
            foreach (var bigCanon in bigCanons)
            {
                if (bigCanon != null && bigCanon.gameObject.activeInHierarchy)
                    allWeaponsInactive = false;
            }
            if (!allWeaponsInactive)
            {
                return;
            }
        }
        float oldHP = currentHP;
        currentHP -= amount;
        if (currentHP <= 0)
        {
            currentHP = 0;
            HandleDeath();
        }
        // Reset force respawn timer on damage
        forceRespawnTimer = -1f;
    }
    private void HandleDeath()
    {
        if (deathVFX != null)
        {
            GameObject vfx = Instantiate(deathVFX, transform.position, transform.rotation);
            Destroy(vfx, 5f);
        }
        onDeath?.Invoke();
        Destroy(gameObject);
        if (GameManager.Instance != null)
            GameManager.Instance.OnEnemyDestroyed(this.gameObject);
        if (GameManager.Instance != null)
            GameManager.Instance.CheckAllEnemiesDefeated();
    }
    // Check if all side ships are destroyed
    private bool AreAllSideShipsDestroyed()
    {
        if (GameManager.Instance == null)
            return true; // If no GameManager, assume all destroyed (fail-safe)
        var enemyShips = GameManager.Instance.GetActiveEnemyShips();
        foreach (var shipGO in enemyShips)
        {
            if (shipGO != null)
            {
                var stats = shipGO.GetComponent<EnemyStats>();
                if (stats != null && stats.CurrentHP > 0)
                    return false;
            }
        }
        return true;
    }
    // Enable/disable boss shield based on side ship status
    private void UpdateShieldStatus()
    {
        bool allDestroyed = AreAllSideShipsDestroyed();
        if (bossShield != null)
            bossShield.SetActive(!allDestroyed);
    }
    // Force weapon respawn every 100,000 HP lost
    private void CheckWeaponRespawnByHP()
    {
        // Calculate the current threshold based on HP
        float hpThreshold = Mathf.Floor(currentHP / 100000f) * 100000f;
        // Ensure we don't go below 0
        hpThreshold = Mathf.Max(hpThreshold, 0f);
        // Check if we've crossed a threshold (HP dropped below a 100k mark)
        if (hpThreshold < lastHPThreshold)
        {
            ForceRespawnAllWeapons();
            lastHPThreshold = hpThreshold;
        }
    }
    // Call respawn methods on WeaponDmgControl
    private void ForceRespawnAllWeapons()
    {
        if (weaponDmgControl != null)
        {
            weaponDmgControl.ReviveAllTurrets();
            weaponDmgControl.ReviveAllCanons();
            weaponDmgControl.ReviveAllBigCanons();
        }
        else
        {
        }
    }
    // Destroys all enemy side ships for testing
    private void DestroyAllSideShips()
    {
        if (GameManager.Instance == null) return;
        var enemyShips = GameManager.Instance.GetActiveEnemyShips();
        foreach (var shipGO in enemyShips)
        {
            if (shipGO != null)
            {
                var stats = shipGO.GetComponent<EnemyStats>();
                if (stats != null && stats.CurrentHP > 0)
                {
                    stats.TakeDamage(stats.CurrentHP);
                }
            }
        }
    }
    // --- Side ship respawn logic at specific HP thresholds ---
    private void CheckSideShipRespawnByHP()
    {
        if (nextSideShipRespawnIndex >= sideShipRespawnThresholds.Length)
            return;
        float threshold = sideShipRespawnThresholds[nextSideShipRespawnIndex];
        if (currentHP <= threshold && AreAllSideShipsDestroyed())
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RespawnEnemySideShips();
            }
            else
            {
            }
            // Reactivate shield (will be handled by UpdateShieldStatus in next frame)
            if (bossShield != null)
                bossShield.SetActive(true);
            nextSideShipRespawnIndex++;
        }
    }
    // Read-only accessors
    public float CurrentHP => currentHP;
    public float MaxHP => maxHP;
    // Debug method to manually trigger weapon respawn
    [ContextMenu("Debug: Force Weapon Respawn")]
    public void DebugForceWeaponRespawn()
    {
        ForceRespawnAllWeapons();
    }
    // Debug method to check weapon status
    [ContextMenu("Debug: Check Weapon Status")]
    public void DebugCheckWeaponStatus()
    {
        if (weaponDmgControl != null)
        {
            // Check turrets
            if (weaponDmgControl.turretsManager != null)
            {
                foreach (var turret in weaponDmgControl.turretsManager.turrets)
                {
                    if (turret != null)
                    {
                    }
                }
            }
            else
            {
            }
            // Check small canons
            if (weaponDmgControl.smallCanonManager != null)
            {
                foreach (var canon in weaponDmgControl.smallCanonManager.canons)
                {
                    if (canon != null)
                    {
                    }
                }
            }
            else
            {
            }
            // Check big canons
            BigCanon[] bigCanons = FindObjectsOfType<BigCanon>();
            foreach (var bigCanon in bigCanons)
            {
                if (bigCanon != null)
                {
                }
            }
        }
        else
        {
        }
    }
}
