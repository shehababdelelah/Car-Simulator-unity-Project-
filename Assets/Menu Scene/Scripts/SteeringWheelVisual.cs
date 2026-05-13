using UnityEngine;

[DisallowMultipleComponent]
public class SteeringWheelVisual : MonoBehaviour
{
    [Header("Input")]
    public string steerAxis = "WheelSteer";
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public bool invertWheelAxis = true;
    public bool invertKeyboard;
    [Tooltip("Flips the final visual steering direction after keyboard or wheel input is selected.")]
    public bool invertVisualRotation;

    [Header("Wheel Rotation")]
    public Vector3 localRotationAxis = Vector3.forward;
    public float maxRotationDegrees = 450f;
    public float keyboardSteerSpeed = 2.5f;
    public float returnSpeed = 4f;
    public float axisDeadZone = 0.05f;

    Quaternion startLocalRotation;
    float currentSteer;
    bool axisMissing;

    void Awake()
    {
        startLocalRotation = transform.localRotation;
    }

    void Update()
    {
        float targetSteer = ReadSteerInput();
        if (invertVisualRotation)
            targetSteer = -targetSteer;

        float speed = Mathf.Abs(targetSteer) > 0.001f ? keyboardSteerSpeed : returnSpeed;

        currentSteer = Mathf.MoveTowards(currentSteer, targetSteer, speed * Time.deltaTime);
        transform.localRotation = startLocalRotation * Quaternion.AngleAxis(currentSteer * maxRotationDegrees, localRotationAxis.normalized);
    }

    float ReadSteerInput()
    {
        float keyboard = 0f;
        if (Input.GetKey(leftKey)) keyboard -= 1f;
        if (Input.GetKey(rightKey)) keyboard += 1f;
        if (invertKeyboard) keyboard = -keyboard;

        float wheel = ReadAxis();
        return Mathf.Abs(wheel) > axisDeadZone ? wheel : keyboard;
    }

    float ReadAxis()
    {
        if (axisMissing || string.IsNullOrWhiteSpace(steerAxis))
            return 0f;

        try
        {
            float value = Input.GetAxis(steerAxis);
            if (invertWheelAxis) value = -value;
            return Mathf.Abs(value) < axisDeadZone ? 0f : Mathf.Clamp(value, -1f, 1f);
        }
        catch (System.ArgumentException)
        {
            axisMissing = true;
            Debug.LogWarning($"{nameof(SteeringWheelVisual)}: Input axis '{steerAxis}' does not exist.");
            return 0f;
        }
    }
}
