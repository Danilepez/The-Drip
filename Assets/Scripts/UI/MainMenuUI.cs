using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void OnPlayClicked()
    {
        SceneManager.LoadScene("IntroCinematic");
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }
}
