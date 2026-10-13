using UnityEngine;
using System.Collections;
using TMPro;

public class Shotgun : MonoBehaviour
{
    public float fireRate = 0.8f;
    public int magSize = 6;
    public int reserveAmmo = 24;
    public int pelletsPerShot = 8;
    public float spreadAngle = 8f;
    public float range = 50f;
    public float pelletDamage = 10f;

    public float reloadTime = 2f;

    public GameObject bullet;
    public Transform bulletSpawnPoint;
    public GameObject weaponFlash;
    public Camera playerCamera;
    public TMP_Text ammoText;

    public AudioSource audioSource;
    public AudioClip shootSound;
    public AudioClip reloadSound;

    private int currentAmmo;
    private float nextTimeToFire;
    private bool isReloading;

    void Start()
    {
        currentAmmo = magSize;
        UpdateUIAmmo();
    }

    

public void Shoot()
{
    if (!gameObject.activeInHierarchy || isReloading)
        return;

    if (Time.time < nextTimeToFire)
        return;

    if (currentAmmo <= 0)
    {
        TryReload();
        return;
    }

    if (bullet == null || bulletSpawnPoint == null || playerCamera == null)
    {
        Debug.LogError("Не назначены Bullet, Bullet Spawn Point или Player Camera!");
        return;
    }

    nextTimeToFire = Time.time + fireRate;

    // Списываем только ОДИН патрон за весь выстрел
    currentAmmo--;
    UpdateUIAmmo();

    if (audioSource != null && shootSound != null)
        audioSource.PlayOneShot(shootSound);

    // Определяем точку прицеливания по центру экрана
    Ray ray = playerCamera.ViewportPointToRay(
        new Vector3(0.5f, 0.5f, 0f)
    );

    Vector3 targetPoint;

    if (Physics.Raycast(ray, out RaycastHit hit, range))
        targetPoint = hit.point;
    else
        targetPoint = ray.GetPoint(range);

    // Основное направление выстрела
    Vector3 aimDirection =
        (targetPoint - bulletSpawnPoint.position).normalized;

    // Создаём несколько отдельных пуль
    for (int i = 0; i < pelletsPerShot; i++)
    {
        Vector3 spreadDirection = Quaternion.Euler(
            Random.Range(-spreadAngle, spreadAngle),
            Random.Range(-spreadAngle, spreadAngle),
            0f
        ) * aimDirection;

        GameObject projectile = Instantiate(
            bullet,
            bulletSpawnPoint.position,
            Quaternion.LookRotation(spreadDirection)
        );

        Bullet bulletScript = projectile.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.speed = 30f;
            bulletScript.damage = pelletDamage;
        }
    }

    // Одна вспышка на весь выстрел
    if (weaponFlash != null)
    {
        Instantiate(
            weaponFlash,
            bulletSpawnPoint.position,
            bulletSpawnPoint.rotation
        );
    }

    Debug.Log("Выстрел! Создано дробинок: " + pelletsPerShot);
}



    public void TryReload()
    {
        if (isReloading || currentAmmo >= magSize || reserveAmmo <= 0)
            return;

        StartCoroutine(Reload());
    }

    private IEnumerator Reload()
    {
        isReloading = true;

        if (audioSource != null && reloadSound != null)
            audioSource.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(reloadTime);

        int neededAmmo = magSize - currentAmmo;
        int ammoToLoad = Mathf.Min(neededAmmo, reserveAmmo);

        currentAmmo += ammoToLoad;
        reserveAmmo -= ammoToLoad;

        isReloading = false;
        UpdateUIAmmo();
    }

    public void UpdateUIAmmo()
    {
        if (ammoText != null)
            ammoText.text = currentAmmo + "/" + reserveAmmo;
    }
}