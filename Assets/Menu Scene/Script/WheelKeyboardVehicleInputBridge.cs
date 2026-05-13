using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using VehiclePhysics;

[DefaultExecutionOrder(10000)]
public class WheelKeyboardVehicleInputBridge : MonoBehaviour
{
    [Header("Wheel Device")]
    [SerializeField] private string preferredDeviceName = "HORI";
    [SerializeField] private bool logDeviceSelection = true;

    [Header("Vehicle")]
    [SerializeField] private bool autoFindVehicle = true;
    [SerializeField] private VehicleBase vehicle;
    [SerializeField] private bool autoFindStandardInput = true;
    [SerializeField] private VPStandardInput standardInput;

    [Header("Wheel Axis Mapping")]
    [SerializeField] private string steeringAxis = "x";
    [SerializeField] private string throttleAxis = "z";
    [SerializeField] private string brakeAxis = "z";
    [SerializeField] private bool combinePedalsOnSingleAxis = true;
    [SerializeField] private bool combinedAxisSigned = true;
    [SerializeField] private bool autoCalibrateCombinedAxis = true;
    [SerializeField] private bool invertCombinedAxis;
    [SerializeField] private bool invertSteering;
    [SerializeField] private bool invertThrottle;
    [SerializeField] private bool invertBrake;
    [SerializeField] private float steeringDeadZone = 0.05f;
    [SerializeField] private float pedalDeadZone = 0.02f;

    [Header("Wheel Buttons")]
    [SerializeField] private string handbrakeButton = "button3";
    [SerializeField] private string ignitionButton = "button1";
    [SerializeField] private string gearDownButton = "button5";
    [SerializeField] private string gearUpButton = "button6";

    [Header("Keyboard Fallback")]
    [SerializeField] private bool combineWithKeyboard = true;
    [SerializeField] private Key keyboardHandbrake = Key.Space;
    [SerializeField] private Key keyboardIgnition = Key.K;
    [SerializeField] private float keyboardIgnitionHoldSeconds = 1.5f;
    [SerializeField] private Key keyboardGearDown = Key.Comma;
    [SerializeField] private Key keyboardGearUp = Key.Period;

    [Header("Visual Steering Wheel")]
    [SerializeField] private Transform visualSteeringWheel;
    [SerializeField] private Vector3 visualSteeringAxis = Vector3.forward;
    [SerializeField] private float visualSteeringWheelRange = 900.0f;
    [SerializeField] private float visualSteeringMaxDegreesPerSecond = 540.0f;
    [SerializeField] private bool invertVisualSteering;

    [Header("Legacy Input Manager Fallback (Optional)")]
    [SerializeField] private bool useLegacyJoystickFallback = true;
    [SerializeField] private string legacySteeringAxis = "WheelAxis1";
    [SerializeField] private string legacyThrottleAxis = "WheelAxis3";
    [SerializeField] private string legacyBrakeAxis = "WheelAxis3";
    [SerializeField] private bool legacyCombinedPedalAxis = true;
    [SerializeField] private bool legacyCombinedAxisSigned = true;
    [SerializeField] private bool autoCalibrateLegacyCombinedAxis = true;
    [SerializeField] private bool invertLegacyCombinedAxis;
    [SerializeField] private int legacyHandbrakeButton = 2;
    [SerializeField] private int legacyIgnitionButton = 0;
    [SerializeField] private int legacyGearDownButton = 4;
    [SerializeField] private int legacyGearUpButton = 5;

    private InputDevice _wheelDevice;
    private string _activeDeviceLabel = "None";
    private Quaternion _visualSteeringInitialRotation;
    private float _visualSteeringAngle;
    private float _lastSteer;
    private float _ignitionHoldUntil;
    private bool _ignitionHeld;
    private bool _combinedAxisCalibrated;
    private float _combinedAxisCenter;
    private float _combinedAxisMin;
    private float _combinedAxisMax;
    private bool _legacyCombinedAxisCalibrated;
    private float _legacyCombinedAxisCenter;
    private float _legacyCombinedAxisMin;
    private float _legacyCombinedAxisMax;

    private struct Inputs
    {
        public float steer;
        public float throttle;
        public float brake;
        public float handbrake;
    }

