using UnityEngine;

[DisallowMultipleComponent]
public class DriverArmSteeringPose : MonoBehaviour
{
    [Header("Input")]
    public string steerAxis = "WheelSteer";
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public bool invertAxis;

    [Header("Left Arm Bones")]
    public Transform leftShoulder;
    public Transform leftUpperArm;
    public Transform leftForearm;
    public Transform leftHand;

    [Header("Right Arm Bones")]
    public Transform rightShoulder;
    public Transform rightUpperArm;
    public Transform rightForearm;
    public Transform rightHand;

    [Header("Steering Pose")]
    [Range(0f, 60f)] public float shoulderTurn = 12f;
    [Range(0f, 80f)] public float upperArmTurn = 22f;
    [Range(0f, 80f)] public float forearmTurn = 28f;
    [Range(0f, 120f)] public float handTurn = 70f;
    public float smoothSpeed = 8f;
    public float axisDeadZone = 0.05f;

    BonePose leftShoulderStart;
    BonePose leftUpperArmStart;
    BonePose leftForearmStart;
    BonePose leftHandStart;
    BonePose rightShoulderStart;
    BonePose rightUpperArmStart;
    BonePose rightForearmStart;
    BonePose rightHandStart;

    float currentSteer;
    bool axisMissing;

    void Awake()
    {
        leftShoulderStart = new BonePose(leftShoulder);
        leftUpperArmStart = new BonePose(leftUpperArm);
        leftForearmStart = new BonePose(leftForearm);
        leftHandStart = new BonePose(leftHand);
        rightShoulderStart = new BonePose(rightShoulder);
        rightUpperArmStart = new BonePose(rightUpperArm);
        rightForearmStart = new BonePose(rightForearm);
        rightHandStart = new BonePose(rightHand);
    }

    void LateUpdate()
    {
        currentSteer = Mathf.MoveTowards(currentSteer, ReadSteerInput(), smoothSpeed * Time.deltaTime);

        ApplyBone(leftShoulder, leftShoulderStart, Vector3.forward, -currentSteer * shoulderTurn);
        ApplyBone(leftUpperArm, leftUpperArmStart, Vector3.forward, -currentSteer * upperArmTurn);
        ApplyBone(leftForearm, leftForearmStart, Vector3.forward, -currentSteer * forearmTurn);
        ApplyBone(leftHand, leftHandStart, Vector3.forward, -currentSteer * handTurn);

        ApplyBone(rightShoulder, rightShoulderStart, Vector3.forward, -currentSteer * shoulderTurn);
        ApplyBone(rightUpperArm, rightUpperArmStart, Vector3.forward, -currentSteer * upperArmTurn);
        ApplyBone(rightForearm, rightForearmStart, Vector3.forward, -currentSteer * forearmTurn);
        ApplyBone(rightHand, rightHandStart, Vector3.forward, -currentSteer * handTurn);
    }

    void ApplyBone(Transform bone, BonePose startPose, Vector3 axis, float angle)
    {
        if (bone == null || !startPose.IsValid) return;
        bone.localRotation = startPose.LocalRotation * Quaternion.AngleAxis(angle, axis);
    }

    float ReadSteerInput()
    {
        float keyboard = 0f;
        if (Input.GetKey(leftKey)) keyboard -= 1f;
        if (Input.GetKey(rightKey)) keyboard += 1f;

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
            if (invertAxis) value = -value;
            return Mathf.Abs(value) < axisDeadZone ? 0f : Mathf.Clamp(value, -1f, 1f);
        }
        catch (System.ArgumentException)
        {
            axisMissing = true;
            Debug.LogWarning($"{nameof(DriverArmSteeringPose)}: Input axis '{steerAxis}' does not exist.");
            return 0f;
        }
    }

    readonly struct BonePose
    {
        public readonly bool IsValid;
        public readonly Quaternion LocalRotation;

        public BonePose(Transform bone)
        {
            IsValid = bone != null;
            LocalRotation = bone != null ? bone.localRotation : Quaternion.identity;
        }
    }
}
