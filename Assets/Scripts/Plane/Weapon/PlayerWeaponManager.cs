using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class PlayerWeaponManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform targetLockUI; // The UI element for targeting
    [SerializeField] private Camera mainCamera; // Reference to main camera
    [Header("Weapon Ranges")]
    public float machineGunFireRange = 1000f;
    public float missileFireRange = 800f;
    [Header("Targeting")]
    [Tooltip("Layers that can be targeted by player weapons")]
    [SerializeField] private LayerMask targetableLayers = 1; // Default to "Default" layer (layer 0)
    [Header("Machine Gun Settings")]
    public float machineGunFireRate = 0.1f; // Time between shots
    public int maxBullets = 30;
    public bool isInfinite = false;
    public int currentBullets;
    public bool isReloading = false;
    public float reloadTime = 2f;
    [Header("Missile Settings")]
    public float missileLaunchDelay = 3f; // Delay between missile launches
    public int maxMissiles = 3;
    public int currentMissiles;
    public float nextLaunchTime = 0f;
    // Current target position in world space
    private Vector3 currentTargetPosition;
    private Ray currentTargetRay;
    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        if (targetLockUI == null) {
            GameObject uiObj = GameObject.Find("Center"); // Use the exact name of your UI object
            if (uiObj != null)
                targetLockUI = uiObj.GetComponent<RectTransform>();
        }
        currentBullets = maxBullets;
        currentMissiles = maxMissiles;
        nextLaunchTime = 0f;
        isReloading = false;
    }
    private void Update()
    {
        UpdateTargetPosition();
        // Manual reload
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentBullets < maxBullets)
        {
            StartCoroutine(Reload());
        }
    }
    private void UpdateTargetPosition()
    {
        if (mainCamera == null) return;
        Vector3 viewportPoint;
        if (targetLockUI != null)
        {
        // Convert UI position to screen point
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, targetLockUI.position);
        // Convert screen point to viewport point (0-1 range)
            viewportPoint = mainCamera.ScreenToViewportPoint(screenPoint);
        }
        else
        {
            // Fallback to center of the screen
            viewportPoint = new Vector3(0.5f, 0.5f, 0f);
        }
        currentTargetRay = mainCamera.ViewportPointToRay(viewportPoint);
        // Update current target position
        RaycastHit hit;
        if (Physics.Raycast(currentTargetRay, out hit, machineGunFireRange, targetableLayers))
        {
            currentTargetPosition = hit.point;
        }
        else
        {
            currentTargetPosition = currentTargetRay.origin + currentTargetRay.direction * machineGunFireRange;
        }
    }
    public bool CanFireBullet() => isInfinite || (currentBullets > 0 && !isReloading);
    public void UseBullet() { if (!isInfinite && currentBullets > 0) currentBullets--; }
    public int GetCurrentBullets() => currentBullets;
    public bool CanFireMissile() => currentMissiles > 0;
    public void UseMissile() { if (currentMissiles > 0) currentMissiles--; }
    public int GetCurrentMissiles() => currentMissiles;
    // Public methods for other scripts to access targeting information
    public Vector3 GetCurrentTargetPosition()
    {
        return currentTargetPosition;
    }
    public Ray GetCurrentTargetRay()
    {
        return currentTargetRay;
    }
    public LayerMask GetTargetableLayers()
    {
        return targetableLayers;
    }
    [ContextMenu("Debug Layer Mask")]
    public void DebugLayerMask()
    {
        for (int i = 0; i < 32; i++)
        {
            if (((1 << i) & targetableLayers.value) != 0)
            {
                string layerName = LayerMask.LayerToName(i);
            }
        }
    }
    [ContextMenu("Debug Target Layers")]
    public void DebugTargetLayers()
    {
        // Find all turrets and enemies in scene
        GameObject[] turrets = GameObject.FindGameObjectsWithTag("Turret");
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        bool allOnDefaultLayer = true;
        foreach (GameObject turret in turrets)
        {
            string layerName = LayerMask.LayerToName(turret.layer);
            bool isTargetable = ((1 << turret.layer) & targetableLayers.value) != 0;
            if (turret.layer != 0) allOnDefaultLayer = false;
        }
        foreach (GameObject enemy in enemies)
        {
            string layerName = LayerMask.LayerToName(enemy.layer);
            bool isTargetable = ((1 << enemy.layer) & targetableLayers.value) != 0;
            if (enemy.layer != 0) allOnDefaultLayer = false;
        }
        if (allOnDefaultLayer)
        {
        }
        else
        {
        }
    }
    public bool IsTargetInRange(float range)
    {
        if (targetLockUI == null) 
        {
            return false;
        }
        RaycastHit hit;
        if (Physics.Raycast(currentTargetRay, out hit, range, targetableLayers))
        {
            bool isEnemy = hit.collider.CompareTag("Enemy");
            bool isTurret = hit.collider.CompareTag("Turret");
            bool inRange = isEnemy || isTurret;
            if (inRange && isTurret)
            {
                // Touch the weapon type to keep logic parity, but no debug usage
                var turret = hit.collider.GetComponentInParent<TurretControl>();
                var smallCanon = hit.collider.GetComponentInParent<SmallCanonControl>();
                var bigCanon = hit.collider.GetComponentInParent<BigCanon>();
            }
            return inRange;
        }
        return false;
    }
    public void SetTargetLockUI(RectTransform uiElement)
    {
        if (uiElement != null)
        {
            targetLockUI = uiElement;
        }
    }
    // Add this method to handle player level up
    public void LevelUp()
    {
        maxBullets += 11;
        maxMissiles += 1;
        currentBullets = maxBullets;
        currentMissiles = maxMissiles;
    }
    public IEnumerator Reload()
    {
        if (isReloading) yield break;
        isReloading = true;
        // Optionally: play reload SFX/UI here
        yield return new WaitForSeconds(reloadTime);
        currentBullets = maxBullets;
        isReloading = false;
    }
}
