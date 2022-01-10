using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDestroyer : MonoBehaviour
{
    public void DestroyParentObj() {
        Destroy(transform.parent.gameObject);
    }

    public void DestroySelfObj() {
        Destroy(gameObject);
    }
}
