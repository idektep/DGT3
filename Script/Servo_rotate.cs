using UnityEngine;

public class MotorAutoAxisController : MonoBehaviour
{
    public enum Axis { X, Y, Z }          // ✅ เลือกแกนหมุนได้
    public Axis rotateAxis = Axis.Z;

    [Header("มุมหมุน")]
    public float closedAngle = 0f;
    public float openAngle = 90f;

    [Header("การเคลื่อนไหว")]
    public float speed = 90f;             // องศาต่อวินาที
    public float waitTime = 1f;           // เวลาหยุดค้างเมื่อถึงมุม

    private float targetAngle;
    private float timer;

    void Start()
    {
        targetAngle = openAngle; // เริ่มจากเปิดก่อน
    }

    void Update()
    {
        float current = GetCurrentAngle();
        float newAngle = Mathf.MoveTowardsAngle(current, targetAngle, speed * Time.deltaTime);
        SetRotation(newAngle);

        if (Mathf.Abs(Mathf.DeltaAngle(newAngle, targetAngle)) < 0.1f)
        {
            timer += Time.deltaTime;
            if (timer >= waitTime)
            {
                // 🔁 สลับมุมเป้าหมายกลับ
                targetAngle = Mathf.Approximately(targetAngle, openAngle) ? closedAngle : openAngle;
                timer = 0f;
            }
        }
    }

    // ✅ ใช้ร่วมกับ MQTT ได้ (สั่งหมุนไปมุมใดก็ได้)
    public void SetAngle(float angle)
    {
        targetAngle = angle;
    }

    // ===============================
    // 🔧 Helper Functions
    // ===============================

    float GetCurrentAngle()
    {
        Vector3 euler = transform.eulerAngles;
        if (rotateAxis == Axis.X) return euler.x;
        if (rotateAxis == Axis.Y) return euler.y;
        return euler.z;
    }

    void SetRotation(float angle)
    {
        Vector3 euler = transform.eulerAngles;
        if (rotateAxis == Axis.X) euler.x = angle;
        else if (rotateAxis == Axis.Y) euler.y = angle;
        else euler.z = angle;
        transform.eulerAngles = euler;
    }
}
