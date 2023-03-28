using UnityEngine;

[System.Serializable]
public class MapParameters
{
    public Vector2Int dims;        
    public Vector3 anchor;
    public Vector3 noiseParameters;
    public Vector2Int noiseOffset;
    public float scale;
    public float exponent;
    public int islandSizeThreshold;
    [Range(.05f, .5f)] public float bridgesAccuracy;
    public float blockSize;
    public int forestVariety;
    public float forestDensity;
    public int floraVariety;
    public float floraDensity;
    public int trapVariety;
    public float trapDensity;
    public int[] containers;
    public Vector2Int contNumRange;
    public string[] ingredients;
    public Vector2Int ingNumRange;
    public string[] nonIngredients;
    public Vector2Int nonIngNumRange;
    public float[] nonIngredientsRates;
    public string[] entitiesForSpawn;
    public Vector2Int entNumRange;
    public LocationData[] neighborLocations;
    public string oldmanName;
    public string oldmanCowName;
    public string pigeonName;
    public int presetVariety;
    public float presetDensity;
    public int[] presetBlockSizes;
    public int randomSeed;
    public Color[] blockColors;
}