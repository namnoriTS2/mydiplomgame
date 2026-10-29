
using UnityEngine;
using UnityEngine.InputSystem;

public class CraftingTable : MonoBehaviour
{
    public CraftingUI craftingUI;

    private bool playerNearby;

    private void Update()
    {
        if (playerNearby &&
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            craftingUI.Open();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            Debug.Log("Нажмите E, чтобы использовать стол");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }
}
