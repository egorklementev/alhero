using UnityEngine;

public class Portal : MonoBehaviour {

    public string SceneToLoad = "none";

    public LogicController logic;
    
    private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player"))
        {
            other.attachedRigidbody.velocity = Vector3.zero;
            logic.ChangeScene(SceneToLoad);
        }     
    }

}