using UnityEngine;

public class DataGradientColor : MonoBehaviour
{
    [Header("Data")]
    public DataCore dataCore;

    [Header("Range")]
    public float minValue = 25f;
    public float maxValue = 50f;

    [Header("Gradient")]
    public Gradient colorGradient;

    private Renderer rend;


    void Start()
    {
        rend = GetComponent<Renderer>();
    }


    void OnEnable()
    {
        if (dataCore != null)
        {
            dataCore.OnFloatChanged += UpdateColor;
        }
    }


    void OnDisable()
    {
        if (dataCore != null)
        {
            dataCore.OnFloatChanged -= UpdateColor;
        }
    }


    void UpdateColor(float value)
    {
        float t =
            Mathf.InverseLerp(
                minValue,
                maxValue,
                value
            );

        Color newColor =
            colorGradient.Evaluate(t);

        rend.material.color =
            newColor;
    }
}