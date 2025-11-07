using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Controller_Movement : MonoBehaviour
{
    // ───────────────────────────────
    // MQTT CONFIG
    // ───────────────────────────────
    [Header("MQTT Settings")]
    public string topic = "idt/move/1";

    // ───────────────────────────────
    // MOVE CONTROL
    // ───────────────────────────────
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    private bool isMoving = false;
    private bool moveBackward = false;
    private bool moveUp = false;
    private bool moveDown = false;
    private bool moveLeft = false;
    private bool moveRight = false;

    // ───────────────────────────────
    // ROTATION CONTROL
    // ───────────────────────────────
    [Header("Rotation Settings")]
    public float rotateSpeed = 90f;
    private Vector3 rotateAxis = Vector3.zero;
    private bool isRotating = false;

    // ───────────────────────────────
    // ANGLE (TARGET ROTATION)
    // ───────────────────────────────
    [Header("Angle Settings")]
    public float angleSpeed = 90f;
    private Vector3 targetEuler;
    private bool isAngling = false;

    // ───────────────────────────────
    // MQTT QUEUE
    // ───────────────────────────────
    private Queue<(string, string)> messageQueue = new Queue<(string, string)>();
    private object queueLock = new object();

    // ───────────────────────────────
    // INITIALIZATION
    // ───────────────────────────────
    IEnumerator Start()
    {
        var mqtt = FindObjectOfType<MQTTCore>();
        yield return new WaitForSeconds(1f);

        mqtt.Subscribe(topic);
        MQTTCore.OnMessageReceived += EnqueueMessage;

        Debug.Log($"✅ Controller_Movement started (Local Follow Heading). Subscribed to: {topic}");
    }

    void OnDestroy()
    {
        MQTTCore.OnMessageReceived -= EnqueueMessage;
    }

    void EnqueueMessage(string t, string msg)
    {
        lock (queueLock)
        {
            messageQueue.Enqueue((t, msg));
        }
    }

    // ───────────────────────────────
    // MAIN LOOP
    // ───────────────────────────────
    void Update()
    {
        // อ่านข้อความจาก MQTT Queue
        while (true)
        {
            (string t, string msg) item;
            lock (queueLock)
            {
                if (messageQueue.Count == 0) break;
                item = messageQueue.Dequeue();
            }

            if (item.t == topic)
                HandleMessage(item.msg);
        }

        // ── MOVE ─────────────────────
        Vector3 moveVector = Vector3.zero;

        if (isMoving)        moveVector += transform.forward;
        if (moveBackward)    moveVector -= transform.forward;
        if (moveUp)          moveVector += transform.up;
        if (moveDown)        moveVector -= transform.up;
        if (moveRight)       moveVector += transform.right;
        if (moveLeft)        moveVector -= transform.right;

        if (moveVector != Vector3.zero)
        {
            transform.position += moveVector.normalized * moveSpeed * Time.deltaTime;
        }

        // ── ROTATE ───────────────────
        if (isRotating && rotateAxis != Vector3.zero)
        {
            transform.Rotate(rotateAxis.normalized * rotateSpeed * Time.deltaTime, Space.Self);
        }

        // ── ANGLE (Servo-like) ───────
        if (isAngling)
        {
            Vector3 current = transform.eulerAngles;
            Vector3 next = new Vector3(
                Mathf.MoveTowardsAngle(current.x, targetEuler.x, angleSpeed * Time.deltaTime),
                Mathf.MoveTowardsAngle(current.y, targetEuler.y, angleSpeed * Time.deltaTime),
                Mathf.MoveTowardsAngle(current.z, targetEuler.z, angleSpeed * Time.deltaTime)
            );

            transform.eulerAngles = next;

            if (Vector3.Distance(next, targetEuler) < 0.1f)
            {
                Debug.Log($"🎯 Reached target angle: {targetEuler}");
                isAngling = false;
            }
        }
    }

    // ───────────────────────────────
    // MESSAGE HANDLER
    // ───────────────────────────────
    void HandleMessage(string msg)
    {
        msg = msg.ToUpper().Trim();
        Debug.Log($"📩 MQTT Received: {msg}");

        // ───────────────────────────────
        // MOVE (Local)
        // ───────────────────────────────
        if (msg == "MOVE:FORWARD") { isMoving = true; moveBackward = false; Debug.Log("✈️ Move Forward"); }
        else if (msg == "MOVE:BACK") { moveBackward = true; isMoving = false; Debug.Log("🔙 Move Backward"); }
        else if (msg == "MOVE:UP") { moveUp = true; moveDown = false; Debug.Log("🛫 Move Up"); }
        else if (msg == "MOVE:DOWN") { moveDown = true; moveUp = false; Debug.Log("🛬 Move Down"); }
        else if (msg == "MOVE:LEFT") { moveLeft = true; moveRight = false; Debug.Log("↖️ Move Left"); }
        else if (msg == "MOVE:RIGHT") { moveRight = true; moveLeft = false; Debug.Log("↗️ Move Right"); }

        else if (msg == "STOP")
        {
            isMoving = moveBackward = moveUp = moveDown = moveLeft = moveRight = false;
            Debug.Log("⏹ Stop Moving");
        }

        // ───────────────────────────────
        // ROTATION (Local Axes)
        // ───────────────────────────────
        else if (msg == "ROTATE:X") { rotateAxis = Vector3.right; isRotating = true; Debug.Log("🔁 Roll +X"); }
        else if (msg == "ROTATE:-X") { rotateAxis = Vector3.left; isRotating = true; Debug.Log("🔁 Roll -X"); }
        else if (msg == "ROTATE:Y") { rotateAxis = Vector3.up; isRotating = true; Debug.Log("🔁 Yaw +Y"); }
        else if (msg == "ROTATE:-Y") { rotateAxis = Vector3.down; isRotating = true; Debug.Log("🔁 Yaw -Y"); }
        else if (msg == "ROTATE:Z") { rotateAxis = Vector3.forward; isRotating = true; Debug.Log("🔁 Pitch +Z"); }
        else if (msg == "ROTATE:-Z") { rotateAxis = Vector3.back; isRotating = true; Debug.Log("🔁 Pitch -Z"); }

        else if (msg == "STOPROT")
        {
            isRotating = false;
            Debug.Log("⏹ Stop Rotation");
        }

        // ───────────────────────────────
        // ANGLE (TARGET ROTATION)
        // ───────────────────────────────
        else if (msg.StartsWith("ANGLE:"))
        {
            string[] parts = msg.Split(':');
            if (parts.Length == 3)
            {
                string axis = parts[1];
                if (float.TryParse(parts[2], out float target))
                {
                    Vector3 euler = transform.eulerAngles;
                    if (axis == "X") euler.x = target;
                    else if (axis == "Y") euler.y = target;
                    else if (axis == "Z") euler.z = target;

                    targetEuler = euler;
                    isAngling = true;
                    Debug.Log($"🎯 Target Angle set: {axis} = {target}");
                }
            }
        }

        // ───────────────────────────────
        // SPEED CONTROLS
        // ───────────────────────────────
        else if (msg.StartsWith("SPEED:"))
        {
            string value = msg.Split(':')[1];
            if (float.TryParse(value, out float newSpeed))
            {
                moveSpeed = newSpeed;
                Debug.Log($"⚡ Move Speed = {moveSpeed}");
            }
        }
        else if (msg.StartsWith("RSPEED:"))
        {
            string value = msg.Split(':')[1];
            if (float.TryParse(value, out float newRSpeed))
            {
                rotateSpeed = newRSpeed;
                Debug.Log($"⚙️ Rotate Speed = {rotateSpeed}");
            }
        }
        else if (msg.StartsWith("ASPEED:"))
        {
            string value = msg.Split(':')[1];
            if (float.TryParse(value, out float newASpeed))
            {
                angleSpeed = newASpeed;
                Debug.Log($"🎚 Angle Speed = {angleSpeed}");
            }
        }
    }
}
