using UnityEngine;
using UnityEngine.UI;

public class SliderJointMQTT : MonoBehaviour
{
    [Header("MQTT")]
    public MQTTCore mqtt;            // ต้องลากเข้า Inspector
    public string topic = "dgt_arm2/arm/control";

    [Header("Joint Settings")]
    public string jointName = "j1";  // เช่น j1, j2, j3...
    public Text valueText;           // Text ที่โชว์ค่าบนจอ

    private Slider slider;
    private float lastValue = -999;

    void Start()
    {
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnSliderChanged(float value)
    {
        int angle = Mathf.RoundToInt(value);

        // แสดงค่าใน Text UI
        if (valueText != null)
            valueText.text = angle.ToString();

        // กัน spam ถ้าค่าเดิมตรงกันไม่ส่ง
        if (Mathf.Abs(angle - lastValue) < 1f)
            return;

        lastValue = angle;

        // สร้างข้อความ MQTT
        string msg = $"movejoint:{jointName},{angle}";
        
        // ส่ง
        mqtt.Publish(topic, msg);

        Debug.Log($"➡️ MQTT: {msg}");
    }
}
