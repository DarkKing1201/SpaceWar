using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;
using System.Collections;
[DisallowMultipleComponent]
public class PlaneStats : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum hit points of the plane.")]
    public int maxHP = 100;
    [SerializeField, Tooltip("Current HP at runtime.")]
    private int currentHP;
    [Header("Health Regeneration")]
    [Tooltip("Time in seconds without taking damage before regeneration starts")]
    [SerializeField] private float regenerationDelay = 3f;
    [Tooltip("Percentage of max HP regenerated per second")]
    [SerializeField] private float regenerationRate = 0.2f;
    private float lastDamageTime;
    [Header("Attack Settings")]
    [Tooltip("Base damage dealt by the plane's attack.")]
    public int attackPoint = 10;
    [Header("Events")]
    // public UnityEvent onDeath; // Removed, handled by GameManager
    [Header("Damage Control")]
    [Tooltip("If false, the plane will not take damage.")]
    public bool canTakeDamage = true;
    [Header("Collision Damage Control")]
    [Tooltip("Cooldown time in seconds between collision damage events")]
    [SerializeField] private float collisionDamageCooldown = 1f;
    private float lastCollisionDamageTime;
    // Debug state tracking
    private int lastLoggedHP = -1;
    void Awake()
    {
        currentHP = maxHP;
        lastDamageTime = Time.time;
        lastCollisionDamageTime = Time.time;
        // UpdateHpScreens(); // Removed UI logic
    }
    void Start()
    {
        // Only assign if not already set (allows for manual override if needed)
        // if (hpScreen == null)
        //     hpScreen = GameObject.Find("HpScreen");
        // if (playerLostHpScreen == null)
        //     playerLostHpScreen = GameObject.Find("PlayerLostHpScreen");
        // if (playerHpLowScreen == null)
        //     playerHpLowScreen = GameObject.Find("PlayerHpLowScreen");
        // UpdateHpScreens(); // Removed UI logic
    }
    /// <summary>Enable or disable taking damage.</summary>
    public void SetCanTakeDamage(bool value)
    {
        canTakeDamage = value;
    }
    /// <summary>Check if collision damage can be applied (respects cooldown).</summary>
    private bool CanTakeCollisionDamage()
    {
        return canTakeDamage && (Time.time - lastCollisionDamageTime >= collisionDamageCooldown);
    }
    /// <summary>Apply collision damage with cooldown protection.</summary>
    private void ApplyCollisionDamage(int amount)
    {
        if (CanTakeCollisionDamage())
        {
            lastCollisionDamageTime = Time.time;
            TakeDamage(amount);
        }
    }
    /// <summary>Inflict damage; fires onDeath if HP = 0.</summary>
    public void TakeDamage(int amount)
    {
        if (!canTakeDamage) return;
        if(amount <= 0 || currentHP <= 0)
            return;
        
        // Debug: Log damage source with timestamp (only when HP actually changes)
        if (lastLoggedHP != currentHP)
        {
            UnityEngine.Debug.Log($"[PlaneStats] HP DROP: Taking {amount} damage. HP: {currentHP} -> {currentHP - amount} at {Time.time:F2}");
            lastLoggedHP = currentHP;
        }
        
        currentHP -= amount;
        lastDamageTime = Time.time; // Update last damage time
        // UpdateHpScreens(); // Removed UI logic
        if(currentHP <= 0)
        {
            currentHP = 0;
            HandleDeath();
        }
    }
    /// <summary>Heal the plane up to maxHP.</summary>
    public void Heal(int amount)
    {
        if(amount <= 0 || currentHP >= maxHP)
            return;
        currentHP = Mathf.Min(currentHP + amount, maxHP);
        // UpdateHpScreens(); // Removed UI logic
    }
    private void HandleDeath()
    {
        // onDeath?.Invoke(); // Removed, handled by GameManager
        // Delegate all death handling to GameManager
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerDeath(this);
    }
#if UNITY_EDITOR
    void Update()
    {
        // Test key: Ctrl+Z to toggle canTakeDamage
        if (UnityEngine.Input.GetKey(KeyCode.LeftControl) && UnityEngine.Input.GetKeyDown(KeyCode.Z))
        {
            canTakeDamage = !canTakeDamage;
        }
        // Check if enough time has passed since last damage
        if (Time.time - lastDamageTime >= regenerationDelay && currentHP < maxHP)
        {
            // Calculate regeneration amount (20% of max HP per second)
            float regenerationAmount = maxHP * regenerationRate * Time.deltaTime;
            Heal(Mathf.RoundToInt(regenerationAmount));
        }
    }
#else
    void Update()
    {
        // Check if enough time has passed since last damage
        if (Time.time - lastDamageTime >= regenerationDelay && currentHP < maxHP)
        {
            // Calculate regeneration amount (20% of max HP per second)
            float regenerationAmount = maxHP * regenerationRate * Time.deltaTime;
            Heal(Mathf.RoundToInt(regenerationAmount));
        }
    }
#endif
    private void UpdateHpScreens()
    {
        // Removed UI logic
    }
    // Getters for current stats
    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;
    public int AttackPoint => attackPoint;
    public bool IsDead()
    {
        return currentHP <= 0 || !gameObject.activeInHierarchy;
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Turret"))
        {
            UnityEngine.Debug.Log($"[PlaneStats] Collision damage from: {collision.gameObject.tag} - {collision.gameObject.name}");
            ApplyCollisionDamage(maxHP); // Instantly die with cooldown protection
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Ground") || other.gameObject.CompareTag("Turret"))
        {
            UnityEngine.Debug.Log($"[PlaneStats] Trigger damage from: {other.gameObject.tag} - {other.gameObject.name}");
            ApplyCollisionDamage(maxHP); // Instantly die with cooldown protection
        }
    }
    // Additional collision detection methods for better coverage
    void OnCollisionStay(Collision collision)
    {
        // REMOVED: Continuous damage on stay - this was causing the VFX/damage disconnect issue!
        // Only handle instant death on Enter, not Stay to prevent continuous damage every frame
    }
    void OnTriggerStay(Collider other)
    {
        // REMOVED: Continuous damage on stay - this was causing the VFX/damage disconnect issue!
        // Only handle instant death on Enter, not Stay to prevent continuous damage every frame
    }
}
