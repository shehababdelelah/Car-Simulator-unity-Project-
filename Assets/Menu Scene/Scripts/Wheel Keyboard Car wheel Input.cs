using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using VehiclePhysics;
using VehiclePhysics.UI;

[DisallowMultipleComponent]
public class WheelKeyboardVPPInput : MonoBehaviour
{
    [Header("Vehicle")]
    public VehicleBase vehicle;

    [Header("Keyboard")]
    public KeyCode throttleKey = KeyCode.W;
    public KeyCode brakeKey = KeyCode.S;
    public KeyCode steerLeftKey = KeyCode.A;
    public KeyCode steerRightKey = KeyCode.D;
    public KeyCode handbrakeKey = KeyCode.Space;
    public KeyCode ignitionKey = KeyCode.K;
    public bool startWithIgnitionOn;

    [Header("Keyboard Gears")]
    public KeyCode reverseGearKey = KeyCode.R;
    public KeyCode neutralGearKey = KeyCode.N;
    public KeyCode driveGearKey = KeyCode.F;
    public KeyCode toggleManualGearboxKey = KeyCode.M;
    public KeyCode gearUpKey = KeyCode.E;
    public KeyCode gearDownKey = KeyCode.Q;
    public bool startInManualGearbox;
    public bool disableAutoShiftInManual = true;

    [Header("Wheel Engine Buttons")]
    [FormerlySerializedAs("wheelIgnitionButton")]
    public KeyCode wheelStartEngineButton = KeyCode.JoystickButton0;
    public KeyCode wheelStopEngineButton = KeyCode.JoystickButton1;

    [Header("Wheel And Pedals Axes")]
    public string wheelSteerAxis = "WheelSteer";
    public string throttlePedalAxis = "WheelThrottle";
    public string brakePedalAxis = "WheelBrake";
    public bool invertSteerAxis;
    public bool invertThrottleAxis;
    public bool invertBrakeAxis;

    [Header("Tuning")]
    [Range(0f, 1f)] public float keyboardSteerStep = 0.04f;
    [Range(0f, 1f)] public float keyboardSteerReturnStep = 0.08f;
    [Range(0f, 0.5f)] public float axisDeadZone = 0.05f;

    [Header("Speed Needle Debug")]
    public bool logSpeedNeedleOnThrottle = true;
    public bool logSpeedNeedleWhileThrottleHeld = true;
    public bool logNeedleEquationOnStart = true;
    public bool logFallbackNeedleAngle = true;
    [Min(0.02f)] public float speedNeedleLogInterval = 0.25f;
    public Dashboard speedDashboard;
    public float speedNeedleMinKph = 0f;
    public float speedNeedleMaxKph = 225f;
    public float speedNeedleMinAngle = 0f;
    public float speedNeedleMaxAngle = -270f;

    float keyboardSteer;
    bool ignitionOn;
    bool manualGearbox;
    bool throttleWasActive;
    float nextThrottleSpeedLogTime;
    int throttleSpeedSampleIndex;
    readonly HashSet<string> disabledAxisNames = new HashSet<string>();

    void Awake()
    {
        if (vehicle == null)
            vehicle = GetComponent<VehicleBase>();

        ignitionOn = startWithIgnitionOn;
        manualGearbox = startInManualGearbox;

        if (speedDashboard == null)
            speedDashboard = GetComponentInChildren<Dashboard>();

        if (speedDashboard == null)
            speedDashboard = FindFirstObjectByType<Dashboard>();

        if (logNeedleEquationOnStart)
            LogNeedleEquations();

        ApplyGearboxMode();
    }

