using UnityEngine;
using UnityEngine.InputSystem;

public class RTSCameraController : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionAsset inputActions;

    private InputAction panAction;
    private InputAction rotateAction;
    private InputAction zoomAction;

    [Header("Pan Settings")]
    public float panSpeed = 10f;

    [Header("Rotate Settings")]
    public float rotateSpeed = 5f;
    private float pitch;
    public float minPitch = 10f;
    public float maxPitch = 80f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 20f;
    public float minZoom = 5f;
    public float maxZoom = 100f;

    void OnEnable()
    {
        panAction = inputActions.FindAction("Pan");
        rotateAction = inputActions.FindAction("Rotate");
        zoomAction = inputActions.FindAction("Zoom");

        panAction?.Enable();
        rotateAction?.Enable();
        zoomAction?.Enable();
    }

    void OnDisable()
    {
        panAction?.Disable();
        rotateAction?.Disable();
        zoomAction?.Disable();
    }

    void Start()
    {
        // ไม่เปลี่ยนตำแหน่ง — ใช้ตำแหน่งเดิมใน Scene
        pitch = transform.eulerAngles.x; // อ่านมุม Pitch ปัจจุบัน
    }

    void Update()
    {
        HandlePan();
        HandleRotate();
        HandleZoom();
    }

    void HandlePan()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 delta = panAction.ReadValue<Vector2>();
            Vector3 right = transform.right;
            Vector3 forward = transform.forward;

            right.y = 0f;
            forward.y = 0f;

            right.Normalize();
            forward.Normalize();

            Vector3 move = (-delta.x * right + -delta.y * forward) * panSpeed * Time.deltaTime;
            transform.position += move;
        }
    }

    void HandleRotate()
    {
        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 delta = rotateAction.ReadValue<Vector2>();

            // หมุนรอบแกน Y
            transform.Rotate(Vector3.up, delta.x * rotateSpeed * Time.deltaTime, Space.World);

            // หมุน Pitch
            pitch -= delta.y * rotateSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            Vector3 currentEuler = transform.eulerAngles;
            transform.eulerAngles = new Vector3(pitch, currentEuler.y, 0f);
        }
    }

    void HandleZoom()
    {
        Vector2 scroll = zoomAction.ReadValue<Vector2>();
        float scrollAmount = scroll.y * zoomSpeed * Time.deltaTime;

        Vector3 direction = transform.forward;
        Vector3 newPosition = transform.position + direction * scrollAmount;

        float distance = Vector3.Distance(newPosition, Vector3.zero); // หรือใช้ตำแหน่งเป้าหมายอื่น
        if (distance >= minZoom && distance <= maxZoom)
        {
            transform.position = newPosition;
        }
    }
}
