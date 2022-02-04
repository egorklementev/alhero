using UnityEngine;

public class FadeLoadScene : MonoBehaviour{

    public SceneManager manager;

    public static string sceneToLoad = "none";

    public void ChangeScene()
    {
        manager.LoadScene(sceneToLoad);
    }
    
}