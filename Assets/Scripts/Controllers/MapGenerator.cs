using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Vector2Int mapSize;
    public Vector3 startPoint;
    public Vector3 noiseParamters;
    public Transform environment;
    public GameObject forestBlock;

    [Range(0, 2 << 15)]
    public int offsetWidth;
    [Range(0, 2 << 15)]
    public int offsetHeight;
    [Range(1f, 100f)]
    public float scale = 1f;
    [Range(.01f, 10f)]
    public float exponent = 1f;

    private Block[,] map;

    public void GenerateForestGround()
    {
        foreach (Transform child in environment)
        {
            Destroy(child.gameObject);
        }

        int width = mapSize.x;
        int height = mapSize.y;

        map = new Block[width, height];

        Vector3 np = noiseParamters * scale;

        for (int w = 0; w < width; w++)
        {
            for (int h = 0; h < height; h++)
            {
                float value_big = Mathf.PerlinNoise(
                    (float) w / width * np.x + offsetWidth,
                    (float) h / height * np.x + offsetHeight
                );
                float value_mid = Mathf.PerlinNoise(
                    (float) w / width * np.y + offsetWidth,
                    (float) h / height * np.y + offsetHeight
                );
                float value_small = Mathf.PerlinNoise(
                    (float) w / width * np.z + offsetWidth,
                    (float) h / height * np.z + offsetHeight
                );

                float value = (value_big + value_mid + value_small) / 
                    (np.x + np.y + np.z);
                value = Mathf.Pow(value, exponent);

                if (value > .5f)
                {
                    map[w, h] = new Block(1); // Ground
                    Instantiate(forestBlock, startPoint + new Vector3(10f * w, 0f, 10f * h), forestBlock.transform.rotation, environment);
                }
                else
                {
                    map[w, h] = new Block(0); // Air
                }
            }
        }
    }
}
