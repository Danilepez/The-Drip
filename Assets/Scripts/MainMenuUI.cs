using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void OnPlayClicked()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }
}
