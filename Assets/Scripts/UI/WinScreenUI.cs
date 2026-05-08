using UnityEngine;

public class WinScreenUI : MonoBehaviour
{
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnMainMenuClicked()
    {
        GameManager.Instance?.GoToMainMenu();
    }
}