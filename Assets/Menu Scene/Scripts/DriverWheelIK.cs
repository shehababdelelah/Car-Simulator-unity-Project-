using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class DriverWheelIK : MonoBehaviour
{
    [Header("Wheel Grip Points")]
    public Transform leftGripPoint;
    public Transform rightGripPoint;

    [Header("Elbow Hint Points")]
    public Transform leftElbowHint;
    public Transform rightElbowHint;

    [Header("IK Weights")]
    [Range(0f, 1f)] public float handPositionWeight = 1f;
    [Range(0f, 1f)] public float handRotationWeight = 0.75f;
    [Range(0f, 1f)] public float elbowHintWeight = 0.8f;

    [Header("Hand Rotation Offsets")]
    public Vector3 leftHandRotationOffset;
    public Vector3 rightHandRotationOffset;

    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        ApplyHandIK(
            AvatarIKGoal.LeftHand,
            AvatarIKHint.LeftElbow,
            leftGripPoint,
            leftElbowHint,
            leftHandRotationOffset);

        ApplyHandIK(
            AvatarIKGoal.RightHand,
            AvatarIKHint.RightElbow,
            rightGripPoint,
            rightElbowHint,
            rightHandRotationOffset);
    }

    void ApplyHandIK(
        AvatarIKGoal handGoal,
        AvatarIKHint elbowHint,
        Transform gripPoint,
        Transform hintPoint,
        Vector3 rotationOffset)
    {
        if (gripPoint == null)
        {
            animator.SetIKPositionWeight(handGoal, 0f);
            animator.SetIKRotationWeight(handGoal, 0f);
            animator.SetIKHintPositionWeight(elbowHint, 0f);
            return;
        }

        animator.SetIKPositionWeight(handGoal, handPositionWeight);
        animator.SetIKRotationWeight(handGoal, handRotationWeight);
        animator.SetIKPosition(handGoal, gripPoint.position);
        animator.SetIKRotation(handGoal, gripPoint.rotation * Quaternion.Euler(rotationOffset));

        if (hintPoint == null)
        {
            animator.SetIKHintPositionWeight(elbowHint, 0f);
            return;
        }

        animator.SetIKHintPositionWeight(elbowHint, elbowHintWeight);
        animator.SetIKHintPosition(elbowHint, hintPoint.position);
    }
}
