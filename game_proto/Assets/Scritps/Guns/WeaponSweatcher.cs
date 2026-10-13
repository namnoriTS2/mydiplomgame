using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSwitcher : MonoBehaviour
{
    [Header("Оружие в руках")]
    [SerializeField] private GameObject[] weapons;

    private int currentWeapon = 0;

    private void Start()
    {
        SelectWeapon(currentWeapon);
    }

    public void OnSelectPistol(InputAction.CallbackContext context)
    {
        if (context.performed)
            SelectWeapon(0);
    }

    public void OnSelectShotgun(InputAction.CallbackContext context)
    {
        if (context.performed)
            SelectWeapon(1);
    }

    public void OnSelectAutomatic(InputAction.CallbackContext context)
    {
        if (context.performed)
            SelectWeapon(2);
    }

    private void SelectWeapon(int index)
    {
        if (weapons == null || index < 0 || index >= weapons.Length)
            return;

        if (weapons[index] == null)
            return;

        currentWeapon = index;

        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
                weapons[i].SetActive(i == currentWeapon);
        }
        UpdateUIAmmo();

        Debug.Log("Выбрано оружие: " + weapons[currentWeapon].name);
    }

    public GameObject GetCurrentWeapon()
    {
        if (weapons == null ||
            currentWeapon < 0 ||
            currentWeapon >= weapons.Length)
            return null;

        return weapons[currentWeapon];
    }

    private void UpdateUIAmmo()
{
    GameObject weapon = weapons[currentWeapon];

    Gun gun = weapon.GetComponent<Gun>();
    if (gun != null)
    {
        gun.UpdateUIAmmo();
        return;
    }

    GunPistol pistol = weapon.GetComponent<GunPistol>();
    if (pistol != null)
    {
        pistol.UpdateUIAmmo();
        return;
    }

    Shotgun shotgun = weapon.GetComponent<Shotgun>();
    if (shotgun != null)
    {
        shotgun.UpdateUIAmmo();
    }
}
}
