using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void OnPlayClicked()
    {
        SceneManager.LoadScene("Level1");
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }
}
