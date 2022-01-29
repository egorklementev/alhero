using System.Collections;
using UnityEngine;
using TMPro;

public class SeedLineUpdater : MonoBehaviour
{
    private void OnEnable() {
        GetComponent<TMP_InputField>().text = DataController.genData.seed.ToString();
    }

    public void GenerateNewSeed()
    {
        DataController.newSeed = new System.Random().Next();
        GetComponent<TMP_InputField>().text = DataController.newSeed.ToString();
    }

    public void CheckInputSeed()
    {
        if (int.TryParse(GetComponent<TMP_InputField>().text, out int seed))
        {
            DataController.newSeed = seed;
        }
        else
        {
            GetComponent<TMP_InputField>().text = "Incorrect seed!!!";
            StartCoroutine(ChangeSeedDelayed());
        }
    }

    IEnumerator ChangeSeedDelayed()
    {
        yield return new WaitForSeconds(2f);
        GenerateNewSeed();
    }
}
