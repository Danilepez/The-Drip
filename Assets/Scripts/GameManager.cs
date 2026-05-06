using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null) CreateInstance();
            return _instance;
        }
        private set => _instance = value;
    }

    public string mainMenuScene = "MainMenu";
    public string gameScene = "Level1";
    public string winScene = "WinScreen";
    public string loseScene = "LoseScreen";

    private static void CreateInstance()
    {
        if (_instance != null) return;
        var go = new GameObject("GameManager");
        _instance = go.AddComponent<GameManager>();
        DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartGame()
    {
        BloodInteractable.ResetState();
        SceneManager.LoadScene(gameScene);
    }

    public void WinGame()
    {
        SceneManager.LoadScene(winScene);
    }

    public void LoseGame()
    {
        SceneManager.LoadScene(loseScene);
    }

    public void RestartGame()
    {
        BloodInteractable.ResetState();
        SceneManager.LoadScene(gameScene);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuScene);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
