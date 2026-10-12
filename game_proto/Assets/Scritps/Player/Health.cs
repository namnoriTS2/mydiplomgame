using System;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   [SerializeField] private Slider health;
   [SerializeField] private PlayerController player;
   [SerializeField] private GameObject DeadScreen;
   [SerializeField] private float damage;
    private void Start()
    {
        player = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if(health.value <= 0)
        {
            DeadScreen.SetActive(true);
            Time.timeScale = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            TakeDmage();
        }       
    }

    private void TakeDmage()
    {
        health.value -= damage;
        Debug.Log($"Health: {health.value}");
    }
}
