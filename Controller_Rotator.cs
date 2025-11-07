using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Controller_Rotator : MonoBehaviour
{
    /* ──────────────────────────────────────────────
     *  🧩 SECTION 1 : MQTT CONFIGURATION
     * ────────────────────────────────────────────── */
    [Header("MQTT Settings")]
    [Tooltip("Topic สำหรับควบคุมมอเตอร์ เช่น idt/motor/1")]
    public string topic = "idt/motor/1";

    /* ──────────────────────────────────────────────
     *  ⚙️ SECTION 2 : ROTATION PARAMETERS
     * ────────────────────────────────────────────── */
    [Header("Rotation Settings")]
    [Tooltip("แกนที่ต้องการหมุน เช่น Y = (0,1,0)")]
    public Vector3 rotationAxis = Vector3.up;
    [Tooltip("ความเร็วเริ่มต้น (องศาต่อวินาที)")]
    public float speed = 100f;

    // ค่าภายใน runtime
    private float currentSpeed;   // ความเร็วที่ใช้งานจริง
    private bool isRotating = false;
    private int direction = 1;    // 1 = CW, -1 = CCW

    /* ──────────────────────────────────────────────
     *  💬 SECTION 3 : MQTT MESSAGE HANDLING
     * ────────────────────────────────────────────── */
    private Queue<(string, string)> messageQueue = new Queue<(string, string)>();
    private object queueLock = new object();

    IEnumerator Start()
    {
        // กำหนดความเร็วเริ่มต้น
        currentSpeed = speed;

        // หา MQTTCore ใน Scene
        var mqtt = FindObjectOfType<MQTTCore>();
        yield return new WaitForSeconds(1f); // รอให้ Core เชื่อมต่อ

        // Subscribe topic
        mqtt.Subscribe(topic);
        MQTTCore.OnMessageReceived += EnqueueMessage;

        Debug.Log($"⚙️ Rotator ready on topic: {topic}");
    }

    void OnDestroy()
    {
        MQTTCore.OnMessageReceived -= EnqueueMessage;
    }

    // ✅ ดักข้อความจาก MQTT (Thread อื่น)
    void EnqueueMessage(string t, string msg)
    {
        lock (queueLock)
        {
            messageQueue.Enqueue((t, msg));
        }
    }

    /* ──────────────────────────────────────────────
     *  🔄 SECTION 4 : MAIN UPDATE LOOP
     * ────────────────────────────────────────────── */
    void Update()
    {
        // อ่าน queue ของ MQTT Message
        while (true)
        {
            (string t, string msg) item;
            lock (queueLock)
            {
                if (messageQueue.Count == 0) break;
                item = messageQueue.Dequeue();
            }

            if (item.t == topic)
            {
                HandleMessage(item.msg);
            }
        }

        // ถ้ากำลังหมุน → หมุนตามทิศทางและความเร็วปัจจุบัน
        if (isRotating)
        {
            transform.Rotate(rotationAxis * currentSpeed * direction * Time.deltaTime);
        }
    }

    /* ──────────────────────────────────────────────
     *  🧠 SECTION 5 : MESSAGE INTERPRETATION
     * ────────────────────────────────────────────── */
    void HandleMessage(string msg)
    {
        msg = msg.ToUpper().Trim();
        Debug.Log($"📥 MQTT Rotator got message: {msg}");

        // ── Control Commands ───────────────────────
        if (msg == "CW")
        {
            direction = 1;
            isRotating = true;
            Debug.Log($"↻ Rotate Clockwise @ {currentSpeed}°/s");
        }
        else if (msg == "CCW")
        {
            direction = -1;
            isRotating = true;
            Debug.Log($"↺ Rotate Counter-Clockwise @ {currentSpeed}°/s");
        }
        else if (msg == "OFF")
        {
            isRotating = false;
            Debug.Log("⏹ Rotation stopped");
        }

        // ── Speed Adjustment ───────────────────────
        else if (msg.StartsWith("SPEED:"))
        {
            string value = msg.Split(':')[1];
            if (float.TryParse(value, out float newSpeed))
            {
                currentSpeed = newSpeed;
                Debug.Log($"⚡ Speed changed to {currentSpeed}°/s");
            }
        }
    }
}
