using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuLoader : MonoBehaviour
{
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadFirstScene()
    {
        SceneManager.LoadScene("1");
    }

    public void LoadScondScene()
    {
        SceneManager.LoadScene("2");
    }
    public void LoadthirdScene()
    {
        SceneManager.LoadScene("3");
    }

    public void LoadFourthScene()
    {
        SceneManager.LoadScene("4");
    }
    public void LoadfifthScene()
    {
        SceneManager.LoadScene("5");
    }
    public void LoadsixthScene()
    {
        SceneManager.LoadScene("6");
    }
    public void LoadsebthScene()
    {
        SceneManager.LoadScene("7");
    }
    public void LoadeighthScene()
    {
        SceneManager.LoadScene("8");
    }

    public void LoadSUVScene()
    {
        SceneManager.LoadScene("SUV");
    }
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game"); // works in editor only as a message
    }
}