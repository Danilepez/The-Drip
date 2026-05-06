using UnityEngine;
public class WinScreenUI : MonoBehaviour
{
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnRestartClicked()
    {
        GameManager.Instance?.RestartGame();
    }

    public void OnQuitClicked()
    {
        GameManager.Instance?.GoToMainMenu();
    }
}
