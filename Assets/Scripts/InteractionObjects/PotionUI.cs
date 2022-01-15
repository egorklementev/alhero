using System.Collections;
using UnityEngine;

public class PotionUI : ItemUI
{
    public Potion potionData;

    private Renderer render;

    protected override void OnEnable()
    {
        base.OnEnable();
        render = GetComponent<Renderer>();
        UpdateColor();
    }

    void UpdateColor()
    {
        render.materials[arcanumMaterialIndex].SetColor("_Color", potionData.GetColor());
    }
}
