using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public LayerMask groundLayer;
    public Animator tankAnimation;
    public float shootingAngle = 45f;
    public float lineLength = 5f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public float projectileSize = 1f;
    public float minShootingAngle = 0f;
    public float maxShootingAngle = 90f;
    public float adjustAngleSpeed = 0.25f;

    private float terrainSlopeAngle = 0f;
    private float moveHorizontal;
    private LineRenderer lineRenderer;

    private void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 2;
        lineRenderer.material.color = Color.red;
    }

    private void Update()
    {
        moveHorizontal = Input.GetAxis("Horizontal");

        bool isMoving = moveHorizontal != 0;
        tankAnimation.SetBool("isMoving", isMoving);

        Vector2 movement = new Vector2(moveHorizontal, 0) * speed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        AlignWithTerrain();

        UpdateShootingAngle();

        UpdateLineRenderer();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            FireProjectile();
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

    private void UpdateShootingAngle()
    {
        if (Input.GetKey(KeyCode.F))
        {
            shootingAngle += adjustAngleSpeed;
        }

        if (Input.GetKey(KeyCode.J))
        {
            shootingAngle -= adjustAngleSpeed;
        }

        shootingAngle = Mathf.Clamp(shootingAngle, minShootingAngle, maxShootingAngle);
    }

    private void UpdateLineRenderer()
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + Quaternion.Euler(0, 0, shootingAngle) * Vector3.right * lineLength;

        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, endPosition);
    }

    private void FireProjectile()
    {
        // If the enemy is firing, don't let the player fire
        if (GameManager.instance.IsEnemyFiring)
        {
            return;
        }

        Vector2 startPosition = transform.position + Quaternion.Euler(0, 0, shootingAngle) * Vector3.right * 0.5f;

        GameObject projectile = Instantiate(projectilePrefab, startPosition, Quaternion.identity);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

        projectile.transform.localScale = new Vector3(projectileSize, projectileSize, projectileSize);

        rb.AddForce(Quaternion.Euler(0, 0, shootingAngle) * Vector2.right * projectileSpeed, ForceMode2D.Impulse);

        GameManager.instance.lastShotAngle = shootingAngle;
        GameManager.instance.lastShotSpeed = projectileSpeed;

        GameManager.instance.PlayerShot();
    }
}