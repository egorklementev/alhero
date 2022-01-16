using UnityEngine;

public class PotionWorld : ItemWorld
{
    public Potion potionData;

    private Renderer render;

    protected override void OnEnable() {
        base.OnEnable();
        render = GetComponentInChildren<Renderer>();
        UpdateColor();
    }

    public void UpdateColor() {
        render.materials[arcanumMaterialIndex].SetColor("_Color", GetColor());
    }

    public Color GetColor() {
        return potionData.GetColor();
    }

}
