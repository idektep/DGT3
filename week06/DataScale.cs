using UnityEngine;

public class DataScale : MonoBehaviour
{
    [Header("Data")]
    public DataCore dataCore;

    [Header("Input Range")]
    public float minValue = 0f;
    public float maxValue = 100f;

    [Header("Scale Range")]
    public Vector3 minScale =
        new Vector3(1f, 0.01f, 1f);

    public Vector3 maxScale =
        new Vector3(1f, 1f, 1f);


    void OnEnable()
    {
        if (dataCore != null)
        {
            dataCore.OnFloatChanged += UpdateScale;
        }
    }


    void OnDisable()
    {
        if (dataCore != null)
        {
            dataCore.OnFloatChanged -= UpdateScale;
        }
    }


    void UpdateScale(float value)
    {
        float t =
            Mathf.InverseLerp(
                minValue,
                maxValue,
                value
            );

        transform.localScale =
            Vector3.Lerp(
                minScale,
                maxScale,
                t
            );
    }
}