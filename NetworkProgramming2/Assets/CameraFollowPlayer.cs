using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    Transform target;
    public float smoothness;
    Vector3 offset;

    private void Awake()
    {
        offset = transform.position;
    }

    private void Update()
    {
        if (target != null)
        {
            var targetPosition = target.position;
            transform.position = Vector3.Lerp(transform.position, targetPosition + offset, smoothness * Time.deltaTime);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, offset, smoothness);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

    }
}
