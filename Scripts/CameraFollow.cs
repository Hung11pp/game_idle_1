using UnityEngine;

/// <summary>
/// Keeps the camera following a target from a slight top-down angle.
/// This is meant for a simple prototype scene.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Follow Settings")]
    public Vector3 offset = new Vector3(0f, 12f, -8f);
    public float followSpeed = 5f;
    public bool lookAtTarget = true;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        if (lookAtTarget)
        {
            Vector3 lookPoint = target.position;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lookPoint - transform.position), followSpeed * Time.deltaTime);
        }
    }
}