using UnityEngine;

public class DataPosition : MonoBehaviour
{
    public DataCore dataCore;

    public float minValue = 0f;
    public float maxValue = 100f;

    public Transform startPoint;
    public Transform endPoint;

    void OnEnable()
    {
        if (dataCore != null)
            dataCore.OnFloatChanged += UpdatePosition;
    }

    void OnDisable()
    {
        if (dataCore != null)
            dataCore.OnFloatChanged -= UpdatePosition;
    }

    void UpdatePosition(float value)
    {
        float t = Mathf.InverseLerp(
            minValue,
            maxValue,
            value
        );

        transform.position = Vector3.Lerp(
            startPoint.position,
            endPoint.position,
            t
        );
    }
}