using UnityEngine;
using Manager = UnityEngine.SceneManagement.SceneManager;

public class SceneManager : MonoBehaviour {

    public void LoadScene(string sceneName)
    {
        Manager.LoadScene(sceneName);
    }
}