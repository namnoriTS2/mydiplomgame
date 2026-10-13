using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using NUnit.Framework;
using TMPro;

public class Gun : MonoBehaviour
{
    public float reloadtime = 1f;
    public float fireRate = 0.15f;
    public int magSize = 20;
    public float recoilDistance = 0.1f;
    public float recoilSpeed = 15f;

    public GameObject bullet;
    public Transform bulletSpawnPoint;
    public GameObject weaponFlash;
    public TMP_Text ammoText;
    public AudioSource audioSource;
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public int reserveAmmo = 100;

    private int currentAmmo;
    private bool isReloading = false;
    private float nextTimeToFire = 0f;
    public Camera playerCamera;
    private Quaternion initalRotation;
    private Vector3 initalPosition;
    private Vector3 reloadRotationOffset = new Vector3(60, 50,50);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentAmmo = magSize;
        initalRotation = transform.localRotation;
        initalPosition = transform.localPosition;
        UpdateUIAmmo();
    }

    public void Shoot()
    {
        if (isReloading)
        return;

    if (Time.time < nextTimeToFire)
        return;

    if (currentAmmo <= 0)
    {
        if(reserveAmmo > 0){
        StartCoroutine(Reload());
        }
        return;
    }

    if (bullet == null || bulletSpawnPoint == null || playerCamera == null)
    {
        Debug.LogError("Назначь Bullet, Bullet Spawn Point и Player Camera!");
        return;
    }

    nextTimeToFire = Time.time + fireRate;
    currentAmmo--;
    UpdateUIAmmo();
    if (audioSource != null && shootSound != null)
    {
    audioSource.PlayOneShot(shootSound);
    }

    // Луч из центра экрана через прицел
    Ray ray = playerCamera.ViewportPointToRay(
        new Vector3(0.5f, 0.5f, 0f)
    );

    Vector3 targetPoint;

    if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        targetPoint = hit.point;
    else
        targetPoint = ray.GetPoint(1000f);

    Vector3 direction =
        (targetPoint - bulletSpawnPoint.position).normalized;

    Quaternion rotation = Quaternion.LookRotation(direction);

    Instantiate(bullet, bulletSpawnPoint.position, rotation);
     Instantiate(weaponFlash, bulletSpawnPoint.position, rotation);
    StopCoroutine(nameof(Recoil));
    StartCoroutine(nameof(Recoil));
    }

    IEnumerator Reload()
    {
        isReloading = true;
        if (audioSource != null && reloadSound != null)
    {
    audioSource.PlayOneShot(reloadSound);
    }
        Quaternion targetRotation = Quaternion.Euler(initalRotation.eulerAngles + reloadRotationOffset);
        float halfReload = reloadtime /2;
        float t = 0f;

        while (t < halfReload)
        {
            t += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(initalRotation, targetRotation, t/halfReload);
            yield return null;
        }
        t = 0f;
        while (t < halfReload)
        {
            t += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(targetRotation, initalRotation, t/halfReload);
            yield return null;
        }

        int neededAmmo = magSize - currentAmmo;
        int ammoToLoad = Mathf.Min(neededAmmo, reserveAmmo);

        currentAmmo += ammoToLoad;
        reserveAmmo -= ammoToLoad;
        isReloading = false;
        UpdateUIAmmo();
    }

    public void TryReload()
    {
        if (isReloading || currentAmmo == magSize || reserveAmmo <= 0)
        {
            return;
        }
        StartCoroutine(Reload());
    }

    private IEnumerator Recoil()
    {
        Vector3 recoilTarget = initalPosition + new Vector3(0, 0, -recoilDistance);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(initalPosition, recoilTarget, t);
            yield return null;
        }

        t= 0f;

         while (t < 1f)
        {
            t += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(recoilTarget, initalPosition, t);
            yield return null;
        }
        transform.localPosition = initalPosition;
    }

    public void UpdateUIAmmo()
    {
        if(ammoText != null)
        {
            ammoText.text = currentAmmo + "/" + reserveAmmo;
        }
    }
}
