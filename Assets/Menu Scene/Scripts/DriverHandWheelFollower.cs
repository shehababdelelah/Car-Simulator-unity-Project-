using UnityEngine;

[DisallowMultipleComponent]
public class DriverHandWheelFollower : MonoBehaviour
{
    [Header("Hand Bones")]
    public Transform leftHand;
    public Transform rightHand;
    public Transform leftForearm;
    public Transform rightForearm;

    [Header("Wheel Grip Points")]
    public Transform leftGripPoint;
    public Transform rightGripPoint;

    [Header("Follow Settings")]
    [Range(0f, 1f)] public float positionWeight = 1f;
    [Range(0f, 1f)] public float rotationWeight = 1f;
    public float followSpeed = 18f;

    [Header("Extra Wrist Rotation")]
    public Vector3 leftHandRotationOffset;
    public Vector3 rightHandRotationOffset;
    public bool swapHands;

    void LateUpdate()
    {
        Transform targetLeftGrip = swapHands ? rightGripPoint : leftGripPoint;
        Transform targetRightGrip = swapHands ? leftGripPoint : rightGripPoint;

        FollowGrip(leftHand, targetLeftGrip, leftHandRotationOffset);
        FollowGrip(rightHand, targetRightGrip, rightHandRotationOffset);

        AimForearm(leftForearm, leftHand);
        AimForearm(rightForearm, rightHand);
    }

    void FollowGrip(Transform hand, Transform grip, Vector3 rotationOffset)
    {
        if (hand == null || grip == null) return;

        float t = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);

        hand.position = Vector3.Lerp(hand.position, grip.position, t * positionWeight);
        Quaternion targetRotation = grip.rotation * Quaternion.Euler(rotationOffset);
        hand.rotation = Quaternion.Slerp(hand.rotation, targetRotation, t * rotationWeight);
    }

    void AimForearm(Transform forearm, Transform hand)
    {
        if (forearm == null || hand == null) return;

        Vector3 direction = hand.position - forearm.position;
        if (direction.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction, forearm.up);
        forearm.rotation = Quaternion.Slerp(forearm.rotation, targetRotation, 0.25f);
    }
}
