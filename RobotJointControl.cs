using UnityEngine;
using UnityEngine.UI;

public class RobotJointControl : MonoBehaviour
{
    // =========================================================
    // MQTT
    // =========================================================

    [Header("MQTT")]
    public MQTTCore mqtt;

    public string topic =
        "dgt_arm3/arm/control";


    // =========================================================
    // JOINT
    // =========================================================

    [Header("Joint Settings")]
    public string jointName =
        "j1";


    // =========================================================
    // UI
    // =========================================================

    [Header("UI")]
    public Text valueText;


    private Slider slider;

    private int lastAngle =
        int.MinValue;


    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        slider =
            GetComponent<Slider>();


        if (slider == null)
        {
            Debug.LogError(
                "SliderJointMQTT requires Slider component"
            );

            return;
        }


        slider.onValueChanged.AddListener(
            OnSliderChanged
        );


        UpdateValueText(
            slider.value
        );
    }


    // =========================================================
    // DESTROY
    // =========================================================

    void OnDestroy()
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(
                OnSliderChanged
            );
        }
    }


    // =========================================================
    // SLIDER
    // =========================================================

    void OnSliderChanged(float value)
    {
        int angle =
            Mathf.RoundToInt(value);


        // -------------------------
        // Update UI
        // -------------------------

        UpdateValueText(angle);


        // -------------------------
        // Prevent duplicate command
        // -------------------------

        if (angle == lastAngle)
            return;


        lastAngle =
            angle;


        // -------------------------
        // Check MQTT
        // -------------------------

        if (mqtt == null)
        {
            Debug.LogWarning(
                "MQTTCore not assigned"
            );

            return;
        }


        // -------------------------
        // Build command
        // -------------------------

        string message =
            "movejoint:"
            + jointName
            + ","
            + angle;


        // -------------------------
        // Publish
        // -------------------------

        mqtt.Publish(
            topic,
            message
        );


        Debug.Log(
            "Robot Command : "
            + message
        );
    }


    // =========================================================
    // UI TEXT
    // =========================================================

    void UpdateValueText(float value)
    {
        if (valueText == null)
            return;


        int angle =
            Mathf.RoundToInt(value);


        valueText.text =
            angle.ToString();
    }
}