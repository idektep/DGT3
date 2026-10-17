using UnityEngine;

public class DataMaterialSwitch : MonoBehaviour
{
    public DataCore dataCore;

    public Material metalMaterial;
    public Material rubberMaterial;
    public Material wetMaterial;

    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void OnEnable()
    {
        if (dataCore != null)
            dataCore.OnStringChanged += ChangeMaterial;
    }

    void OnDisable()
    {
        if (dataCore != null)
            dataCore.OnStringChanged -= ChangeMaterial;
    }

    void ChangeMaterial(string value)
    {
        string type =
            value.Trim().ToUpper();

        if (type == "METAL")
        {
            rend.material = metalMaterial;
        }
        else if (type == "RUBBER")
        {
            rend.material = rubberMaterial;
        }
        else if (type == "WET")
        {
            rend.material = wetMaterial;
        }
    }
}