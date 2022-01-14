using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemUI : MonoBehaviour
{
    public GameObject worldItem;

    private void Start() {
        foreach (Material m in GetComponent<MeshRenderer>().materials)
        {
            m.renderQueue = 3002;
        }
    }
}
