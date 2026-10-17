using UnityEngine;

public class DataRotation : MonoBehaviour
{
    public DataCore dataCore;

    public Vector3 rotationAxis = Vector3.forward;

    private float currentRPM = 0f;

    void OnEnable()
    {
        if (dataCore != null)
            dataCore.OnFloatChanged += SetRPM;
    }

    void OnDisable()
    {
        if (dataCore != null)
            dataCore.OnFloatChanged -= SetRPM;
    }

    void SetRPM(float value)
    {
        currentRPM = value;
    }

    void Update()
    {
        float degreePerSecond =
            currentRPM * 6f;

        transform.Rotate(
            rotationAxis,
            degreePerSecond * Time.deltaTime,
            Space.Self
        );
    }
}