using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ContinueConfirmMenu : MonoBehaviour
{
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    [SerializeField] private string menuSceneName = "Menu";

    private void OnEnable()
    {
        if (yesButton != null)
            yesButton.onClick.AddListener(CloseMenu);

        if (noButton != null)
            noButton.onClick.AddListener(LoadMenuScene);
    }

    private void OnDisable()
    {
        if (yesButton != null)
            yesButton.onClick.RemoveListener(CloseMenu);

        if (noButton != null)
            noButton.onClick.RemoveListener(LoadMenuScene);
    }

    public void CloseMenu()
    {
        gameObject.SetActive(false);
    }

    public void LoadMenuScene()
    {
        SceneManager.LoadScene(menuSceneName);
    }
}
