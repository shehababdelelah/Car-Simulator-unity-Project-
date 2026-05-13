using UnityEngine;
using UnityEngine.SceneManagement;

public class DashboardMenuController : MonoBehaviour
{
    [SerializeField] private string drivingSceneName = "SUV";

    public void StartDriving()
    {
        SceneManager.LoadScene(drivingSceneName);
    }

    public void RestartMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
