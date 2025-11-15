using UnityEngine;

public class ButtonMQTTSender : MonoBehaviour
{
    public MQTTCore mqtt;
    public string topic = "idt/button";
    public string text_in;   // กรอกข้อความตรง Inspector ได้เลย

    public void Send()
    {
        if (mqtt == null)
        {
            Debug.LogWarning("MQTTCore not assigned");
            return;
        }

        mqtt.Publish(topic, text_in);

        Debug.Log($"MQTT Send [{topic}] {text_in}");
    }
}
