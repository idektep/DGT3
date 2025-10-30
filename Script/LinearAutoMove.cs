using UnityEngine;

public class LinearAutoMove : MonoBehaviour
{
    public enum Axis { X, Y, Z }     // ✅ เลือกทิศทางการเคลื่อน
    [Header("ตั้งค่าแกนเคลื่อนที่")]
    public Axis moveAxis = Axis.Z;

    [Header("ตำแหน่งเริ่มและสิ้นสุด")]
    public float startPos = 0f;      // จุดเริ่มต้น
    public float endPos = 2f;        // จุดปลายทาง

    [Header("การเคลื่อนไหว")]
    public float speed = 1f;         // ความเร็ว (หน่วยต่อวินาที)
    public float waitTime = 1f;      // เวลาหยุดค้างเมื่อถึงจุด

    private float targetPos;
    private float waitTimer;

    void Start()
    {
        targetPos = endPos;          // เริ่มจากวิ่งไปปลายทางก่อน
    }

    void Update()
    {
        float current = GetCurrentPosition();
        float newPos = Mathf.MoveTowards(current, targetPos, speed * Time.deltaTime);
        SetPosition(newPos);

        // ถ้าเคลื่อนถึงเป้าหมาย → รอก่อนกลับ
        if (Mathf.Abs(newPos - targetPos) < 0.001f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                targetPos = Mathf.Approximately(targetPos, endPos) ? startPos : endPos;
                waitTimer = 0f;
            }
        }
    }

    // 🧭 ดึงค่าตำแหน่งตามแกนที่เลือก
    float GetCurrentPosition()
    {
        Vector3 p = transform.localPosition;
        if (moveAxis == Axis.X) return p.x;
        if (moveAxis == Axis.Y) return p.y;
        return p.z;
    }

    // ⚙️ ตั้งตำแหน่งใหม่ตามแกนที่เลือก
    void SetPosition(float value)
    {
        Vector3 p = transform.localPosition;
        if (moveAxis == Axis.X) p.x = value;
        else if (moveAxis == Axis.Y) p.y = value;
        else p.z = value;
        transform.localPosition = p;
    }
}
