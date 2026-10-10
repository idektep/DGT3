using UnityEngine;
using System;

public class DataCore : MonoBehaviour
{
    public enum PayloadType
    {
        Float,
        Int,
        String,
        Bool
    }

    [Header("MQTT")]
    public MQTTCore mqtt;

    [Header("Data Setting")]
    public string topic = "idt/sensor/temp";
    public PayloadType payloadType = PayloadType.Float;

    [Header("Current Data")]
    public float floatValue;
    public int intValue;
    public string stringValue;
    public bool boolValue;

    public event Action<float> OnFloatChanged;
    public event Action<int> OnIntChanged;
    public event Action<string> OnStringChanged;
    public event Action<bool> OnBoolChanged;


    void OnEnable()
    {
        MQTTCore.OnConnected += OnMQTTConnected;
        MQTTCore.OnMessageReceived += OnMQTTMessage;
    }


    void OnDisable()
    {
        MQTTCore.OnConnected -= OnMQTTConnected;
        MQTTCore.OnMessageReceived -= OnMQTTMessage;
    }


    void OnMQTTConnected()
    {
        if (mqtt != null)
        {
            mqtt.Subscribe(topic);
        }
    }


    void OnMQTTMessage(string receiveTopic, string payload)
    {
        if (receiveTopic != topic)
            return;

        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            ParsePayload(payload);
        });
    }


    void ParsePayload(string payload)
    {
        switch (payloadType)
        {
            // =========================
            // FLOAT
            // =========================
            case PayloadType.Float:

                if (float.TryParse(payload, out float f))
                {
                    floatValue = f;

                    OnFloatChanged?.Invoke(floatValue);

                    Debug.Log(
                        topic +
                        " = " +
                        floatValue
                    );
                }

                break;


            // =========================
            // INT
            // =========================
            case PayloadType.Int:

                if (int.TryParse(payload, out int i))
                {
                    intValue = i;

                    OnIntChanged?.Invoke(intValue);

                    Debug.Log(
                        topic +
                        " = " +
                        intValue
                    );
                }

                break;


            // =========================
            // STRING
            // =========================
            case PayloadType.String:

                stringValue = payload;

                OnStringChanged?.Invoke(stringValue);

                Debug.Log(
                    topic +
                    " = " +
                    stringValue
                );

                break;


            // =========================
            // BOOL
            // =========================
            case PayloadType.Bool:

                string data =
                    payload.Trim().ToLower();

                if (
                    data == "true" ||
                    data == "1" ||
                    data == "on"
                )
                {
                    boolValue = true;
                }
                else
                {
                    boolValue = false;
                }

                OnBoolChanged?.Invoke(boolValue);

                Debug.Log(
                    topic +
                    " = " +
                    boolValue
                );

                break;
        }
    }
}