using UnityEngine;

public class SideLine : MonoBehaviour 
{
    public void Disable()
    {
        UIController.RequestedLinesNum--;
        Destroy(gameObject);
    }
}