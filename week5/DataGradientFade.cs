using UnityEngine;

public class DataGradientFade : MonoBehaviour
{
    [Header("Data")]
    public DataCore dataCore;

    [Header("Range")]
    public float minValue = 25f;
    public float maxValue = 50f;

    [Header("Gradient")]
    public Gradient colorGradient;

    [Header("Fade")]
    public float fadeSpeed = 2f;

    private Renderer rend;

    private Color targetColor;


    void Start()
    {
        rend = GetComponent<Renderer>();

        targetColor =
            rend.material.color;
    }


    void OnEnable()
    {
        if (dataCore != null)
        {
            dataCore.OnFloatChanged += SetTargetColor;
        }
    }


    void OnDisable()
    {
        if (dataCore != null)
        {
            dataCore.OnFloatChanged -= SetTargetColor;
        }
    }


    void SetTargetColor(float value)
    {
        float t =
            Mathf.InverseLerp(
                minValue,
                maxValue,
                value
            );

        targetColor =
            colorGradient.Evaluate(t);
    }


    void Update()
    {
        rend.material.color =
            Color.Lerp(
                rend.material.color,
                targetColor,
                fadeSpeed *
                Time.deltaTime
            );
    }
}