    private void Awake()
    {
        if (visualSteeringWheel != null)
            _visualSteeringInitialRotation = visualSteeringWheel.localRotation;
    }

    private void OnEnable()
    {
        ResolveVehicleTargets();
        ResolveWheelDevice(forceLog: true);
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void Update()
    {
        ResolveVehicleTargets();
        ResolveWheelDevice(forceLog: false);
        HandleButtons();
    }

    private void FixedUpdate()
    {
        if (vehicle == null && standardInput == null)
            return;

        Inputs wheel = ReadWheelOrLegacyInputs();
        Inputs keyboard = ReadKeyboardInputs();
        Inputs finalInputs = MergeInputs(wheel, keyboard);

        _lastSteer = finalInputs.steer;
        ApplyToVehicle(finalInputs);
        ApplyIgnitionKey();
    }

    private void LateUpdate()
    {
        if (visualSteeringWheel == null)
            return;

        float direction = invertVisualSteering ? -1.0f : 1.0f;
        float target = _lastSteer * visualSteeringWheelRange * 0.5f * direction;
        float step = Mathf.Max(0.0f, visualSteeringMaxDegreesPerSecond) * Time.deltaTime;
        _visualSteeringAngle = Mathf.MoveTowards(_visualSteeringAngle, target, step);

        Vector3 axis = visualSteeringAxis.sqrMagnitude > 0.0f ? visualSteeringAxis.normalized : Vector3.forward;
        visualSteeringWheel.localRotation = _visualSteeringInitialRotation * Quaternion.AngleAxis(_visualSteeringAngle, axis);
    }

    private void ResolveVehicleTargets()
    {
        if (autoFindVehicle && (vehicle == null || !vehicle.isActiveAndEnabled))
            vehicle = FindFirstObjectByType<VehicleBase>();

        if (autoFindStandardInput && (standardInput == null || !standardInput.isActiveAndEnabled))
            standardInput = FindFirstObjectByType<VPStandardInput>();
    }

    private void ResolveWheelDevice(bool forceLog)
    {
        if (_wheelDevice != null && _wheelDevice.added)
            return;

        _wheelDevice = FindPreferredWheelDevice();
        AutoConfigureKnownDeviceMapping();
        string nextLabel = _wheelDevice != null ? _wheelDevice.displayName : BuildFallbackDeviceLabel();

        if (forceLog || (logDeviceSelection && nextLabel != _activeDeviceLabel))
            Debug.Log($"[WheelBridge] Active wheel device: {nextLabel}");

        _activeDeviceLabel = nextLabel;
    }

    private void AutoConfigureKnownDeviceMapping()
    {
        if (_wheelDevice == null)
            return;

        string id = $"{_wheelDevice.displayName} {_wheelDevice.name} {_wheelDevice.layout}".ToLowerInvariant();

        // If the wheel is exposed as XInput/Gamepad, use trigger + stick mapping.
        if (id.Contains("xinput") || id.Contains("gamepad"))
        {
            steeringAxis = "leftStick/x";
            throttleAxis = "rightTrigger";
            brakeAxis = "leftTrigger";
            combinePedalsOnSingleAxis = false;
            combinedAxisSigned = false;
            return;
        }

        // Typical wheel mapping (HORI/DirectInput): steering on X and pedals on shared Z.
        if (id.Contains("hori") || id.Contains("wheel") || id.Contains("racing"))
        {
            steeringAxis = "x";
            throttleAxis = "z";
            brakeAxis = "z";
            combinePedalsOnSingleAxis = true;
            combinedAxisSigned = true;
        }
    }

    private InputDevice FindPreferredWheelDevice()
    {
        string preferred = preferredDeviceName.ToLowerInvariant();

        foreach (InputDevice device in InputSystem.devices)
        {
            if (device == null || !device.added)
                continue;

            string id = $"{device.displayName} {device.name} {device.layout}".ToLowerInvariant();
            if (!string.IsNullOrEmpty(preferred) && id.Contains(preferred))
                return device;

            if (id.Contains("hori") || id.Contains("racing wheel") || id.Contains("wheel"))
                return device;
        }

        return null;
    }

    private string BuildFallbackDeviceLabel()
    {
        if (useLegacyJoystickFallback)
        {
            string[] names = Input.GetJoystickNames();
            string preferred = preferredDeviceName.ToLowerInvariant();
            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                string lowered = name.ToLowerInvariant();
                if ((string.IsNullOrEmpty(preferred) || lowered.Contains(preferred)) || lowered.Contains("hori") || lowered.Contains("racing wheel") || lowered.Contains("wheel"))
                    return $"Legacy Joystick ({name})";
            }
        }

        return "None";
    }

