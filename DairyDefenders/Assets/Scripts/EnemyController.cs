using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float speed = 5f;
    public LayerMask groundLayer;
    public GameObject projectilePrefab;
    public float projectileSize = 1f;
    public float enemyFireDelay = 2.0f;

    private float terrainSlopeAngle = 0f;
    private float moveHorizontal;

    private bool isFiringBack = false;

    private void Update()
    {
        AlignWithTerrain();

        if (GameManager.instance.playerHasShot && !isFiringBack)
        {
            StartCoroutine(FireBack());
            GameManager.instance.playerHasShot = false;
        }
    }

    private void AlignWithTerrain()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            Mathf.Infinity,
            groundLayer
        );

        if (hit.collider != null)
        {
            terrainSlopeAngle = Vector2.Angle(hit.normal, Vector2.up);

            float offset = Mathf.Abs(Mathf.Sin(terrainSlopeAngle * Mathf.Deg2Rad)) * GetComponent<Collider2D>().bounds.extents.x;

            transform.position = new Vector2(transform.position.x, hit.point.y + GetComponent<Collider2D>().bounds.extents.y + offset);

            float sign = (hit.normal.x >= 0) ? -1f : 1f;
            transform.eulerAngles = new Vector3(0, 0, terrainSlopeAngle * sign);
        }
    }

    IEnumerator FireBack()
    {
        GameManager.instance.IsEnemyFiring = true;

        yield return new WaitForSeconds(enemyFireDelay);

        FireProjectile(GameManager.instance.lastShotAngle, GameManager.instance.lastShotSpeed);

        GameManager.instance.lastShotAngle = -1;
        GameManager.instance.lastShotSpeed = -1;

        GameManager.instance.IsEnemyFiring = false;
    }

    private void FireProjectile(float angle, float speed)
    {
        // Convert the angle from a right-pointing angle to a left-pointing angle
        float reversedAngle = 180 - angle;

        Vector2 startPosition = transform.position + Quaternion.Euler(0, 0, reversedAngle) * Vector3.right * 0.5f;

        GameObject projectile = Instantiate(projectilePrefab, startPosition, Quaternion.identity);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

        projectile.transform.localScale = new Vector3(projectileSize, projectileSize, projectileSize);

        rb.AddForce(Quaternion.Euler(0, 0, reversedAngle) * Vector2.right * speed, ForceMode2D.Impulse);
    }
}