using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Controller_Light : MonoBehaviour
{
    [Header("MQTT Settings")]
    public string topic = "idt/led/led1";

    [Header("Lamp Components")]
    public Light lampLight;
    public Renderer lampRenderer;

    private Queue<(string, string)> messageQueue = new Queue<(string, string)>();
    private object queueLock = new object();

    IEnumerator Start()
    {
        var mqtt = FindObjectOfType<MQTTCore>();
        yield return new WaitForSeconds(1f);

        mqtt.Subscribe(topic);
        MQTTCore.OnMessageReceived += EnqueueMessage;

        if (lampRenderer == null)
            lampRenderer = GetComponent<Renderer>();
    }

    void OnDestroy()
    {
        MQTTCore.OnMessageReceived -= EnqueueMessage;
    }

    // ✅ รับข้อความจาก MQTT (Thread อื่น)
    void EnqueueMessage(string t, string msg)
    {
        lock (queueLock)
        {
            messageQueue.Enqueue((t, msg));
        }
    }

    // ✅ ทำงานบน Main Thread
    void Update()
    {
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
    }

    void HandleMessage(string msg)
    {
        Debug.Log($"📥 HandleMessage: msg={msg}");

        if (msg.ToUpper() == "ON")
        {
            SetLampState(Color.yellow, 3f);
            Debug.Log("💡 Lamp ON");
        }
        else if (msg.ToUpper() == "OFF")
        {
            SetLampState(Color.gray, 0.2f);
            Debug.Log("💡 Lamp OFF");
        }
    }

    void SetLampState(Color color, float intensity)
    {
        if (lampRenderer != null)
            lampRenderer.material.color = color;

        if (lampLight != null)
        {
            lampLight.color = color;
            lampLight.intensity = intensity;
        }
    }
}
