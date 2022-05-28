using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{

    public Animator rightPanelAnim;
    public Animator fadePanelAnim;

    [Space(10f)]
    public List<GameObject> uiGroups;
    public Transform sideLines;
    public GameObject sideLine;

    [Space(10f)]
    public TextMeshProUGUI fpsLine;
    public TextMeshProUGUI debugLine;
    public TextMeshProUGUI coinLine;

    [Space(10f)]
    public float delay = .5f;
    public float sideLineOffset = 0f;

    public static string activeGroup = "none";
    public static int requestedLinesNum = 0;

    private static LinkedList<string> groupsForActivation = new LinkedList<string>();
    private static LinkedList<string> groupsForDeactivation = new LinkedList<string>();
    private static LinkedList<string> groupsForInstantDeactivation = new LinkedList<string>();
    private static bool showRightPanel = false;
    private static bool triggerFadePanel = false;
    private static float fadeSpeed = 1f;
    private static List<string> requestedSideLines = new List<string>();

    // Update is called once per frame
    void Update()
    {
        int fps = (int)(1f / Time.unscaledDeltaTime);
        fpsLine.text = "FPS: " + fps.ToString();
        coinLine.text = DataController.genData.coins.ToString();

        rightPanelAnim.SetBool("DoShow", showRightPanel);

        if (triggerFadePanel)
        {
            fadePanelAnim.SetFloat("fade_speed", fadeSpeed);
            fadePanelAnim.SetTrigger("Triggered");

            triggerFadePanel = false;
            fadeSpeed = 1f;
        }

        // Checking whether UI group activation/deactivation was scheduled
        if (groupsForActivation.First != null)
        {
            for (LinkedListNode<string> node = groupsForActivation.First; node != null; node = node.Next)
            {
                uiGroups.Find(group => group.name.Equals(node.Value)).SetActive(true);
            }
            groupsForActivation.Clear();
        }
        if (groupsForInstantDeactivation.First != null)
        {
            for (LinkedListNode<string> node = groupsForInstantDeactivation.First; node != null; node = node.Next)
            {
                uiGroups.Find(group => group.name.Equals(node.Value)).SetActive(false);
            }
            groupsForInstantDeactivation.Clear();
        }
        if (groupsForDeactivation.First != null)
        {
            for (LinkedListNode<string> node = groupsForDeactivation.First; node != null; node = node.Next)
            {
                StartCoroutine(HideUIGroup(uiGroups.Find(group => group.name.Equals(node.Value))));
            }
            groupsForDeactivation.Clear();
        }

        foreach (string line in requestedSideLines)
        {
            GameObject sd = Instantiate(sideLine, sideLines);
            sd.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -60f * requestedLinesNum - sideLineOffset); 
            sd.GetComponent<TextMeshProUGUI>().text = line;
        }
        requestedSideLines.Clear();
    }

    public void SetDebugLine(params string[] message)
    {
        string sep = " ";
        StringBuilder line = new StringBuilder("Debug: ");
        foreach (string str in message)
        {
            line.Append(str).Append(sep);
        }
        debugLine.text = line.ToString();
    }

    public static void TriggerRightPanel()
    {
        showRightPanel = !showRightPanel;
        HeroMoveController.uiTookControl = showRightPanel;
    }

    public static void ActivateUIGroup(string name)
    {
        groupsForActivation.AddLast(name);
        activeGroup = name;
    }

    public static void DeactivateUIGroup(string name)
    {
        groupsForDeactivation.AddLast(name);
    }

    public static void DeactivateUIGroupInstantly(string name)
    {
        groupsForInstantDeactivation.AddLast(name);
    }

    IEnumerator HideUIGroup(GameObject obj, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
    }

    public void OnCloseButtonPressed()
    {
        Container c = LogicController.curContainer;
        if (c != null)
        {
            c.ResetSelection();
        }
        TriggerRightPanel();
        DeactivateUIGroup(activeGroup);
        activeGroup = "none";
        LogicController.curContainer = null;
    }

    public static void TriggerFade(float speed = 1f)
    {
        triggerFadePanel = true;
        fadeSpeed = speed;
    }

    public void ShowLongFade()
    {
        fadePanelAnim.Play("FadeOutLong");
    }

    public void StartSceneFade(string sceneName, float delay)
    {
        // HeroMoveController.uiTookControl = true;
        fadePanelAnim.SetTrigger("SceneChange");
        fadePanelAnim.SetFloat("fade_speed", delay);
        FadeLoadScene.sceneToLoad = sceneName;
    }

    public static void SpawnSideLine(params string[] text)
    {
        requestedSideLines.AddRange(text);
        requestedLinesNum += text.Length;
    }
}
