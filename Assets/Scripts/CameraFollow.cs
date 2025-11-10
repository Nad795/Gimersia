using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float smoothSpeed = 5f;

    private float fixedX;
    private float minY;

    void Start()
    {
        fixedX = transform.position.x;
        minY = transform.position.y;
    }

    void LateUpdate()
    {
        if (player == null) return;

        float targetY = Mathf.Max(player.position.y, minY);
        Vector3 targetPosition = new Vector3(
            fixedX,
            targetY,
            transform.position.z
        );

        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}
