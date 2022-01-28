using UnityEngine;
using TMPro;

public class SeedLineUpdater : MonoBehaviour
{
    private void OnEnable() {
        GetComponent<TMP_InputField>().text = DataController.genData.seed.ToString();
    }
}
