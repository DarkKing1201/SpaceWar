using UnityEngine;
public class PlayerBullet : MonoBehaviour
{
    // Static variables to keep last log states
    private static string lastHitObjectName = null;
    private static bool lastInFireRange = false;
    private static float lastDamageDealt = -1f;
    void OnCollisionEnter(Collision collision)
    {
        string hitName = collision.gameObject.name;
        if (hitName != lastHitObjectName)
        {
            lastHitObjectName = hitName;
        }
        // Prioritize Turret first
        if (collision.gameObject.CompareTag("Turret"))
        {
            // Check for all weapon types since all weapons use "Turret" tag
            var turret = collision.gameObject.GetComponentInParent<TurretControl>();
            var smallCanon = collision.gameObject.GetComponentInParent<SmallCanonControl>();
            var bigCanon = collision.gameObject.GetComponentInParent<BigCanon>();
            if (turret != null)
            {
                turret.TakeDamage(1); // Or your bullet damage value
            }
            else if (smallCanon != null)
            {
                smallCanon.TakeDamage(1); // Or your bullet damage value
            }
            else if (bigCanon != null)
            {
                bigCanon.TakeDamage(1); // Or your bullet damage value
            }
            else
            {
            }
        }
        // Then Enemy
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            var enemyStats = collision.gameObject.GetComponentInParent<EnemyStats>();
            var mainBossStats = collision.gameObject.GetComponentInParent<MainBossStats>();
            var player = GameManager.Instance.currentPlayer;
            var weaponManager = player.GetComponent<PlayerWeaponManager>();
            var gunControl = player.GetComponent<MachineGunControl>();
            var playerStats = player.GetComponent<PlaneStats>();
            if (enemyStats != null && weaponManager != null && gunControl != null && playerStats != null)
            {
                // Check fire range
                float distance = Vector3.Distance(player.transform.position, collision.transform.position);
                bool inRange = distance <= weaponManager.machineGunFireRange;
                if (inRange != lastInFireRange)
                {
                    lastInFireRange = inRange;
                }
                float finalDamage = gunControl.damage + playerStats.attackPoint;
                if (finalDamage != lastDamageDealt)
                {
                    lastDamageDealt = finalDamage;
                }
                enemyStats.TakeDamage((int)finalDamage);
            }
            else if (mainBossStats != null && weaponManager != null && gunControl != null && playerStats != null)
            {
                // Check fire range
                float distance = Vector3.Distance(player.transform.position, collision.transform.position);
                bool inRange = distance <= weaponManager.machineGunFireRange;
                if (inRange != lastInFireRange)
                {
                    lastInFireRange = inRange;
                }
                float finalDamage = gunControl.damage + playerStats.attackPoint;
                if (finalDamage != lastDamageDealt)
                {
                    lastDamageDealt = finalDamage;
                }
                mainBossStats.TakeDamage((int)finalDamage);
            }
        }
        Destroy(gameObject);
    }
} 
