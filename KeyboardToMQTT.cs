using UnityEngine;
using UnityEngine.InputSystem;

public class KeyboardToMQTT : MonoBehaviour
{
    public InputActionAsset actions;

    private InputAction forward;
    private InputAction back;
    private InputAction left;
    private InputAction right;
    private InputAction stop;
    public MQTTCore mqtt;
    public string topic = "robot/cmd";

    private string lastCmd = ""; // กัน spam

    void OnEnable()
    {
        forward = actions.FindAction("forwardAction");
        back    = actions.FindAction("backwardAction");
        left    = actions.FindAction("leftAction");
        right   = actions.FindAction("rightAction");

        forward.Enable();
        back.Enable();
        left.Enable();
        right.Enable();
    }

    void OnDisable()
    {
        forward.Disable();
        back.Disable();
        left.Disable();
        right.Disable();
    }

    void Update()
    {
        string cmd = "";

        // หากมีปุ่มใดกดอยู่ ให้ส่งคำสั่งนั้น
        if (forward.IsPressed())
            cmd = "move:forward";
        else if (back.IsPressed())
            cmd = "move:backward";
        else if (left.IsPressed())
            cmd = "move:left";
        else if (right.IsPressed())
            cmd = "move:right";
        else
            cmd = "move:stop";  // ไม่มีปุ่มไหนกด = STOP

        // ส่งเฉพาะตอนคำสั่งเปลี่ยน
        if (cmd != lastCmd)
        {
            mqtt.Publish(topic, cmd);
            lastCmd = cmd;
            Debug.Log("MQTT → " + cmd);
        }
    }
}
