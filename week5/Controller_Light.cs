using UnityEngine;

public class Controller_Light : MonoBehaviour
{
    [Header("Data")]
    public DataCore dataCore;

    [Header("Lamp Components")]
    public Light lampLight;
    public Renderer lampRenderer;

    private Color onColor = Color.yellow;
    private Color offColor = Color.gray;

    private float onIntensity = 3f;
    private float offIntensity = 0.2f;


    void Start()
    {
        if (lampRenderer == null)
        {
            lampRenderer = GetComponent<Renderer>();
        }
    }


    void OnEnable()
    {
        if (dataCore != null)
        {
            dataCore.OnBoolChanged += SetLampState;
        }
    }


    void OnDisable()
    {
        if (dataCore != null)
        {
            dataCore.OnBoolChanged -= SetLampState;
        }
    }


    void SetLampState(bool state)
    {
        if (state)
        {
            SetLamp(onColor, onIntensity);
        }
        else
        {
            SetLamp(offColor, offIntensity);
        }
    }


    void SetLamp(Color color, float intensity)
    {
        if (lampRenderer != null)
        {
            lampRenderer.material.color = color;
        }

        if (lampLight != null)
        {
            lampLight.color = color;
            lampLight.intensity = intensity;
        }
    }
}