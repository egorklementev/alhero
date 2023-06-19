using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LabPortal : MonoBehaviour {

    public Color Color;

    [Header("Refs")]
    public ParticleSystem particles;
    public Renderer portalRender;

    public LogicController logic;

    private int[] _locationUnclockSteps = { 0, 5, 15, 35, 75 };
    private string[] _locations = { "ForestScene", "DesertScene", "WinterForestScene", "HellScene", "HeavenScene" };

    private void Start() 
    {
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
            logic.SetSpawnCheckpoint("none");

            List<string> possibleLocations = new List<string>();
            for (int i = 0; i < _locationUnclockSteps.Length; i++)
            {
                if (DataController.genData.potionsCooked >= _locationUnclockSteps[i])
                {
                    possibleLocations.Add(_locations[i]);
                }
            }

            logic.ChangeScene(possibleLocations[Random.Range(0, possibleLocations.Count)], false);
        }     
    }

}