    void Update()
    {
        if (vehicle == null) return;

        if (Input.GetKeyDown(ignitionKey))
            ignitionOn = !ignitionOn;

        if (Input.GetKeyDown(wheelStartEngineButton))
            ignitionOn = true;

        if (Input.GetKeyDown(wheelStopEngineButton))
            ignitionOn = false;

        HandleGearKeys();

        float steer = CombineSigned(ReadKeyboardSteer(), ReadAxis(wheelSteerAxis, invertSteerAxis));
        float throttle = Mathf.Max(ReadKeyboardButton(throttleKey), ReadPedal(throttlePedalAxis, invertThrottleAxis));
        float brake = Mathf.Max(ReadKeyboardButton(brakeKey), ReadPedal(brakePedalAxis, invertBrakeAxis));
        float handbrake = ReadKeyboardButton(handbrakeKey);

        SetInput(InputData.Steer, steer);
        SetInput(InputData.Throttle, throttle);
        SetInput(InputData.Brake, brake);
        SetInput(InputData.Handbrake, handbrake);
        SetInput(InputData.Clutch, 0f);
        vehicle.data.Set(Channel.Input, InputData.Key, ignitionOn ? 1 : -1);

        HandleThrottleSpeedNeedleLog(throttle);
    }

    void HandleGearKeys()
    {
        if (Input.GetKeyDown(toggleManualGearboxKey))
        {
            manualGearbox = !manualGearbox;
            ApplyGearboxMode();
        }

        if (Input.GetKeyDown(reverseGearKey))
            SetAutomaticGear(2);

        if (Input.GetKeyDown(neutralGearKey))
            SetAutomaticGear(3);

        if (Input.GetKeyDown(driveGearKey))
            SetAutomaticGear(4);

        if (Input.GetKeyDown(gearUpKey))
            ShiftGear(+1);

        if (Input.GetKeyDown(gearDownKey))
            ShiftGear(-1);
    }

    void ApplyGearboxMode()
    {
        if (vehicle == null) return;

        VPVehicleController controller = vehicle.GetComponent<VPVehicleController>();
        if (controller == null) return;

        controller.gearbox.type = manualGearbox ? Gearbox.Type.Manual : Gearbox.Type.Automatic;

        if (manualGearbox)
        {
            vehicle.data.Set(Channel.Settings, SettingsData.AutoShiftOverride, disableAutoShiftInManual ? 2 : 1);
        }
        else
        {
            vehicle.data.Set(Channel.Settings, SettingsData.AutoShiftOverride, 0);
            SetAutomaticGear(4);
        }
    }

    void ShiftGear(int direction)
    {
        int current = vehicle.data.Get(Channel.Input, InputData.AutomaticGear);
        SetAutomaticGear(current + direction);
    }

    void SetAutomaticGear(int gear)
    {
        vehicle.data.Set(Channel.Input, InputData.AutomaticGear, Mathf.Clamp(gear, 0, 5));
    }

    float ReadKeyboardSteer()
    {
        float target = 0f;

        if (Input.GetKey(steerLeftKey)) target -= 1f;
        if (Input.GetKey(steerRightKey)) target += 1f;

        float step = target == 0f ? keyboardSteerReturnStep : keyboardSteerStep;
        keyboardSteer = Mathf.MoveTowards(keyboardSteer, target, step);
        return keyboardSteer;
    }

    float ReadKeyboardButton(KeyCode key)
    {
        return Input.GetKey(key) ? 1f : 0f;
    }

    float ReadPedal(string axisName, bool invert)
    {
        float value = ReadAxis(axisName, invert);

        // Many pedals report -1 when released and +1 when pressed.
        if (value < 0f)
            value = (value + 1f) * 0.5f;

        return Mathf.Clamp01(value);
    }

    float ReadAxis(string axisName, bool invert)
    {
        if (string.IsNullOrWhiteSpace(axisName))
            return 0f;

        if (disabledAxisNames.Contains(axisName))
            return 0f;

        float value;

        try
        {
            value = Input.GetAxis(axisName);
        }
        catch (System.ArgumentException)
        {
            disabledAxisNames.Add(axisName);
            Debug.LogWarning($"{nameof(WheelKeyboardVPPInput)}: Input axis '{axisName}' does not exist. Add it in Project Settings > Input Manager, or clear this field.");
            return 0f;
        }

        if (invert) value = -value;
        return Mathf.Abs(value) < axisDeadZone ? 0f : Mathf.Clamp(value, -1f, 1f);
    }

