
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance;
    public static bool gameisPause = false;

    public GameObject pauseMenuUI;
    public GameObject optionsMenuUI;

    [Header("Игровой интерфейс")]
    [SerializeField] private GameObject healthUI;
    [SerializeField] private GameObject staminaUI;
    [SerializeField] private GameObject ammoUI;
    [SerializeField] private GameObject inventoryUI;
    private bool inventoryWasOpen;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Time.timeScale = 1f;
        gameisPause = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsMenuUI.activeSelf)
            {
                CloseOptions();
            }
            else if (gameisPause)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        optionsMenuUI.SetActive(false);
        inventoryWasOpen = inventoryUI != null && inventoryUI.activeSelf;
        // Скрываем игровой интерфейс
        SetGameUI(false);

        Time.timeScale = 0f;
        gameisPause = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);

        // Возвращаем игровой интерфейс
        SetGameUI(true);

        Time.timeScale = 1f;
        gameisPause = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OpenOptions()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);

        // Игровой интерфейс остаётся скрытым
        SetGameUI(false);

        Time.timeScale = 0f;
        gameisPause = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void CloseOptions()
    {
        optionsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);

        // Игровой интерфейс остаётся скрытым
        SetGameUI(false);

        Time.timeScale = 0f;
        gameisPause = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void SetGameUI(bool state)
    {
        if (healthUI != null)
            healthUI.SetActive(state);
        if (staminaUI != null)
            staminaUI.SetActive(state);

        if (ammoUI != null)
            ammoUI.SetActive(state);
        if (inventoryUI != null)
            inventoryUI.SetActive(state && inventoryWasOpen);
    }

    public void LoadScene()
    {
        Time.timeScale = 1f;
        gameisPause = false;

        SceneManager.LoadScene("Menu");
    }
}
