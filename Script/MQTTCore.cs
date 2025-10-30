using UnityEngine;
using System;
using System.Text;
using System.Collections;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

public class MQTTCore : MonoBehaviour
{
    [Header("MQTT Settings")]
    public string broker = "broker.emqx.io";
    public int port = 1883;
    public bool autoReconnect = true;
    public float reconnectDelay = 3f; // วินาที

    private MqttClient client;
    private string clientId;
    private bool isConnecting = false;

    // ✅ Broadcast event ให้ Module อื่นรับข้อความได้
    public static event Action<string, string> OnMessageReceived;

    void Start()
    {
        Connect();
    }

    void Connect()
    {
        if (isConnecting) return;
        isConnecting = true;

        try
        {
            // ✅ สร้าง clientId แบบสุ่ม (กันชนหลาย instance)
            clientId = "unity_" + Guid.NewGuid().ToString("N").Substring(0, 8);

            // ✅ ใช้ constructor แบบเดิม (เวอร์ชันเก่ารองรับแน่นอน)
            client = new MqttClient(broker);

            client.MqttMsgPublishReceived += OnMessage;
            client.ConnectionClosed += OnConnectionClosed;

            client.Connect(clientId);
            Debug.Log($"✅ MQTT Connected as {clientId}");
            isConnecting = false;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"⚠️ MQTT Connect failed: {ex.Message}");
            isConnecting = false;

            if (autoReconnect)
                StartCoroutine(Reconnect());
        }
    }

    void OnConnectionClosed(object sender, EventArgs e)
    {
        Debug.LogWarning("❌ MQTT Disconnected unexpectedly!");
        if (autoReconnect)
            StartCoroutine(Reconnect());
    }

    IEnumerator Reconnect()
    {
        yield return new WaitForSeconds(reconnectDelay);
        Debug.Log("🔄 Reconnecting to MQTT...");
        Connect();
    }

    void OnMessage(object sender, MqttMsgPublishEventArgs e)
    {
        string topic = e.Topic;
        string message = Encoding.UTF8.GetString(e.Message);

        Debug.Log($"📥 MQTT Message: [{topic}] {message}");
        OnMessageReceived?.Invoke(topic, message);
    }

    public void Subscribe(string topic)
    {
        if (client != null && client.IsConnected)
        {
            client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
            Debug.Log($"📡 Subscribed: {topic}");
        }
        else
        {
            Debug.LogWarning("❌ MQTT not connected (Subscribe failed)");
        }
    }

    public void Publish(string topic, string message)
    {
        if (client != null && client.IsConnected)
        {
            client.Publish(topic, Encoding.UTF8.GetBytes(message));
            Debug.Log($"➡️ Publish: [{topic}] {message}");
        }
        else
        {
            Debug.LogWarning("❌ MQTT not connected (Publish failed)");
        }
    }

    void OnApplicationQuit()
    {
        if (client != null && client.IsConnected)
        {
            client.Disconnect();
            Debug.Log("🔌 MQTT Disconnected");
        }
    }
}