    private Inputs ReadWheelOrLegacyInputs()
    {
        if (_wheelDevice == null && useLegacyJoystickFallback && HasLegacyJoystick())
            return ReadLegacyInputs();

        return ReadInputSystemWheelInputs();
    }

    private Inputs ReadInputSystemWheelInputs()
    {
        Inputs inputs = default;

        float steer = ReadDeviceAxis(steeringAxis);
        if (invertSteering)
            steer = -steer;
        inputs.steer = Mathf.Abs(steer) < steeringDeadZone ? 0.0f : Mathf.Clamp(steer, -1.0f, 1.0f);

        if (combinePedalsOnSingleAxis || throttleAxis == brakeAxis)
        {
            float combined = ReadDeviceAxis(throttleAxis);
            if (invertCombinedAxis)
                combined = -combined;
            SplitCombinedPedalAxis(
                combined,
                combinedAxisSigned,
                autoCalibrateCombinedAxis,
                ref _combinedAxisCalibrated,
                ref _combinedAxisCenter,
                ref _combinedAxisMin,
                ref _combinedAxisMax,
                out inputs.throttle,
                out inputs.brake);
        }
        else
        {
            inputs.throttle = ConvertSinglePedal(ReadDeviceAxis(throttleAxis), invertThrottle);
            inputs.brake = ConvertSinglePedal(ReadDeviceAxis(brakeAxis), invertBrake);
        }

        inputs.handbrake = ReadDeviceButton(handbrakeButton) ? 1.0f : 0.0f;
        return inputs;
    }

    private Inputs ReadLegacyInputs()
    {
        Inputs inputs = default;

        float steer = SafeLegacyAxis(legacySteeringAxis);
        if (Mathf.Abs(steer) < 0.001f && legacySteeringAxis.StartsWith("WheelAxis"))
            steer = SafeLegacyAxis("Horizontal");
        if (invertSteering)
            steer = -steer;
        inputs.steer = Mathf.Abs(steer) < steeringDeadZone ? 0.0f : Mathf.Clamp(steer, -1.0f, 1.0f);

        if (legacyCombinedPedalAxis || legacyThrottleAxis == legacyBrakeAxis)
        {
            float combined = SafeLegacyAxis(legacyThrottleAxis);
            if (Mathf.Abs(combined) < 0.001f && legacyThrottleAxis.StartsWith("WheelAxis"))
                combined = SafeLegacyAxis("Vertical");
            if (invertLegacyCombinedAxis)
                combined = -combined;
            SplitCombinedPedalAxis(
                combined,
                legacyCombinedAxisSigned,
                autoCalibrateLegacyCombinedAxis,
                ref _legacyCombinedAxisCalibrated,
                ref _legacyCombinedAxisCenter,
                ref _legacyCombinedAxisMin,
                ref _legacyCombinedAxisMax,
                out inputs.throttle,
                out inputs.brake);
        }
        else
        {
            inputs.throttle = ConvertSinglePedal(SafeLegacyAxis(legacyThrottleAxis), invertThrottle);
            inputs.brake = ConvertSinglePedal(SafeLegacyAxis(legacyBrakeAxis), invertBrake);
        }

        inputs.handbrake = Input.GetKey(LegacyButton(legacyHandbrakeButton)) ? 1.0f : 0.0f;
        return inputs;
    }

