using UnityEngine;

public class HealthBarFollower : MonoBehaviour
{
    public Transform objectToFollow;
    public Vector3 offset;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (objectToFollow)
        {
            transform.position = objectToFollow.position + offset;
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
        }
    }
}