using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Follow Target")]
    [SerializeField] private Transform target;
    [SerializeField] private float offsetX = 2f;
    [SerializeField] private float smoothTime = 0.15f;

    [Header("Stopper")]
    [SerializeField] private Transform stopper; // assign your Stopper here (optional)

    private Vector3 velocity = Vector3.zero;
    private float fixedY;
    private float fixedZ;

    private void Awake()
    {
        fixedY = transform.position.y;
        fixedZ = transform.position.z;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // base desired X from player
        float desiredX = target.position.x + offsetX;

        // if we have a stopper, don't go past its X
        if (stopper != null)
        {
            desiredX = Mathf.Min(desiredX, stopper.position.x);
        }

        Vector3 desiredPos = new Vector3(
            desiredX,
            fixedY,
            fixedZ
        );

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref velocity,
            smoothTime
        );
    }
}
