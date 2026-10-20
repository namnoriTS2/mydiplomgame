
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSwitcher : MonoBehaviour
{
    public static WeaponSwitcher Instance { get; private set; }

    [Header("Оружие в руках")]
    [SerializeField] private GameObject[] weapons;

    [Header("Доступность оружия")]
    [SerializeField] private bool[] unlockedWeapons = { false, false, false };

    private int currentWeapon = 0;

   private void Awake()
{
    if (Instance != null && Instance != this)
    {
        Debug.LogError("В сцене найден второй WeaponSwitcher!");
        Destroy(gameObject);
        return;
    }

    Instance = this;

    Debug.Log("WeaponSwitcher.Instance успешно назначен!");

    if (weapons == null)
    {
        Debug.LogError("Массив weapons не назначен!");
        return;
    }

    unlockedWeapons = new bool[weapons.Length];
}

    private void Start()
    {  
        currentWeapon = -1;

        foreach (GameObject weapon in weapons)
        {   
            if (weapon != null){
                weapon.SetActive(false);
            }
        }
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

        if (unlockedWeapons == null ||
            index >= unlockedWeapons.Length ||
            !unlockedWeapons[index])
        {
            Debug.Log("Это оружие ещё не найдено!");
            return;
        }

        currentWeapon = index;

        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
                weapons[i].SetActive(i == currentWeapon);
        }

        UpdateUIAmmo();

        Debug.Log("Выбрано оружие: " + weapons[currentWeapon].name);
    }

    public bool UnlockWeapon(InventoryWeaponKind kind)
    {
        int index = GetWeaponIndex(kind);

        if (index < 0 || weapons == null ||
            index >= weapons.Length || weapons[index] == null)
        {
            Debug.LogWarning("Не удалось найти объект оружия: " + kind);
            return false;
        }

        unlockedWeapons[index] = true;
        Debug.Log("Оружие разблокировано: " + kind);
        return true;
    }

    public bool EquipWeapon(InventoryWeaponKind kind)
    {
        int index = GetWeaponIndex(kind);

        if (index < 0 || !unlockedWeapons[index])
            return false;

        SelectWeapon(index);
        return currentWeapon == index;
    }

    private int GetWeaponIndex(InventoryWeaponKind kind)
    {
        switch (kind)
        {
            case InventoryWeaponKind.Pistol:
                return 0;

            case InventoryWeaponKind.Shotgun:
                return 1;

            case InventoryWeaponKind.Automatic:
                return 2;

            default:
                return -1;
        }
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
        GameObject weapon = GetCurrentWeapon();

        if (weapon == null)
            return;

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
            shotgun.UpdateUIAmmo();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
