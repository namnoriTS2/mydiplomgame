using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] Transform cameraTransform;
    [SerializeField] float sensitivity = 0.05f;
    [SerializeField] float minVerticalAngle = -50f;
    [SerializeField] float maxVerticalAngle = 50f;

    float verticalRotation = 0f;
    Vector2 mouseInput;

  public void OnLook(InputAction.CallbackContext context)
{
    mouseInput = context.ReadValue<Vector2>();
}

    // Update is called once per frame
    void Update()
    {
        if (PauseMenu.gameisPause)
        {
            return;
        }
        float rotationy = mouseInput.x * sensitivity;
        player.Rotate(0f, rotationy, 0f);
        verticalRotation -= mouseInput.y * sensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

    }
}
