using UnityEngine;
using System.Collections.Generic;

public class MinimapController : MonoBehaviour 
{
    public float mapSize = 200f;
    public Renderer mapRenderer;    
    public RectTransform heroMarker;
    public GameObject portalMarkerPref;

    [Space(15f)]
    public LogicController logic;

    private float realMapSize = 10f;
    private List<GameObject> portalMarkers = new List<GameObject>();

    private void Update() 
    {
        Vector3 heroPos = logic.GetHeroPosition();
        heroMarker.anchoredPosition = new Vector2(
            (heroPos.x / realMapSize) * mapSize + 12f, 
            (heroPos.z / realMapSize) * mapSize + 12f);
    }

    public void UpdateMinimap(Map map)
    {
        foreach (GameObject pMarker in portalMarkers)
        {
            Destroy(pMarker);
        }

        realMapSize = map.Parameters.blockSize * map.Width * 2f;

        Texture2D mapTexture = new Texture2D(map.Width, map.Height);
        for (int w = 0; w < map.Width; w++)
        {
            for (int h = 0; h < map.Height; h++)
            {
                mapTexture.SetPixel(w, h, map.GetBlock(w, h).GetMapColor());

                if (map.GetBlock(w, h).CntmntType == Block.ContainmentType.PORTAL)
                {
                    GameObject marker = Instantiate(portalMarkerPref, transform as RectTransform);
                    (marker.transform as RectTransform).anchoredPosition = new Vector2(
                        ((float)w / map.Width) * mapSize + 12f,
                        ((float)h / map.Height) * mapSize + 12f
                    );
                    portalMarkers.Add(marker);
                }
            }
        }
        mapTexture.filterMode = FilterMode.Point;
        mapTexture.Apply();
        mapRenderer.material.mainTexture = mapTexture;
        mapRenderer.material.renderQueue = 2985; // Transparent-15
    }

}