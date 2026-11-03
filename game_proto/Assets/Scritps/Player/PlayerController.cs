using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    CharacterController characterController;

    [SerializeField] float movespeed = 3f;
    [SerializeField] float runspeed = 20f;
    [SerializeField] float jumpheight = 3f;
    [SerializeField] Stamina stamina;

    [Header("Audio")]
    [SerializeField] private PlayerAudio playerAudio;

    float gravity = -9.81f;

    public bool isJumping = false;
    public bool isRunning = false;

    bool canrun = false;

    Vector2 moveInput;
    Vector3 velocity;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        characterController = GetComponent<CharacterController>();
        stamina = GetComponent<Stamina>();

        if (playerAudio == null)
            playerAudio = GetComponent<PlayerAudio>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && characterController.isGrounded)
        {
            if (stamina == null || stamina.Value >= 10f)
            {
                isJumping = true;
                playerAudio?.PlayJumpSound();
                EnemyController.MakeSound(transform.position, 0.8f);
                if (stamina != null)
                    stamina.SpendJump();
            }
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        if (InventoryManager.Instance != null &&
            InventoryManager.Instance.IsOpen)
        {
            return;
        }

        if (CraftingUI.IsCraftingOpen)
            return;

        isRunning = Input.GetKey(KeyCode.LeftShift)
                    && moveInput.y > 0 && canrun;

        if (stamina != null && stamina.Value <= 0f)
            canrun = false;

        if (stamina != null && stamina.Value >= 10f)
            canrun = true;

        Vector3 movement =
            (transform.right * moveInput.x +
             transform.forward * moveInput.y).normalized;

        if (characterController.isGrounded)
        {
            velocity.y = -2f;

            if (isJumping)
            {
                velocity.y = Mathf.Sqrt(jumpheight * -2f * gravity);
                isJumping = false;
            }
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        float currentspeed;

        if (isRunning && moveInput.y > 0)
            currentspeed = runspeed;
        else
            currentspeed = movespeed;

    bool hasMovement = moveInput.sqrMagnitude > 0.01f;

    if (hasMovement && characterController.isGrounded)
    {
        bool stepPlayed = playerAudio != null && playerAudio.PlayMovementSound(isRunning);
        if (isRunning && stepPlayed)
        {
            EnemyController.MakeSound(transform.position, 0.6f);
        }
    }
    else
    {
        playerAudio?.StopMovementSound();
    }
    characterController.Move(movement * currentspeed * Time.deltaTime);
    characterController.Move(velocity * Time.deltaTime);
    }
}