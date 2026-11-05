using UnityEngine;


public class MQTTObjectClick :
    MonoBehaviour
{
    // ========================================================
    // MQTT
    // ========================================================

    [Header("MQTT")]

    public MQTTCore mqtt;


    // ========================================================
    // STATUS DATA
    // ========================================================

    [Header("Status")]

    public DataCore statusData;


    // ========================================================
    // COMMAND
    // ========================================================

    [Header("Command")]

    public string topic =
        "PREFIX/led/led1";

    public string onMessage =
        "on";

    public string offMessage =
        "off";


    // ========================================================
    // CURRENT ACTUAL STATE
    // ========================================================

    private bool currentState =
        false;


    // ========================================================
    // ENABLE
    // ========================================================

    void OnEnable()
    {
        if (
            statusData != null
        )
        {
            statusData
                .OnBoolChanged +=
                UpdateActualState;
        }
    }


    // ========================================================
    // DISABLE
    // ========================================================

    void OnDisable()
    {
        if (
            statusData != null
        )
        {
            statusData
                .OnBoolChanged -=
                UpdateActualState;
        }
    }


    // ========================================================
    // STATUS UPDATE
    // ========================================================

    void UpdateActualState(
        bool state
    )
    {
        currentState =
            state;


        Debug.Log(
            "Actual Lamp State = "
            + currentState
        );
    }


    // ========================================================
    // CLICK
    // ========================================================

    void OnMouseDown()
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


        // ถ้าปัจจุบัน ON
        // คลิกครั้งนี้จะ Request OFF

        if (
            currentState
        )
        {
            mqtt.Publish(
                topic,
                offMessage
            );


            Debug.Log(
                "Object Click → Request OFF"
            );
        }

        // ถ้าปัจจุบัน OFF
        // คลิกครั้งนี้จะ Request ON

        else
        {
            mqtt.Publish(
                topic,
                onMessage
            );


            Debug.Log(
                "Object Click → Request ON"
            );
        }
    }
}