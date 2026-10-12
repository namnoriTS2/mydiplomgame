using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    public Gun gun;

    private bool isHoldingShoot = false;

    public void OnShoot(InputAction.CallbackContext context)
    {
        // isHoldingShoot = value.isPressed;
        // Debug.Log("Shoot pressed: " + isHoldingShoot);
        if (context.performed)
    {
        isHoldingShoot = true;
    }
    else if (context.canceled)
    {
        isHoldingShoot = false;
    }

    }  

    public void OnReload1(InputAction.CallbackContext context)
    {
       if (context.performed && gun != null)
    {
        gun.TryReload();
    }
    }

    void Update()
    {
        if(isHoldingShoot && gun != null)
        {
            gun.Shoot();
        }
    }
}