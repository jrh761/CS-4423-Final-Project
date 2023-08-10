using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class EnemyController : MonoBehaviour
{
    public float speed = 5f;
    public LayerMask groundLayer;
    public GameObject projectilePrefab;
    public float projectileSize = 1f;
    public float enemyFireDelay = 2.0f;

    private float terrainSlopeAngle = 0f;
    private float moveHorizontal;

    public GameObject shootParticleEffectPrefab;

    public float lineLength = 0.5f;
    private LineRenderer lineRenderer;
    private AudioSource shootingAudioSource;
    public AudioClip tankShootClip;
    public Material lineMaterial;
    public AudioMixer gameAudioMixer;
    private ParticleSystem shootingParticleSystem;

    private bool isFiringBack = false;

    private void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.sortingLayerName = "Player";
        lineRenderer.sortingOrder = 1;
        lineRenderer.material = lineMaterial;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 2;

        shootingParticleSystem = Instantiate(shootParticleEffectPrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        Vector3 endPosition = lineRenderer.GetPosition(1);
        shootingParticleSystem.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        shootingParticleSystem.transform.position = endPosition;

        shootingAudioSource = gameObject.AddComponent<AudioSource>();

        if (shootingAudioSource)
        {
            shootingAudioSource.outputAudioMixerGroup = gameAudioMixer.FindMatchingGroups("SFX")[0];
        }
    }

    private void Update()
    {
        AlignWithTerrain();
        UpdateLineRenderer();

        if (GameManager.instance.playerHasShot && !isFiringBack)
        {
            GameManager.instance.playerHasShot = false;
            StartCoroutine(FireBack());
        }
    }
    private void UpdateLineRenderer()
    {
        float currentAngle = GameManager.instance.lastShotAngle;

        if (currentAngle == -1)
        {
            currentAngle = 45f;
        }

        float reversedAngle = 180 - currentAngle;

        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + Quaternion.Euler(0, 0, reversedAngle) * Vector3.right * lineLength;

        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, endPosition);

        shootingParticleSystem.transform.position = endPosition;
        shootingParticleSystem.transform.rotation = Quaternion.Euler(0, 0, reversedAngle);
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
        isFiringBack = true;
        GameManager.instance.IsEnemyFiring = true;

        yield return new WaitForSeconds(enemyFireDelay);

        while (GameObject.FindGameObjectWithTag("Projectile") != null)
        {
            yield return null;
        }

        FireProjectile(GameManager.instance.lastShotAngle, GameManager.instance.lastShotSpeed);
        shootingAudioSource.PlayOneShot(tankShootClip);

        GameManager.instance.lastShotAngle = -1;
        GameManager.instance.lastShotSpeed = -1;
        GameManager.instance.IsEnemyFiring = false;

        isFiringBack = false;
    }

    private void FireProjectile(float angle, float speed)
    {
        // Convert the angle from a right-pointing angle to a left-pointing angle
        float reversedAngle = 180 - angle;

        if (Random.value > 0.5f)
        {
            speed += Random.Range(0f, 4f);
        }

        Vector2 startPosition = transform.position + Quaternion.Euler(0, 0, reversedAngle) * Vector3.right * 0.5f;

        GameObject projectile = Instantiate(projectilePrefab, startPosition, Quaternion.identity);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

        projectile.transform.localScale = new Vector3(projectileSize, projectileSize, projectileSize);

        rb.AddForce(Quaternion.Euler(0, 0, reversedAngle) * Vector2.right * speed, ForceMode2D.Impulse);

        shootingParticleSystem.Play();
    }
}