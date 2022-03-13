using UnityEngine;
using TMPro;

public class Portal : MonoBehaviour {

    public string LabelToShow = "none";
    public string SceneToLoad = "none";
    public string CheckpointToSpawn = "none";
    public Color Color;

    [Header("Refs")]
    public TextMeshProUGUI label;
    public ParticleSystem particles;
    public Renderer portalRender;

    public LogicController logic;

    private void Start() 
    {
        label.text = LabelToShow;
        portalRender.material.SetColor("_Color", Color);
        portalRender.material.SetColor("_EmissionColor", Color);
        var colorOverLife = particles.colorOverLifetime;
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color, 0f),
                new GradientColorKey(Color, 1f),
            },
            colorOverLife.color.gradient.alphaKeys
        );
        colorOverLife.color = g;
    }
    
    private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player"))
        {
            other.attachedRigidbody.velocity = Vector3.zero;
            logic.SetSpawnCheckpoint(CheckpointToSpawn);
            logic.ChangeScene(SceneToLoad);
        }     
    }

}