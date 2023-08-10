using UnityEngine;
using UnityEngine.Audio;

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
    public float adjustShootSpeed = 0.1f;
    public float maxShootSpeed = 20f;
    public float minShootSpeed = 1f;
    public float gasUsedScaler = 10f;
    public float lineWidth = 0.1f;
    public AudioClip tankMovingClip;
    public AudioClip angleAdjustClip;
    public AudioClip tankShootClip;
    public AudioMixer gameAudioMixer;
    public Material lineMaterial;
    public GameObject shootParticleEffectPrefab;

    private float terrainSlopeAngle = 0f;
    private float moveHorizontal;
    private LineRenderer lineRenderer;

    private AudioSource movementAudioSource;
    private AudioSource adjustmentAudioSource;
    private AudioSource shootingAudioSource;

    private bool isManualAngleChange = false;
    private bool isManualPowerChange = false;
    private bool isButtonControlled = false;
    private ParticleSystem shootingParticleSystem;

    private TerrainGenerator terrainGenerator;

    private void Start()
    {
        movementAudioSource = gameObject.AddComponent<AudioSource>();
        adjustmentAudioSource = gameObject.AddComponent<AudioSource>();
        shootingAudioSource = gameObject.AddComponent<AudioSource>();

        if (adjustmentAudioSource)
        {
            adjustmentAudioSource.outputAudioMixerGroup = gameAudioMixer.FindMatchingGroups("SFX")[0];
        }
        if (shootingAudioSource)
        {
            shootingAudioSource.outputAudioMixerGroup = gameAudioMixer.FindMatchingGroups("SFX")[0];
        }
        if (movementAudioSource)
        {
            movementAudioSource.outputAudioMixerGroup = gameAudioMixer.FindMatchingGroups("SFX")[0];
        }

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 2;
        lineRenderer.sortingLayerName = "Player";
        lineRenderer.sortingOrder = 1;
        lineRenderer.material = lineMaterial;
        shootingAngle = 45f;
        projectileSpeed = 10f;

        shootingParticleSystem = Instantiate(shootParticleEffectPrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        Vector3 endPosition = lineRenderer.GetPosition(1);
        shootingParticleSystem.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        shootingParticleSystem.transform.position = endPosition;

        terrainGenerator = TerrainGenerator.Instance;
    }

    private void Update()
    {
        UpdateMovement();

        bool isMoving = moveHorizontal != 0;
        tankAnimation.SetBool("isMoving", isMoving);

        Vector2 movement = new Vector2(moveHorizontal, 0) * speed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        if (isMoving && isButtonControlled)
        {
            terrainGenerator.ReduceGas(gasUsedScaler * Time.deltaTime);
        }

        if (isMoving && !movementAudioSource.isPlaying)
        {
            movementAudioSource.clip = tankMovingClip;
            movementAudioSource.Play();
        }
        else if (!isMoving)
        {
            movementAudioSource.Stop();
        }

        AlignWithTerrain();

        if (!isManualAngleChange)
        {
            UpdateShootingAngle();
        }
        else
        {
            isManualAngleChange = false;
        }

        if (!isManualPowerChange)
        {
            UpdateProjectileSpeed();
        }
        else
        {
            isManualPowerChange = false;
        }

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
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            if (!adjustmentAudioSource.isPlaying)
            {
                adjustmentAudioSource.clip = angleAdjustClip;
                adjustmentAudioSource.Play();
            }
        }
        else
        {
            if (adjustmentAudioSource.isPlaying)
            {
                adjustmentAudioSource.Stop();
            }
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            shootingAngle += adjustAngleSpeed;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            shootingAngle -= adjustAngleSpeed;
        }

        shootingAngle = Mathf.Clamp(shootingAngle, minShootingAngle, maxShootingAngle);

        if (TerrainGenerator.Instance != null && TerrainGenerator.Instance.angleSlider != null)
        {
            TerrainGenerator.Instance.angleSlider.value = shootingAngle;
        }
    }

    private void UpdateProjectileSpeed()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            projectileSpeed += adjustShootSpeed;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            projectileSpeed -= adjustShootSpeed;
        }

        projectileSpeed = Mathf.Clamp(projectileSpeed, minShootSpeed, maxShootSpeed);

        if (TerrainGenerator.Instance != null && TerrainGenerator.Instance.powerSlider != null)
        {
            TerrainGenerator.Instance.powerSlider.value = projectileSpeed;
        }
    }

    private void UpdateMovement()
    {
        if (terrainGenerator.GetGasLeft() <= 0)
        {
            moveHorizontal = 0;
            return;
        }

        if (!isButtonControlled)
        {
            if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D))
            {
                moveHorizontal = 0;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                moveHorizontal = -1;
                terrainGenerator.ReduceGas(gasUsedScaler * Time.deltaTime);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                moveHorizontal = 1;
                terrainGenerator.ReduceGas(gasUsedScaler * Time.deltaTime);
            }
            else
            {
                moveHorizontal = 0;
            }
        }
    }

    private void UpdateLineRenderer()
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + Quaternion.Euler(0, 0, shootingAngle) * Vector3.right * lineLength;

        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, endPosition);

        shootingParticleSystem.transform.position = endPosition;
        shootingParticleSystem.transform.rotation = Quaternion.Euler(0, 0, shootingAngle);
    }

    public void FireProjectile()
    {
        if (GameManager.instance.IsEnemyFiring || GameObject.FindGameObjectWithTag("Projectile") != null)
            return;

        Vector2 startPosition = transform.position + Quaternion.Euler(0, 0, shootingAngle) * Vector3.right * 0.5f;

        GameObject projectile = Instantiate(projectilePrefab, startPosition, Quaternion.identity);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

        projectile.transform.localScale = new Vector3(projectileSize, projectileSize, projectileSize);

        rb.AddForce(Quaternion.Euler(0, 0, shootingAngle) * Vector2.right * projectileSpeed, ForceMode2D.Impulse);

        shootingAudioSource.PlayOneShot(tankShootClip);
        shootingParticleSystem.Play();

        GameManager.instance.lastShotAngle = shootingAngle;
        GameManager.instance.lastShotSpeed = projectileSpeed;
        GameManager.instance.PlayerShot();
    }

    public void StartMoveForward()
    {
        isButtonControlled = true;
        moveHorizontal = 1;
    }

    public void StopMove()
    {
        isButtonControlled = false;
        if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
        {
            moveHorizontal = 0;
        }
    }

    public void StartMoveBackward()
    {
        isButtonControlled = true;
        moveHorizontal = -1;
    }

    public void SetAngle(float value)
    {
        shootingAngle = value;
        isManualAngleChange = true;
    }

    public void SetPower(float value)
    {
        projectileSpeed = value;
        isManualPowerChange = true;
    }
}