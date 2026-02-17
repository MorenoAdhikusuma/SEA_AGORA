using UnityEngine;
using UnityEngine.SceneManagement;

public class Main_Menu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("Main_Level");
    }

    public void exitGame()
    {
        Application.Quit();
    }

    public void options()
    {
        SceneManager.LoadScene("Options");
    }
}