    private void SplitCombinedPedalAxis(
        float raw,
        bool signedAxis,
        bool autoCalibrate,
        ref bool calibrated,
        ref float center,
        ref float minValue,
        ref float maxValue,
        out float throttle,
        out float brake)
    {
        if (autoCalibrate)
        {
            if (!calibrated)
            {
                calibrated = true;
                center = raw;
                minValue = raw;
                maxValue = raw;
            }

            minValue = Mathf.Min(minValue, raw);
            maxValue = Mathf.Max(maxValue, raw);

            float upRange = Mathf.Max(0.0001f, maxValue - center);
            float downRange = Mathf.Max(0.0001f, center - minValue);
            float up = (raw - center) / upRange;
            float down = (center - raw) / downRange;

            throttle = up > pedalDeadZone ? Mathf.Clamp01(up) : 0.0f;
            brake = down > pedalDeadZone ? Mathf.Clamp01(down) : 0.0f;
        }
        else if (signedAxis)
        {
            throttle = raw > pedalDeadZone ? Mathf.Clamp01(raw) : 0.0f;
            brake = raw < -pedalDeadZone ? Mathf.Clamp01(-raw) : 0.0f;
        }
        else
        {
            float centered = raw * 2.0f - 1.0f;
            throttle = centered > pedalDeadZone ? Mathf.Clamp01(centered) : 0.0f;
            brake = centered < -pedalDeadZone ? Mathf.Clamp01(-centered) : 0.0f;
        }

        if (invertThrottle)
            throttle = 1.0f - throttle;
        if (invertBrake)
            brake = 1.0f - brake;
    }

    private float ConvertSinglePedal(float raw, bool invert)
    {
        float value = Mathf.Abs(raw) < pedalDeadZone ? 0.0f : Mathf.Clamp01(Mathf.Abs(raw));
        if (invert)
            value = 1.0f - value;
        return value;
    }

    private Inputs ReadKeyboardInputs()
    {
        Inputs inputs = default;
        if (!combineWithKeyboard || Keyboard.current == null)
            return inputs;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            inputs.steer -= 1.0f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            inputs.steer += 1.0f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            inputs.throttle = 1.0f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            inputs.brake = 1.0f;
        if (Keyboard.current[keyboardHandbrake].isPressed)
            inputs.handbrake = 1.0f;

        return inputs;
    }

    private Inputs MergeInputs(Inputs wheel, Inputs keyboard)
    {
        if (!combineWithKeyboard)
            return wheel;

        wheel.steer = Mathf.Abs(wheel.steer) > steeringDeadZone ? wheel.steer : keyboard.steer;
        wheel.throttle = Mathf.Max(wheel.throttle, keyboard.throttle);
        wheel.brake = Mathf.Max(wheel.brake, keyboard.brake);
        wheel.handbrake = Mathf.Max(wheel.handbrake, keyboard.handbrake);
        return wheel;
    }

    private void ApplyToVehicle(Inputs inputs)
    {
        if (standardInput != null)
        {
            standardInput.externalSteer = inputs.steer;
            standardInput.externalThrottle = inputs.throttle;
            standardInput.externalBrake = inputs.brake;
            standardInput.externalHandbrake = inputs.handbrake;
        }
        else if (vehicle != null)
        {
            vehicle.data.Set(Channel.Input, InputData.Steer, ToVehicleInput(inputs.steer));
            vehicle.data.Set(Channel.Input, InputData.Throttle, ToVehicleInput(inputs.throttle));
            vehicle.data.Set(Channel.Input, InputData.Brake, ToVehicleInput(inputs.brake));
            vehicle.data.Set(Channel.Input, InputData.Handbrake, ToVehicleInput(inputs.handbrake));
        }
    }

    private void HandleButtons()
    {
        if (vehicle == null)
            return;

        bool ignitionPressed = ReadDeviceButtonDown(ignitionButton) || ReadKeyboardDown(keyboardIgnition);
        bool ignitionHeld = ReadDeviceButton(ignitionButton) || ReadKeyboardHeld(keyboardIgnition);
        bool gearUpPressed = ReadDeviceButtonDown(gearUpButton) || ReadKeyboardDown(keyboardGearUp);
        bool gearDownPressed = ReadDeviceButtonDown(gearDownButton) || ReadKeyboardDown(keyboardGearDown);

        if (useLegacyJoystickFallback && HasLegacyJoystick())
        {
            ignitionPressed |= Input.GetKeyDown(LegacyButton(legacyIgnitionButton));
            ignitionHeld |= Input.GetKey(LegacyButton(legacyIgnitionButton));
            gearUpPressed |= Input.GetKeyDown(LegacyButton(legacyGearUpButton));
            gearDownPressed |= Input.GetKeyDown(LegacyButton(legacyGearDownButton));
        }

        QueueIgnitionKey(ignitionPressed, ignitionHeld);

        if (gearUpPressed)
            ShiftAutomaticGear(+1);
        if (gearDownPressed)
            ShiftAutomaticGear(-1);
    }

