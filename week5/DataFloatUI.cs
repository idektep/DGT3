using UnityEngine;
using TMPro;

public class DataFloatUI : MonoBehaviour
{
    [Header("Data")]
    public DataCore dataCore;

    [Header("UI")]
    public TMP_Text valueText;

    [Header("Display")]
    public string label = "Temperature";
    public string unit = "°C";
    public int decimalPoint = 1;


    void OnEnable()
    {
        if (dataCore != null)
        {
            dataCore.OnFloatChanged += UpdateUI;
        }
    }


    void OnDisable()
    {
        if (dataCore != null)
        {
            dataCore.OnFloatChanged -= UpdateUI;
        }
    }


    void UpdateUI(float value)
    {
        valueText.text =
            label +
            " : " +
            value.ToString(
                "F" + decimalPoint
            ) +
            " " +
            unit;
    }
}