using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TargetLockUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject normalUI; // UI element to show when no target is locked
    public GameObject machineGunUI; // Renamed from targetLockUI, used for both target lock and machine gun aim
    public GameObject missileLockUI; // UI element to show when target is in missile lock range
    public TMP_Text laserRangeText; // TMP text to notify if laser is in fire range
    public float blinkInterval = 0.5f; // How fast the text blinks
    [Header("AutoTarget Reference")]
    public AutoTargetLock autoTargetLock; // Reference to the auto target lock system
    [Header("Weapon Reference")]
    public MachineGunControl machineGunControl; // Reference to the machine gun control
    public PlayerWeaponManager weaponManager; // Reference to the weapon manager
    public MissileLaunch missileLaunch; // Reference to the missile launch system
    public LaserActive laserActive; // Reference to the laser weapon logic
    [Header("Debug")]
    public bool showDebugLogs = true;
    [Header("Cheat/Debug UI")]
    public TMP_Text CheatHp; // Assign this to the yellow CheatHp text in the inspector
    private float cheatHpDisplayTimer = 0f;
    private const float cheatHpDisplayDuration = 1f;
    private PlaneStats playerStats;
    private bool lastCanTakeDamage = true;
    [Header("Missile Mode UI")]
    public TMP_Text MissileModeText; // Assign this to the missile mode TMP text in the inspector
    private float missileModeDisplayTimer = 0f;
    private const float missileModeDisplayDuration = 1f;
    private bool isLaserInRange = false;
    private float blinkTimer = 0f;
    private bool referencesChecked = false;
    void Update()
    {
        // Always check and assign weapon references at runtime
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            if (machineGunControl == null)
                machineGunControl = player.GetComponent<MachineGunControl>();
            if (weaponManager == null)
                weaponManager = player.GetComponent<PlayerWeaponManager>();
            if (missileLaunch == null)
                missileLaunch = player.GetComponent<MissileLaunch>();
            if (laserActive == null)
                laserActive = player.GetComponent<LaserActive>();
            if (playerStats == null || playerStats.gameObject != player)
                playerStats = player.GetComponent<PlaneStats>();
        }
        // Always check and assign autoTargetLock at runtime
		if (autoTargetLock == null)
		{
			autoTargetLock = FindObjectOfType<AutoTargetLock>();
		}
		// Reference checking logic
		if (!referencesChecked)
		{
			bool allAssigned = true;
			referencesChecked = allAssigned;
		}
		// --- Machine Gun UI logic ---
		bool inFireRange = false;
		if (machineGunUI != null && weaponManager != null)
		{
			inFireRange = weaponManager.IsTargetInRange(weaponManager.machineGunFireRange);
			machineGunUI.SetActive(inFireRange);
		}
        // --- Missile UI logic ---
        bool missileInRange = false;
        if (missileLockUI != null && weaponManager != null)
        {
            missileInRange = weaponManager.IsTargetInRange(weaponManager.missileFireRange);
            missileLockUI.SetActive(missileInRange);
        }
        // --- Laser UI logic ---
        // isLaserInRange is already set by the laser range notification logic
        // --- Normal UI logic ---
        if (normalUI != null)
            normalUI.SetActive(!(inFireRange || missileInRange || isLaserInRange));
        // If we don't have PlayerWeaponManager reference yet, keep searching
        if (weaponManager == null)
        {
            weaponManager = FindObjectOfType<PlayerWeaponManager>();
            if (weaponManager == null)
            {
                return;
            }
        }
        // Machine gun UI logic: check if raycast from crosshair hits enemy within range
        bool mgInRange = false;
        if (weaponManager != null)
        {
            Ray ray = weaponManager.GetCurrentTargetRay();
            RaycastHit hit;
            // Use the same layer mask as PlayerWeaponManager for consistency
            LayerMask targetableLayers = weaponManager.GetTargetableLayers();
            if (Physics.Raycast(ray, out hit, weaponManager.machineGunFireRange, targetableLayers))
            {
				if (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("Turret"))
				{
					float hitDistance = hit.distance;
					mgInRange = hitDistance <= weaponManager.machineGunFireRange;
				}
            }
        }
        // Update machine gun UI
        bool targetUIActive = machineGunUI != null && machineGunUI.activeInHierarchy;
        bool normalUIActive = normalUI != null && normalUI.activeInHierarchy;
        if (mgInRange && !targetUIActive)
        {
            UpdateUI(true);
        }
        else if (!mgInRange && !normalUIActive)
        {
            UpdateUI(false);
        }
        // Missile lock UI logic (still depends on AutoTargetLock)
        bool missileUIActive = missileLockUI != null && missileLockUI.activeInHierarchy;
        if (missileLaunch != null && !missileLaunch.useAutoTargetLock)
        {
            // Dumb-fire mode: UI on if any enemy is in missile fire range AND within lockCircleRadius of center
            bool enemyInView = false;
            if (autoTargetLock == null)
                autoTargetLock = FindObjectOfType<AutoTargetLock>();
            if (autoTargetLock != null)
            {
                foreach (string tag in autoTargetLock.targetTags)
                {
                    GameObject[] candidates = GameObject.FindGameObjectsWithTag(tag);
                    foreach (GameObject obj in candidates)
                    {
                        if (obj == null) continue;
                        float distance = Vector3.Distance(missileLaunch.transform.position, obj.transform.position);
                        if (distance <= weaponManager.missileFireRange)
                        {
                            // Check if in lock circle radius
                            Vector3 viewportPos = Camera.main.WorldToViewportPoint(obj.transform.position);
                            float distFromCenter = Vector2.Distance(new Vector2(viewportPos.x, viewportPos.y), new Vector2(0.5f, 0.5f));
                            if (distFromCenter <= autoTargetLock.lockCircleRadius)
                            {
                                enemyInView = true;
                                break;
                            }
                        }
                    }
                    if (enemyInView) break;
                }
            }
            if (enemyInView && !missileUIActive)
        {
            UpdateMissileUI(true);
        }
            else if (!enemyInView && missileUIActive)
            {
                UpdateMissileUI(false);
            }
        }
        else
        {
            // Lock-on mode: only show UI if target is locked AND inside lock circle
            if (autoTargetLock == null)
            {
                autoTargetLock = FindObjectOfType<AutoTargetLock>();
                if (autoTargetLock == null)
                {
                    return;
                }
            }
            bool hasTarget = autoTargetLock.HasTarget();
            bool targetInLockCircle = autoTargetLock.isTargetInLockCircle;
            bool inMissileRange = false;
            if (hasTarget && autoTargetLock.lockedTarget != null && weaponManager != null)
            {
                Vector3 missileFromPos = missileLaunch != null ? missileLaunch.transform.position : transform.position;
                float missileDistance = Vector3.Distance(missileFromPos, autoTargetLock.lockedTarget.position);
                inMissileRange = missileDistance <= weaponManager.missileFireRange;
            }
            // Only show missile UI if target is locked, in range, AND inside lock circle
            if (hasTarget && inMissileRange && targetInLockCircle && !missileUIActive)
            {
                UpdateMissileUI(true);
            }
            else if ((!hasTarget || !inMissileRange || !targetInLockCircle) && missileUIActive)
			{
				UpdateMissileUI(false);
			}
        }
        // Laser range notification logic
        if (laserActive == null)
        {
            isLaserInRange = false;
        }
        else
        {
            float range = laserActive.CurrentBeamLength;
            Ray ray = laserActive.weaponManager != null ? laserActive.weaponManager.GetCurrentTargetRay() : Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            bool inRange = false;
            if (Physics.Raycast(ray, out hit, range))
            {
                if (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("Turret"))
                {
                    inRange = true;
                }
            }
            else
            {
				// No hit within range
			}
            isLaserInRange = inRange;
        }
        // Blinking TMP text logic
        if (laserRangeText != null)
        {
            if (isLaserInRange)
            {
                blinkTimer += Time.deltaTime;
                if (blinkTimer >= blinkInterval)
                {
                    laserRangeText.enabled = !laserRangeText.enabled;
                    blinkTimer = 0f;
                }
                laserRangeText.text = "Laser in Fire Range!";
            }
            else
            {
                laserRangeText.enabled = false;
                blinkTimer = 0f;
            }
        }
        // Toggle canTakeDamage with Left Ctrl + J
        if (playerStats != null && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.J))
        {
            playerStats.canTakeDamage = !playerStats.canTakeDamage;
            lastCanTakeDamage = playerStats.canTakeDamage;
            if (CheatHp != null)
            {
                CheatHp.gameObject.SetActive(true);
                CheatHp.text = "CAN TAKE DAMAGE: " + (playerStats.canTakeDamage ? "ON" : "OFF");
                cheatHpDisplayTimer = cheatHpDisplayDuration;
            }
        }
        // Handle CheatHp text display timer
        if (CheatHp != null && CheatHp.gameObject.activeSelf)
        {
            if (cheatHpDisplayTimer > 0f)
            {
                cheatHpDisplayTimer -= Time.deltaTime;
                if (cheatHpDisplayTimer <= 0f)
                {
                    CheatHp.gameObject.SetActive(false);
                }
            }
        }
        // Missile mode display timer logic
        if (MissileModeText != null && MissileModeText.gameObject.activeSelf)
        {
            if (missileModeDisplayTimer > 0f)
            {
                missileModeDisplayTimer -= Time.deltaTime;
                if (missileModeDisplayTimer <= 0f)
                {
                    MissileModeText.gameObject.SetActive(false);
                }
            }
        }
    }
    private void ConnectToAutoTargetLock()
    {
        if (autoTargetLock == null) return;
        // Subscribe to target lock events
        autoTargetLock.OnTargetLocked += OnTargetLocked;
        autoTargetLock.OnTargetLost += OnTargetLost;
    }
    private void OnTargetLocked(Transform target)
    {
        UpdateUI(true);
    }
    private void OnTargetLost(Transform target)
    {
        UpdateUI(false);
    }
    private void UpdateUI(bool targetLocked)
    {
        // Enable/disable Normal UI
        if (normalUI != null)
        {
            normalUI.SetActive(!targetLocked);
        }
        // Enable/disable Machine Gun UI
        if (machineGunUI != null)
        {
            machineGunUI.SetActive(targetLocked);
        }
    }
    private void UpdateMissileUI(bool missileLocked)
    {
        if (missileLockUI != null)
        {
            missileLockUI.SetActive(missileLocked);
        }
    }
    // Public methods for manual control (if needed)
    public void ForceShowNormal()
    {
        UpdateUI(false);
    }
    public void ForceShowTargetLock()
    {
        UpdateUI(true);
    }
    // Method to manually set the AutoTargetLock reference (called by GameManager)
    public void SetAutoTargetLock(AutoTargetLock targetLock)
    {
        // Unsubscribe from old events
        if (autoTargetLock != null)
        {
            autoTargetLock.OnTargetLocked -= OnTargetLocked;
            autoTargetLock.OnTargetLost -= OnTargetLost;
        }
        // Set new reference
        autoTargetLock = targetLock;
        // Connect to new AutoTargetLock
        if (autoTargetLock != null)
        {
            ConnectToAutoTargetLock();
        }
    }
    public void SetTargetLockUI(RectTransform uiElement)
    {
        if (uiElement != null)
        {
            machineGunUI = uiElement.gameObject;
        }
    }
    /// <summary>
    /// Call this function to display the current missile mode on screen for 1 second.
    /// </summary>
    public void ShowMissileMode()
    {
        if (MissileModeText != null && missileLaunch != null)
        {
            MissileModeText.gameObject.SetActive(true);
            MissileModeText.color = Color.red;
            if (missileLaunch.useAutoTargetLock)
                MissileModeText.text = "Missile Mode: Auto Target Lock";
            else
                MissileModeText.text = "Missile Mode: Straight";
            missileModeDisplayTimer = missileModeDisplayDuration;
        }
    }
} 
