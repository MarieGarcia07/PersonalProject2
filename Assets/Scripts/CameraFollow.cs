using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;          
    public Vector3 offset = new Vector3(0, 5, 10); 
    public float positionSmoothSpeed = 5f;
    public float rotationSmoothSpeed = 5f;

    // Fixed cinematic angle (side view with slight tilt)
    public Vector3 fixedEulerAngles = new Vector3(20, 30, 0);

    void LateUpdate()
    {
        if (target == null) return;

        // --- Smooth Position ---
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(
            transform.position, 
            desiredPosition, 
            positionSmoothSpeed * Time.deltaTime
        );

        // --- Smooth Rotation toward fixed angle ---
        Quaternion desiredRotation = Quaternion.Euler(fixedEulerAngles);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            desiredRotation, 
            rotationSmoothSpeed * Time.deltaTime
        );
    }
}

