using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public float lastShotAngle = -1;
    public float lastShotSpeed = -1;
    public bool playerHasShot = false;

    public bool IsEnemyFiring { get; set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayerShot()
    {
        playerHasShot = true;
    }
}