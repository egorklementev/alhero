using System.Collections.Generic;
using UnityEngine;

public class MapParameters
{
    public Vector2Int dims { get; private set; } = new Vector2Int(64, 64);        
    public Vector3 anchor { get; private set; } = Vector3.zero;
    public Vector3 noiseParameters { get; private set; } = new Vector3(1f, 2f, 4f);
    public Vector2Int noiseOffset { get; private set; } = Vector2Int.zero;
    public float scale { get; private set; } = 3f;
    public float exponent { get; private set; } = .225f;
    public int islandSizeThreshold { get; private set; } = 8;
    public float blockSize { get; private set; } = 7f;
    public int forestVariety { get; private set; } = 1;
    public float forestDensity { get; private set; } = .2f;
    public int floraDiversity { get; private set; } = 1;
    public float floraDensity { get; private set; } = .2f;
    public int containerVariety { get; private set; } = 1;
    public List<int> ingredientsForSpawn { get; private set; } = new List<int>();
    public Vector2Int ingNumRange { get; private set; } = Vector2Int.zero;
    public List<string> entitiesForSpawn { get; private set; } = new List<string>();
    public Vector2Int contNumRange { get; private set; } = Vector2Int.zero;
    public LocationData[] neighborLocations { get; private set; } 

    public MapParameters SetDims(Vector2Int dimensions)
    {
        dims = dimensions;
        return this;
    }

    public MapParameters SetAnchor(Vector3 anch)
    {
        anchor = anch;
        return this;
    }

    public MapParameters SetNoiseParams(Vector3 noise)
    {
        noiseParameters = noise;
        return this;
    }

    public MapParameters SetNoiseOffset(Vector2Int offset)
    {
        noiseOffset = offset;
        return this;
    }

    public MapParameters SetScale(float s)
    {
        scale = s;
        return this;
    }

    public MapParameters SetExponent(float exp)
    {
        exponent = exp;
        return this;
    }

    public MapParameters SetIslandSizeThreshold(int thresh)
    {
        islandSizeThreshold = thresh;
        return this;
    }

    public MapParameters SetBlockSize(float size)
    {
        blockSize = size;
        return this;
    }

    public MapParameters SetForestVariety(int variety)
    {
        forestVariety = variety;
        return this;
    }

    public MapParameters SetForestDensity(float density)
    {
        forestDensity = density;
        return this;
    }

    public MapParameters SetContainerVariety(int variety)
    {
        containerVariety = variety;
        return this;
    }

    public MapParameters SetContNumRange(Vector2Int range)
    {
        contNumRange = range;
        return this;
    }

    public MapParameters AddIngredientForSpawn(params int[] ingredients)
    {
        ingredientsForSpawn.AddRange(ingredients);
        return this;
    }

    public MapParameters SetIngNumRange(Vector2Int range)
    {
        ingNumRange = range;
        return this;
    }

    public MapParameters AddEntityForSpawn(params string[] entities)
    {
        entitiesForSpawn.AddRange(entities);
        return this;
    }

    public MapParameters SetNeighborLocations(LocationData[] lData)
    {
        neighborLocations = lData;
        return this;
    }

}