    float CombineSigned(float keyboardValue, float wheelValue)
    {
        return Mathf.Abs(wheelValue) > axisDeadZone ? wheelValue : keyboardValue;
    }

    void SetInput(int input, float value)
    {
        vehicle.data.Set(Channel.Input, input, Mathf.RoundToInt(Mathf.Clamp(value, -1f, 1f) * 10000f));
    }

    void HandleThrottleSpeedNeedleLog(float throttle)
    {
        bool throttleActive = throttle > 0.01f;

        if (!logSpeedNeedleOnThrottle)
        {
            throttleWasActive = throttleActive;
            return;
        }

        if (!throttleActive)
        {
            throttleWasActive = false;
            nextThrottleSpeedLogTime = 0f;
            return;
        }

        bool justPressed = !throttleWasActive;
        bool intervalReached = logSpeedNeedleWhileThrottleHeld && Time.time >= nextThrottleSpeedLogTime;

        if (justPressed || intervalReached)
        {
            LogSpeedNeedleSnapshot(justPressed ? "pressed" : "held");
            nextThrottleSpeedLogTime = Time.time + Mathf.Max(0.02f, speedNeedleLogInterval);
        }

        throttleWasActive = true;
    }

    void LogSpeedNeedleSnapshot(string state)
    {
        float speedMs = vehicle.data.Get(Channel.Vehicle, VehicleData.Speed) / 1000f;
        if (speedMs < 0f)
            speedMs = 0f;

        float speedKph = speedMs * 3.6f;
        float needleAngle = GetSpeedNeedleAngle(speedKph);
        float fallbackNeedleAngle = MapNeedleAngle(speedKph, speedNeedleMinKph, speedNeedleMaxKph, speedNeedleMinAngle, speedNeedleMaxAngle);
        string fallbackInfo = logFallbackNeedleAngle
            ? $", fallbackGaugeAngle={fallbackNeedleAngle:0.00} deg"
            : string.Empty;

        Debug.Log(
            $"[Throttle Speed] {state} #{++throttleSpeedSampleIndex}: speed={speedKph:0.00} km/h ({speedMs:0.00} m/s), dashboardNeedleAngle={needleAngle:0.00} deg{fallbackInfo}",
            this);
    }

    float GetSpeedNeedleAngle(float speedKph)
    {
        Dashboard.Needle needle = speedDashboard != null ? speedDashboard.speedNeedle : null;
        if (needle != null)
            return MapNeedleAngle(speedKph, needle.minValue, needle.maxValue, needle.angleAtMinValue, needle.angleAtMaxValue);

        return MapNeedleAngle(speedKph, speedNeedleMinKph, speedNeedleMaxKph, speedNeedleMinAngle, speedNeedleMaxAngle);
    }

    void LogNeedleEquations()
    {
        Dashboard.Needle needle = speedDashboard != null ? speedDashboard.speedNeedle : null;
        if (needle != null)
        {
            Debug.Log(
                $"[Throttle Speed] Dashboard equation: angle = {needle.angleAtMinValue:0.###} + ((speedKph - {needle.minValue:0.###}) / ({needle.maxValue:0.###} - {needle.minValue:0.###})) * ({needle.angleAtMaxValue:0.###} - {needle.angleAtMinValue:0.###})",
                this);
        }

        Debug.Log(
            $"[Throttle Speed] Fallback gauge equation: angle = {speedNeedleMinAngle:0.###} + ((speedKph - {speedNeedleMinKph:0.###}) / ({speedNeedleMaxKph:0.###} - {speedNeedleMinKph:0.###})) * ({speedNeedleMaxAngle:0.###} - {speedNeedleMinAngle:0.###})",
            this);
    }

    float MapNeedleAngle(float value, float minValue, float maxValue, float minAngle, float maxAngle)
    {
        if (Mathf.Approximately(maxValue, minValue))
            return minAngle;

        float t = (value - minValue) / (maxValue - minValue);
        return Mathf.LerpUnclamped(minAngle, maxAngle, t);
    }
}
