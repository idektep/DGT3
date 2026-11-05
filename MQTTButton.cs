using UnityEngine;


public class MQTTButton :
    MonoBehaviour
{
    // ========================================================
    // MQTT
    // ========================================================

    [Header("MQTT")]

    public MQTTCore mqtt;


    // ========================================================
    // COMMAND
    // ========================================================

    [Header("Command")]

    public string topic =
        "<PREFIX>/led/led1";

    public string message =
        "on";


    // ========================================================
    // SEND
    // ========================================================

    public void SendCommand()
    {
        if (
            mqtt == null
        )
        {
            Debug.LogWarning(
                "MQTTCore not assigned"
            );

            return;
        }


        mqtt.Publish(
            topic,
            message
        );


        Debug.Log(
            "Button Publish : "
            + topic
            + " = "
            + message
        );
    }
}