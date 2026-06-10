// GameManager.cs — full updated version
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState { Playing, GameOver, Win }
    public GameState currentState = GameState.Playing;

    [Header("UI")]
    [SerializeField] private GameObject gameOverCanvas;
    [SerializeField] private GameObject winCanvas;

    [Header("Camera")]
    [SerializeField] private MouseLook mouseLook;
    [SerializeField] private CameraSwitcher cameraSwitcher;

    [Header("Keys")]
    [SerializeField] public int totalKeys = 7;
    private int collectedKeys = 0;

    [Header("Doors")]
    [SerializeField] private DoorController[] doors; // drag all 4 doors here

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        SetState(GameState.Playing);
    }

    public void CollectKey()
    {
        collectedKeys++;
        HUDManager.Instance.UpdateKeyCount(collectedKeys, totalKeys);

        if (collectedKeys >= totalKeys)
            RevealRandomDoor();
    }

    void RevealRandomDoor()
    {
        if (doors.Length == 0) return;
        int randomIndex = Random.Range(0, doors.Length);
        doors[randomIndex].Reveal();
        Debug.Log("Door revealed: " + doors[randomIndex].name);
    }

    public void TriggerGameOver()
    {
        if (currentState == GameState.GameOver) return;
        SetState(GameState.GameOver);
    }

    public void TriggerWin()
    {
        if (currentState == GameState.Win) return;
        SetState(GameState.Win);
    }

    void SetState(GameState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case GameState.Playing:
                gameOverCanvas.SetActive(false);
                winCanvas.SetActive(false);
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                if (mouseLook != null) mouseLook.enabled = true;
                if (cameraSwitcher != null) cameraSwitcher.enabled = true;
                break;

            case GameState.GameOver:
                gameOverCanvas.SetActive(true);
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (mouseLook != null) mouseLook.enabled = false;
                if (cameraSwitcher != null) cameraSwitcher.enabled = false;
                break;

            case GameState.Win:
                winCanvas.SetActive(true);
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (mouseLook != null) mouseLook.enabled = false;
                if (cameraSwitcher != null) cameraSwitcher.enabled = false;
                break;
        }
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}