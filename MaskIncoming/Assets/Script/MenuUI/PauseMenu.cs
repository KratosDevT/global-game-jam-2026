using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    private bool isPaused;

    //public void OnPause(InputValue value)
    //{
    //    Debug.Log("P pressed");

    //    if (value.isPressed)
    //    {
    //        pauseMenu.SetActive(true);
    //    }
    //}

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }
}
