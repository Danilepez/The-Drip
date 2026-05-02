using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialUI : MonoBehaviour
{
    public void OnContinueClicked()
    {
        SceneManager.LoadScene("Level1");
    }
}
