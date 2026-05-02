using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseScreenUI : MonoBehaviour
{
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnRestartClicked()
    {
        SceneManager.LoadScene("Level1");
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }
}
