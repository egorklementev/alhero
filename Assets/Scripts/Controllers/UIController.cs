using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{

    public Animator rightPanelAnim;

    [Space(10f)]
    public List<GameObject> uiGroups;

    [Space(10f)]
    public TextMeshProUGUI fpsLine;
    public TextMeshProUGUI debugLine;

    [Space(10f)]
    public float delay = .5f;

    private static LinkedList<string> groupsForActivation = new LinkedList<string>();
    private static LinkedList<string> groupsForDeactivation = new LinkedList<string>();
    private static bool showRightPanel = false;

    // Update is called once per frame
    void Update()
    {
        int fps = (int)(1f / Time.unscaledDeltaTime);
        fpsLine.text = "FPS: " + fps.ToString();

        rightPanelAnim.SetBool("DoShow", showRightPanel);

        // Checking whether UI group activation/deactivation was scheduled
        if (groupsForActivation.First != null)
        {
            for (LinkedListNode<string> node = groupsForActivation.First; node != null; node = node.Next)
            {
                uiGroups.Find(group => group.name.Equals(node.Value)).SetActive(true);
            }
            groupsForActivation.Clear();
        }
        if (groupsForDeactivation.First != null)
        {
            for (LinkedListNode<string> node = groupsForDeactivation.First; node != null; node = node.Next)
            {
                StartCoroutine(HideUIGroup(uiGroups.Find(group => group.name.Equals(node.Value))));
            }
            groupsForDeactivation.Clear();
        }
    }

    public void SetDebugLine(string message)
    {
        debugLine.text = "Debug: " + message;
    }

    public static void TriggerRightPanel()
    {
        showRightPanel = !showRightPanel;
    }

    public static void ActivateUIGroup(string name)
    {
        groupsForActivation.AddLast(name);
    }

    public static void DeactivateUIGroup(string name)
    {
        groupsForDeactivation.AddLast(name);
    }

    IEnumerator HideUIGroup(GameObject obj)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
    }
}
