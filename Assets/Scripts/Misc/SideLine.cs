using UnityEngine;

public class SideLine : MonoBehaviour 
{
    public void Disable()
    {
        UIController.requestedLinesNum--;
        Destroy(gameObject);
    }
}