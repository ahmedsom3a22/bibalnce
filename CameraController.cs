using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 1.5f, -3f);
    public float rotationSpeed = 5f;
    public float verticalRotationLimit = 80f; // Limit the vertical rotation angle

    private float currentX = 0f;
    private float currentY = 0f; // New variable to store vertical rotation

    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("Camera target is not set!");
            return;
        }

        currentX += Input.GetAxis("Mouse X") * rotationSpeed;
        currentY -= Input.GetAxis("Mouse Y") * rotationSpeed; // Subtract to invert vertical rotation

        // Clamp the vertical rotation to the specified limit
        currentY = Mathf.Clamp(currentY, -verticalRotationLimit, verticalRotationLimit);

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0f); // Apply vertical rotation

        Vector3 desiredPosition = target.position + rotation * offset;

        transform.position = desiredPosition;

        transform.LookAt(target.position + Vector3.up);
    }

}
