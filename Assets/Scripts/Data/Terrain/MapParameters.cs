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
    public float blockSize;
    public int forestVariety;
    public float forestDensity;
    public int floraDiversity;
    public float floraDensity;
    public int[] containers;
    public Vector2Int contNumRange;
    public string[] ingredients;
    public Vector2Int ingNumRange;
    public string[] entitiesForSpawn;
    public Vector2Int entNumRange;
    public LocationData[] neighborLocations;
}