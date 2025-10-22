using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class WeaponDmgControl : MonoBehaviour
{
    [Header("Bullet Turret Settings")]
    [Tooltip("Damage dealt by a single turret bullet.")]
    [SerializeField] private float bulletDamage = 20f;
    [Tooltip("Time between each shot for a turret.")]
    [SerializeField] private float turretFireRate = 0.1f;
    [Tooltip("The range at which a turret will start detecting and firing at players.")]
    [SerializeField] private float turretFireRange = 100f;
    [Header("Laser Canon Settings")]
    [Tooltip("Damage dealt per second by the laser canon.")]
    [SerializeField] private float smallCanonDamage = 50f;
    [Tooltip("Time between each shot for the laser canon.")]
    [SerializeField] private float smallCanonFireRate = 0.05f;
    [Tooltip("The range at which a laser canon will detect, fire, and reach.")]
    [SerializeField] private float smallCanonFireRange = 100f;
    [Header("Big Canon Settings")]
    [Tooltip("Damage dealt per second by the big laser canon.")]
    [SerializeField] private float bigCanonDamage = 100f;
    [Tooltip("Time between each shot for the big laser canon.")]
    [SerializeField] private float bigCanonFireRate = 0.1f;
    [Tooltip("The range at which a big laser canon will detect, fire, and reach.")]
    [SerializeField] private float bigCanonFireRange = 200f;
    // Revive system for turrets
    [Header("Turret Revive System")]
    public TurretsManager turretsManager;
    public float turretReviveTime = 60f;
    private float turretReviveTimer = 0f;
    private bool turretReviveTimerRunning = false;
    // Revive system for cannons
    [Header("Cannon Revive System")]
    public SmallCanonManager smallCanonManager;
    public float cannonReviveTime = 60f;
    private float cannonReviveTimer = 0f;
    private bool cannonReviveTimerRunning = false;
    // Revive system for big cannons
    [Header("Big Cannon Revive System")]
    public float bigCannonReviveTime = 90f; // Longer revive time for big cannon
    private float bigCannonReviveTimer = 0f;
    private bool bigCannonReviveTimerRunning = false;
    // Start is called before the first frame update
    void Start()
    {
        // No initialization needed; managers are assigned via inspector
    }
    void Update()
    {
        // Turret revive timer
        if (turretReviveTimerRunning)
        {
            turretReviveTimer -= Time.deltaTime;
            if (turretReviveTimer <= 0f)
            {
                turretReviveTimerRunning = false;
                if (turretsManager != null && turretsManager.currentTurretCount > 0)
                {
                    ReviveAllTurrets();
                }
            }
        }
        // Cannon revive timer
        if (cannonReviveTimerRunning)
        {
            cannonReviveTimer -= Time.deltaTime;
            if (cannonReviveTimer <= 0f)
            {
                cannonReviveTimerRunning = false;
                if (smallCanonManager != null && smallCanonManager.currentCanonCount > 0)
                {
                    ReviveAllCanons();
                }
            }
        }
        // Big cannon revive timer
        if (bigCannonReviveTimerRunning)
        {
            bigCannonReviveTimer -= Time.deltaTime;
            if (bigCannonReviveTimer <= 0f)
            {
                bigCannonReviveTimerRunning = false;
                ReviveAllBigCanons();
            }
        }
    }
    // --- Turret Getters ---
    public float GetBulletDamage()
    {
        return bulletDamage;
    }
    public float GetTurretFireRate()
    {
        return turretFireRate;
    }
    public float GetTurretFireRange()
    {
        return turretFireRange;
    }
    // --- Small Canon Getters ---
    public float GetSmallCanonDamage()
    {
        return smallCanonDamage;
    }
    public float GetSmallCanonFireRate()
    {
        return smallCanonFireRate;
    }
    public float GetSmallCanonFireRange()
    {
        return smallCanonFireRange;
    }
    // --- Big Canon Getters ---
    public float GetBigCanonDamage()
    {
        return bigCanonDamage;
    }
    public float GetBigCanonFireRate()
    {
        return bigCanonFireRate;
    }
    public float GetBigCanonFireRange()
    {
        return bigCanonFireRange;
    }
    // Method to set bullet damage
    public void SetBulletDamage(float damage)
    {
        bulletDamage = damage;
        // No need to update turrets, they will always use GetBulletDamage()
    }
    // Call this when a turret is destroyed
    public void OnTurretDestroyed()
    {
        if (turretsManager != null)
            turretsManager.currentTurretCount = Mathf.Max(turretsManager.currentTurretCount - 1, 0);
        if (!turretReviveTimerRunning)
        {
            turretReviveTimer = turretReviveTime;
            turretReviveTimerRunning = true;
        }
    }
    // Call this when a cannon is destroyed
    public void OnCanonDestroyed()
    {
        if (smallCanonManager != null)
        {
            smallCanonManager.currentCanonCount = Mathf.Max(smallCanonManager.currentCanonCount - 1, 0);
        }
        if (!cannonReviveTimerRunning)
        {
            cannonReviveTimer = cannonReviveTime;
            cannonReviveTimerRunning = true;
        }
    }
    // Call this when a big cannon is destroyed
    public void OnBigCanonDestroyed()
    {
        if (!bigCannonReviveTimerRunning)
        {
            bigCannonReviveTimer = bigCannonReviveTime;
            bigCannonReviveTimerRunning = true;
        }
    }
    public void ReviveAllTurrets()
    {
        if (turretsManager == null || turretsManager.turrets == null || turretsManager.turrets.Count == 0)
        {
            return;
        }
        foreach (var turret in turretsManager.turrets)
        {
            if (turret != null)
            {
                turret.currentHP = turretsManager.turretHP;
                turret.gameObject.SetActive(true);
            }
        }
        turretsManager.currentTurretCount = turretsManager.maxTurretCount;
    }
    public void ReviveAllCanons()
    {
        if (smallCanonManager == null || smallCanonManager.canons == null || smallCanonManager.canons.Count == 0)
        {
            return;
        }
        foreach (var canon in smallCanonManager.canons)
        {
            if (canon != null)
            {
                canon.currentHP = smallCanonManager.canonHP;
                canon.gameObject.SetActive(true);
            }
        }
        smallCanonManager.currentCanonCount = smallCanonManager.maxCanonCount;
    }
    public void ReviveAllBigCanons()
    {
        BigCanon[] bigCanons = GameObject.FindObjectsOfType<BigCanon>(true);
        foreach (var bigCanon in bigCanons)
        {
            if (bigCanon != null)
            {
                bigCanon.currentHP = bigCanon.maxHP;
                bigCanon.gameObject.SetActive(true);
            }
        }
    }
}