    private void ShiftAutomaticGear(int delta)
    {
        int current = vehicle.data.Get(Channel.Input, InputData.AutomaticGear);
        int next = Mathf.Clamp(current + delta, 0, 5);
        vehicle.data.Set(Channel.Input, InputData.AutomaticGear, next);
    }

    private void QueueIgnitionKey(bool pressed, bool held)
    {
        if (pressed)
            _ignitionHoldUntil = Time.time + keyboardIgnitionHoldSeconds;

        _ignitionHeld = held;
    }

    private void ApplyIgnitionKey()
    {
        if (vehicle == null)
            return;

        int keyState = vehicle.data.Get(Channel.Input, InputData.Key);
        if (_ignitionHeld || Time.time < _ignitionHoldUntil)
        {
            vehicle.data.Set(Channel.Input, InputData.Key, keyState < 0 ? 0 : 1);
            return;
        }

        if (keyState == 1)
            vehicle.data.Set(Channel.Input, InputData.Key, 0);
    }

    private float ReadDeviceAxis(string axisPath)
    {
        if (_wheelDevice == null || string.IsNullOrWhiteSpace(axisPath))
            return 0.0f;

        if (_wheelDevice is Gamepad gamepad)
        {
            switch (axisPath)
            {
                case "leftStick/x": return gamepad.leftStick.x.ReadValue();
                case "leftStick/y": return gamepad.leftStick.y.ReadValue();
                case "rightStick/x": return gamepad.rightStick.x.ReadValue();
                case "rightStick/y": return gamepad.rightStick.y.ReadValue();
                case "leftTrigger": return gamepad.leftTrigger.ReadValue();
                case "rightTrigger": return gamepad.rightTrigger.ReadValue();
            }
        }

        AxisControl axis = _wheelDevice.TryGetChildControl<AxisControl>(axisPath);
        return axis != null ? axis.ReadValue() : 0.0f;
    }

    private bool ReadDeviceButton(string buttonPath)
    {
        if (_wheelDevice == null || string.IsNullOrWhiteSpace(buttonPath))
            return false;

        ButtonControl button = _wheelDevice.TryGetChildControl<ButtonControl>(buttonPath);
        return button != null && button.isPressed;
    }

    private bool ReadDeviceButtonDown(string buttonPath)
    {
        if (_wheelDevice == null || string.IsNullOrWhiteSpace(buttonPath))
            return false;

        ButtonControl button = _wheelDevice.TryGetChildControl<ButtonControl>(buttonPath);
        return button != null && button.wasPressedThisFrame;
    }

    private bool ReadKeyboardHeld(Key key)
    {
        return Keyboard.current != null && Keyboard.current[key].isPressed;
    }

    private bool ReadKeyboardDown(Key key)
    {
        return Keyboard.current != null && Keyboard.current[key].wasPressedThisFrame;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (_wheelDevice == device && (change == InputDeviceChange.Removed || change == InputDeviceChange.Disconnected))
            _wheelDevice = null;
    }

    private bool HasLegacyJoystick()
    {
        string[] names = Input.GetJoystickNames();
        if (names == null)
            return false;

        for (int i = 0; i < names.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(names[i]))
                return true;
        }

        return false;
    }

    private static float SafeLegacyAxis(string axisName)
    {
        try
        {
            return Input.GetAxisRaw(axisName);
        }
        catch
        {
            return 0.0f;
        }
    }

    private static string LegacyButton(int index)
    {
        return $"joystick button {index}";
    }

    private static int ToVehicleInput(float value)
    {
        return Mathf.RoundToInt(Mathf.Clamp(value, -1.0f, 1.0f) * 10000.0f);
    }
}
