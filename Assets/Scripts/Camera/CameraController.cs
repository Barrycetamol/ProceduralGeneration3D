using UnityEngine;
//chatgpt generated camera controller
public class CameraController : MonoBehaviour
{
    public Transform boat; // The boat or object to orbit around
    public float distance = 10f; // Distance from the target
    public float rotationSpeed = 5f; // Speed of rotation
    public float minYAngle = -20f; // Minimum vertical angle
    public float maxYAngle = 130f; // Maximum vertical angle

    private float currentX = 0f; // Horizontal rotation angle
    private float currentY = 30f; // Vertical rotation angle

    void LateUpdate()
    {
        if (boat == null)
        {
            return;
        }

        // Check if the left mouse button is pressed
        if (Input.GetMouseButton(0))
        {
            // Adjust rotation based on mouse movement
            currentX += Input.GetAxis("Mouse X") * rotationSpeed;
            currentY -= Input.GetAxis("Mouse Y") * rotationSpeed;
        }

        // Calculate the new position and rotation of the camera
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 position = boat.position - (rotation * Vector3.forward * distance);

        // Apply the calculated position and rotation
        transform.position = position;
        transform.LookAt(boat);
    }
}