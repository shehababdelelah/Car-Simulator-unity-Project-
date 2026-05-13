using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VehiclePhysics;

public class EscapeMenuActions : MonoBehaviour
{
    [SerializeField] private GameObject vehicle;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button resetCarButton;
    [SerializeField] private Button resetLevelButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private string menuSceneName = "Menu";
    [SerializeField] private bool startHidden = true;
    [SerializeField] private bool pauseWhileOpen = true;

    private Canvas menuCanvas;
    private GraphicRaycaster graphicRaycaster;
    private CanvasGroup canvasGroup;
    private bool isVisible;

    private void Awake()
    {
        menuCanvas = GetComponent<Canvas>();
        graphicRaycaster = GetComponent<GraphicRaycaster>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(CloseMenu);

        if (resetCarButton != null)
            resetCarButton.onClick.AddListener(ResetCar);

        if (resetLevelButton != null)
            resetLevelButton.onClick.AddListener(ResetLevel);

        if (leaveButton != null)
            leaveButton.onClick.AddListener(LeaveToMenu);
    }

    private void Start()
    {
        SetMenuVisible(!startHidden);
    }

    private void OnDisable()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(CloseMenu);

        if (resetCarButton != null)
            resetCarButton.onClick.RemoveListener(ResetCar);

        if (resetLevelButton != null)
            resetLevelButton.onClick.RemoveListener(ResetLevel);

        if (leaveButton != null)
            leaveButton.onClick.RemoveListener(LeaveToMenu);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ToggleMenu();
    }

    public void ToggleMenu()
    {
        SetMenuVisible(!isVisible);
    }

    public void CloseMenu()
    {
        SetMenuVisible(false);
    }

    public void ResetCar()
    {
        VPResetVehicle resetVehicle = null;
        if (vehicle != null)
            resetVehicle = vehicle.GetComponent<VPResetVehicle>();

        if (resetVehicle == null)
            resetVehicle = FindFirstObjectByType<VPResetVehicle>();

        if (resetVehicle != null)
        {
            resetVehicle.DoReset();
            CloseMenu();
        }
    }

    public void ResetLevel()
    {
        Time.timeScale = 1f;
        DestroyPersistentRuntimeVehicles();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LeaveToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    private void SetMenuVisible(bool visible)
    {
        isVisible = visible;

        if (menuCanvas != null)
            menuCanvas.enabled = visible;

        if (graphicRaycaster != null)
            graphicRaycaster.enabled = visible;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        if (pauseWhileOpen)
            Time.timeScale = visible ? 0f : 1f;
    }

    private void DestroyPersistentRuntimeVehicles()
    {
        foreach (VehicleBase persistentVehicle in FindObjectsByType<VehicleBase>(FindObjectsSortMode.None))
        {
            if (persistentVehicle.gameObject.scene.name == "DontDestroyOnLoad")
                Destroy(persistentVehicle.gameObject);
        }
    }
}
