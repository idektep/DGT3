using UnityEngine;

public class DataVibration : MonoBehaviour
{
    public DataCore dataCore;

    public float minValue = 0f;
    public float maxValue = 1f;

    public float maxAmplitude = 0.05f;
    public float frequency = 25f;

    private float amplitude = 0f;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.localPosition;
    }

    void OnEnable()
    {
        if (dataCore != null)
            dataCore.OnFloatChanged += SetVibration;
    }

    void OnDisable()
    {
        if (dataCore != null)
            dataCore.OnFloatChanged -= SetVibration;
    }

    void SetVibration(float value)
    {
        float t = Mathf.InverseLerp(
            minValue,
            maxValue,
            value
        );

        amplitude = Mathf.Lerp(
            0f,
            maxAmplitude,
            t
        );
    }

    void Update()
    {
        float x =
            Mathf.Sin(Time.time * frequency)
            * amplitude;

        float y =
            Mathf.Sin(Time.time * frequency * 1.3f)
            * amplitude;

        transform.localPosition =
            startPosition +
            new Vector3(x, y, 0f);
    }
}