using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform boat;              // Reference to the boat
    public Vector3 offset = new Vector3(0, 5, -5); // Offset position behind and above the boat
    public float rotationSpeed = 5f;    // Speed to smoothly rotate with the boat
    public float focusHeight = 5f;    // Speed to smoothly rotate with the boat

    void LateUpdate()
    {
        if (boat == null) return;

        // Set the camera's position relative to the boat
        var pos = boat.position + boat.TransformDirection(offset);
        pos.y = 12.75f + offset.y;
        transform.position = pos;

        var direction = boat.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Smoothly rotate the camera toward the target
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
