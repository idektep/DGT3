using UnityEngine;

public class RobotArmPoseController : MonoBehaviour
{
    // =========================================================
    // DATA
    // =========================================================

    [Header("Data")]
    public DataCore poseData;


    // =========================================================
    // ROBOT AXIS
    // =========================================================

    [Header("Axis Transforms")]
    public Transform Axis1;
    public Transform Axis2;
    public Transform Axis3;
    public Transform Axis4;


    // =========================================================
    // GRIPPER
    // =========================================================

    [Header("Gripper")]
    public Transform ClampU;
    public Transform ClampD;

    public float gripperMaxOpen = 130f;
    public float gripperMaxDistance = 0.015f;
    public float gripperSpeed = 0.02f;

    public bool invertGripper = false;


    // =========================================================
    // AXIS DIRECTION
    // =========================================================

    [Header("Invert Axis Direction")]
    public bool invertAxis1 = true;
    public bool invertAxis2 = false;
    public bool invertAxis3 = false;
    public bool invertAxis4 = false;


    // =========================================================
    // AXIS OFFSET
    // =========================================================

    [Header("Initial Offset per Axis")]
    public float offsetAxis1 = 0f;
    public float offsetAxis2 = 0f;
    public float offsetAxis3 = 0f;
    public float offsetAxis4 = 0f;


    // =========================================================
    // MOVEMENT
    // =========================================================

    [Header("Rotation Speed")]
    public float rotationSpeed = 90f;


    // =========================================================
    // INTERNAL DATA
    // =========================================================

    private Quaternion targetRot1;
    private Quaternion targetRot2;
    private Quaternion targetRot3;
    private Quaternion targetRot4;

    private Vector3 clampU_StartPos;
    private Vector3 clampD_StartPos;

    private Vector3 clampU_TargetPos;
    private Vector3 clampD_TargetPos;


    // =========================================================
    // ENABLE / DISABLE
    // =========================================================

    void OnEnable()
    {
        if (poseData != null)
        {
            poseData.OnStringChanged += SetPoseFromString;
        }
    }


    void OnDisable()
    {
        if (poseData != null)
        {
            poseData.OnStringChanged -= SetPoseFromString;
        }
    }


    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        // -------------------------
        // Save initial gripper pos
        // -------------------------

        if (ClampU != null)
        {
            clampU_StartPos = ClampU.localPosition;
            clampU_TargetPos = clampU_StartPos;
        }

        if (ClampD != null)
        {
            clampD_StartPos = ClampD.localPosition;
            clampD_TargetPos = clampD_StartPos;
        }


        // -------------------------
        // Save initial rotation
        // -------------------------

        if (Axis1 != null)
            targetRot1 = Axis1.localRotation;

        if (Axis2 != null)
            targetRot2 = Axis2.localRotation;

        if (Axis3 != null)
            targetRot3 = Axis3.localRotation;

        if (Axis4 != null)
            targetRot4 = Axis4.localRotation;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    void Update()
    {
        // -------------------------
        // AXIS 1
        // -------------------------

        if (Axis1 != null)
        {
            Axis1.localRotation =
                Quaternion.RotateTowards(
                    Axis1.localRotation,
                    targetRot1,
                    rotationSpeed * Time.deltaTime
                );
        }


        // -------------------------
        // AXIS 2
        // -------------------------

        if (Axis2 != null)
        {
            Axis2.localRotation =
                Quaternion.RotateTowards(
                    Axis2.localRotation,
                    targetRot2,
                    rotationSpeed * Time.deltaTime
                );
        }


        // -------------------------
        // AXIS 3
        // -------------------------

        if (Axis3 != null)
        {
            Axis3.localRotation =
                Quaternion.RotateTowards(
                    Axis3.localRotation,
                    targetRot3,
                    rotationSpeed * Time.deltaTime
                );
        }


        // -------------------------
        // AXIS 4
        // -------------------------

        if (Axis4 != null)
        {
            Axis4.localRotation =
                Quaternion.RotateTowards(
                    Axis4.localRotation,
                    targetRot4,
                    rotationSpeed * Time.deltaTime
                );
        }


        // -------------------------
        // GRIPPER U
        // -------------------------

        if (ClampU != null)
        {
            ClampU.localPosition =
                Vector3.MoveTowards(
                    ClampU.localPosition,
                    clampU_TargetPos,
                    gripperSpeed * Time.deltaTime
                );
        }


        // -------------------------
        // GRIPPER D
        // -------------------------

        if (ClampD != null)
        {
            ClampD.localPosition =
                Vector3.MoveTowards(
                    ClampD.localPosition,
                    clampD_TargetPos,
                    gripperSpeed * Time.deltaTime
                );
        }
    }


    // =========================================================
    // RECEIVE ROBOT POSE
    // =========================================================

    public void SetPoseFromString(string poseStr)
    {
        if (string.IsNullOrWhiteSpace(poseStr))
            return;


        string[] parts = poseStr.Split(',');


        if (parts.Length < 5)
        {
            Debug.LogWarning(
                "Invalid Robot Pose : " + poseStr
            );

            return;
        }


        // =====================================================
        // AXIS 1
        // =====================================================

        if (
            float.TryParse(
                parts[0].Trim(),
                out float a1
            )
        )
        {
            float angle =
                (a1 - 90f)
                + offsetAxis1;


            if (invertAxis1)
                angle *= -1f;


            targetRot1 =
                Quaternion.Euler(
                    0f,
                    angle,
                    0f
                );
        }


        // =====================================================
        // AXIS 2
        // =====================================================

        if (
            float.TryParse(
                parts[1].Trim(),
                out float a2
            )
        )
        {
            float angle =
                (a2 - 90f)
                + offsetAxis2;


            if (invertAxis2)
                angle *= -1f;


            targetRot2 =
                Quaternion.Euler(
                    angle,
                    0f,
                    0f
                );
        }


        // =====================================================
        // AXIS 3
        // =====================================================

        if (
            float.TryParse(
                parts[2].Trim(),
                out float a3
            )
        )
        {
            float angle =
                (a3 - 90f)
                + offsetAxis3;


            if (invertAxis3)
                angle *= -1f;


            targetRot3 =
                Quaternion.Euler(
                    angle,
                    0f,
                    0f
                );
        }


        // =====================================================
        // AXIS 4
        // =====================================================

        if (
            float.TryParse(
                parts[3].Trim(),
                out float a4
            )
        )
        {
            float angle =
                (a4 - 90f)
                + offsetAxis4;


            if (invertAxis4)
                angle *= -1f;


            targetRot4 =
                Quaternion.Euler(
                    angle,
                    0f,
                    0f
                );
        }


        // =====================================================
        // GRIPPER
        // =====================================================

        if (
            float.TryParse(
                parts[4].Trim(),
                out float gripperAngle
            )
        )
        {
            float ratio =
                Mathf.Clamp01(
                    gripperAngle /
                    gripperMaxOpen
                );


            float distance =
                ratio *
                gripperMaxDistance;


            float direction =
                invertGripper
                ? -1f
                : 1f;


            if (ClampU != null)
            {
                clampU_TargetPos =
                    clampU_StartPos
                    +
                    new Vector3(
                        direction * distance,
                        0f,
                        0f
                    );
            }


            if (ClampD != null)
            {
                clampD_TargetPos =
                    clampD_StartPos
                    +
                    new Vector3(
                        -direction * distance,
                        0f,
                        0f
                    );
            }
        }
    }
}