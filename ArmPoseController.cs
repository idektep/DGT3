using UnityEngine;

public class ArmPoseController : MonoBehaviour
{
    [Header("MQTT Reference")]
    public MQTTCore mqtt;   // ต้องลากใน Inspector

    [Header("MQTT Topic Settings")]
    public string topicPrefix = "idt";
    public string poseTopic = "arm/status/pose";

    [Header("Axis Transforms")]
    public Transform Axis1;
    public Transform Axis2;
    public Transform Axis3;
    public Transform Axis4;

    [Header("Gripper")]
    public Transform ClampU;
    public Transform ClampD;
    public float gripperMaxOpen = 130f;
    public float gripperMaxDistance = 0.015f;
    public float gripperSpeed = 0.02f;
    public bool invertGripper = false;

    [Header("Invert Axis Direction")]
    public bool invertAxis1 = true;
    public bool invertAxis2 = false;
    public bool invertAxis3 = false;
    public bool invertAxis4 = false;

    [Header("Initial Offset per Axis (degrees)")]
    public float offsetAxis1 = 0f;
    public float offsetAxis2 = 0f;
    public float offsetAxis3 = 0f;
    public float offsetAxis4 = 0f;

    [Header("Rotation Speed (deg/sec)")]
    public float rotationSpeed = 90f;

    private Quaternion targetRot1, targetRot2, targetRot3, targetRot4;
    private Vector3 clampU_StartPos, clampD_StartPos;
    private Vector3 clampU_TargetPos, clampD_TargetPos;

    void OnEnable()
    {
        MQTTCore.OnConnected += OnMQTTConnected;
        MQTTCore.OnMessageReceived += HandleMQTT;
    }

    void OnDisable()
    {
        MQTTCore.OnConnected -= OnMQTTConnected;
        MQTTCore.OnMessageReceived -= HandleMQTT;
    }

    void Start()
    {
        if (ClampU) clampU_StartPos = ClampU.localPosition;
        if (ClampD) clampD_StartPos = ClampD.localPosition;

        if (Axis1) targetRot1 = Axis1.localRotation;
        if (Axis2) targetRot2 = Axis2.localRotation;
        if (Axis3) targetRot3 = Axis3.localRotation;
        if (Axis4) targetRot4 = Axis4.localRotation;
    }

    void OnMQTTConnected()
    {
        string fullTopic = $"{topicPrefix}/{poseTopic}";
        mqtt.Subscribe(fullTopic);
        Debug.Log($"🛰 Subscribed (after connect): {fullTopic}");
    }

    void HandleMQTT(string topic, string msg)
    {
        string fullTopic = $"{topicPrefix}/{poseTopic}";

        if (topic == fullTopic)
        {
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
            {
                SetPoseFromString(msg);
            });
        }
    }

    void Update()
    {
        if (Axis1) Axis1.localRotation = Quaternion.RotateTowards(Axis1.localRotation, targetRot1, rotationSpeed * Time.deltaTime);
        if (Axis2) Axis2.localRotation = Quaternion.RotateTowards(Axis2.localRotation, targetRot2, rotationSpeed * Time.deltaTime);
        if (Axis3) Axis3.localRotation = Quaternion.RotateTowards(Axis3.localRotation, targetRot3, rotationSpeed * Time.deltaTime);
        if (Axis4) Axis4.localRotation = Quaternion.RotateTowards(Axis4.localRotation, targetRot4, rotationSpeed * Time.deltaTime);

        if (ClampU) ClampU.localPosition = Vector3.MoveTowards(ClampU.localPosition, clampU_TargetPos, gripperSpeed * Time.deltaTime);
        if (ClampD) ClampD.localPosition = Vector3.MoveTowards(ClampD.localPosition, clampD_TargetPos, gripperSpeed * Time.deltaTime);
    }

    public void SetPoseFromString(string poseStr)
    {
        string[] parts = poseStr.Split(',');
        if (parts.Length < 5) return;

        if (float.TryParse(parts[0], out float a1))
        {
            float y = ((a1 - 90f) + offsetAxis1) * (invertAxis1 ? -1f : 1f);
            targetRot1 = Quaternion.Euler(0f, y, 0f);
        }

        if (float.TryParse(parts[1], out float a2))
        {
            float x = ((a2 - 90f) + offsetAxis2) * (invertAxis2 ? -1f : 1f);
            targetRot2 = Quaternion.Euler(x, 0f, 0f);
        }

        if (float.TryParse(parts[2], out float a3))
        {
            float x = ((a3 - 90f) + offsetAxis3) * (invertAxis3 ? -1f : 1f);
            targetRot3 = Quaternion.Euler(x, 0f, 0f);
        }

        if (float.TryParse(parts[3], out float a4))
        {
            float x = ((a4 - 90f) + offsetAxis4) * (invertAxis4 ? -1f : 1f);
            targetRot4 = Quaternion.Euler(x, 0f, 0f);
        }

        if (float.TryParse(parts[4], out float gAngle))
        {
            float ratio = Mathf.Clamp01(gAngle / gripperMaxOpen);
            float offset = ratio * gripperMaxDistance;

            float dir = invertGripper ? -1f : 1f;

            if (ClampU) clampU_TargetPos = clampU_StartPos + new Vector3(dir * offset, 0f, 0f);
            if (ClampD) clampD_TargetPos = clampD_StartPos + new Vector3(-dir * offset, 0f, 0f);
        }
    }
}
