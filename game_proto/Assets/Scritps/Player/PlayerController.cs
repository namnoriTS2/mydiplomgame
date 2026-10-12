using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]

public class PlayerController : MonoBehaviour{
    CharacterController characterController;
    [SerializeField] float movespeed = 3f;
    [SerializeField] float runspeed = 20f;
    [SerializeField] float jumpheight = 3f;
    [SerializeField] Stamina stamina;
    float gravity = -9.81f;
    public bool isJumping = false; 
    public bool isRunning = false;
    bool canrun = false;
    Vector2 moveInput;
    Vector3 velocity;


//ищется на объекте контроллер и помогает использовать методы
    void Start (){
    Cursor.visible = false;
    Cursor.lockState = CursorLockMode.Locked;
    characterController = GetComponent<CharacterController>();
    stamina = GetComponent<Stamina>();
   }

   public void OnJump(InputAction.CallbackContext context){
    if(context.performed && characterController.isGrounded){
        if (stamina == null || stamina.Value >= 10f){
        isJumping = true;
        if (stamina != null) stamina.SpendJump();   
        }
    }
    }
   public void OnMove(InputAction.CallbackContext context){
    moveInput = context.ReadValue<Vector2>();
   }

//    public void OnAttack(InputAction.CallbackContext context)
//     {
//         if (context.started)
//         shootingScript?.StartFiring();

//     if (context.canceled)
//         shootingScript?.StopFiring();
//     }

//     public void OnReload(InputAction.CallbackContext context)
//     {
//     if (context.performed)
//         shootingScript?.Reload();
//     }

   private void Update(){
    isRunning = Input.GetKey(KeyCode.LeftShift)
    && moveInput.y >0 && canrun;
    if (stamina !=null && stamina.Value <= 0f){
        canrun = false;
    } 
    if (stamina !=null && stamina.Value >= 10f){
        canrun = true;
    }
    Vector3 movement = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;
    if(characterController.isGrounded){
        velocity.y = -2f;

        if (isJumping){
            velocity.y = Mathf.Sqrt(jumpheight * -2f * gravity);
            isJumping = false;
        }
    }
    else{
        velocity.y += gravity * Time.deltaTime;
    }
    float currentspeed;
    if (isRunning && moveInput.y>0){
        currentspeed = runspeed; 
    }else{
        currentspeed = movespeed;
    }
    characterController.Move(movement * currentspeed * Time.deltaTime);
    characterController.Move(velocity * Time.deltaTime);
   }
}
