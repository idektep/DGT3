using UnityEngine;

public class DataConditionColor : MonoBehaviour
{
    [Header("Data")]
    public DataCore dataCore;

    [Header("Condition")]
    public float lowLimit = 30f;
    public float highLimit = 40f;

    [Header("Color")]
    public Color lowColor = Color.blue;
    public Color normalColor = Color.green;
    public Color highColor = Color.red;

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
        if (value < lowLimit)
        {
            rend.material.color =
                lowColor;
        }
        else if (value <= highLimit)
        {
            rend.material.color =
                normalColor;
        }
        else
        {
            rend.material.color =
                highColor;
        }
    }
}