using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private WeaponSwitcher weaponSwitcher;

    private bool isHoldingShoot = false;

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed)
            isHoldingShoot = true;
        else if (context.canceled)
            isHoldingShoot = false;
    }

    public void OnReload1(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        GameObject weapon = GetCurrentWeapon();

        if (weapon == null)
            return;

        Gun gun = weapon.GetComponent<Gun>();
        if (gun != null)
        {
            gun.TryReload();
            return;
        }

        GunPistol pistol = weapon.GetComponent<GunPistol>();
        if (pistol != null)
        {
            pistol.TryReload();
            return;
        }

        Shotgun shotgun = weapon.GetComponent<Shotgun>();
        if (shotgun != null)
        {
            shotgun.TryReload();
        }
    }

    private void Update()
    {
        if (!isHoldingShoot)
            return;

        GameObject weapon = GetCurrentWeapon();

        if (weapon == null || !weapon.activeInHierarchy)
            return;

        Gun gun = weapon.GetComponent<Gun>();
        if (gun != null)
        {
            gun.Shoot();
            return;
        }

        GunPistol pistol = weapon.GetComponent<GunPistol>();
        if (pistol != null)
        {
            pistol.Shoot();
            return;
        }

        Shotgun shotgun = weapon.GetComponent<Shotgun>();
        if (shotgun != null)
        {
            shotgun.Shoot();
        }
    }

    private GameObject GetCurrentWeapon()
    {
        if (weaponSwitcher == null)
        {
            Debug.LogError("PlayerShooting: не назначен WeaponSwitcher!");
            return null;
        }

        return weaponSwitcher.GetCurrentWeapon();
    }
}
