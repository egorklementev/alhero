using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using TMPro;
using System;
using System.Linq;

public class UIController : MonoBehaviour
{
    [FormerlySerializedAs("rightPanelAnim")]
    public Animator RightPanelAnim;
    [FormerlySerializedAs("fadePanelAnim")]
    public Animator FadePanelAnim;

    [Space(10f)]
    [FormerlySerializedAs("uiGroups")]
    public List<GameObject> UIGroups;
    [FormerlySerializedAs("sideLines")]
    public Transform SideLines;
    [FormerlySerializedAs("sideLine")]
    public GameObject SideLine;
    public GameObject EntityPanelPrefab;
    public Sprite[] EntityPanelSprites;
    public Transform EntityPanelAnchor;
    public Transform EntityPanelsParent;

    [Space(10f)]
    [FormerlySerializedAs("fpsLine")]
    public TextMeshProUGUI FpsLine;
    [FormerlySerializedAs("debugLine")]
    public TextMeshProUGUI DebugLine;
    [FormerlySerializedAs("coinLine")]
    public TextMeshProUGUI CoinLine;

    [Space(10f)]
    [FormerlySerializedAs("delay")]
    public float Delay = .5f;
    [FormerlySerializedAs("sideLineOffset")]
    public float SideLineOffset = 0f;

    [Space(15f)]
    public TextMeshProUGUI AvailablePigeonsLabel;
    public TextMeshProUGUI PotionsLeftLabel;

    [Space(20f)]
    public TextMeshProUGUI StatTotalScore;
    public TextMeshProUGUI StatDeaths;
    public TextMeshProUGUI StatIngsUsed;
    public TextMeshProUGUI StatPotionsFailed;
    public TextMeshProUGUI StatMoneyCollected;
    public TextMeshProUGUI StatMoneyEarned;
    public TextMeshProUGUI StatMoneySpent;
    public TextMeshProUGUI StatRaccoon;
    public TextMeshProUGUI StatItemsBought;
    public TextMeshProUGUI StatContainers;
    public TextMeshProUGUI StatLocations;

    public static string ActiveGroup = "none";
    public static int RequestedLinesNum = 0;

    private float fpsLineTimer = 0f;

    private static Dictionary<string, GameObject> entityPanels = new Dictionary<string, GameObject>();
    private static HashSet<string> activeGroups = new HashSet<string>();
    private static LinkedList<string> groupsForActivation = new LinkedList<string>();
    private static LinkedList<string> groupsForDeactivation = new LinkedList<string>();
    private static LinkedList<float> delaysForDeactivation = new LinkedList<float>();
    private static bool showRightPanel = false;
    private static bool triggerFadePanel = false;
    private static bool doNotFadeOut = false;
    private static float fadeSpeed = 1f;
    private static List<(string, float, object[])> requestedSideLines = new List<(string, float, object[])>();
    private static List<string> entityPanelsToSpawn = new List<string>();
    private static List<string> entityPanelsToDespawn = new List<string>();

    private void Start()
    {
        if (doNotFadeOut)
        {
            doNotFadeOut = false; // Reset
            FadePanelAnim.SetBool("DoNotFadeOut", true);
        }

        UpdateLabLabels();
    }

    void Update()
    {
        UpdateLines();

        RightPanelAnim.SetBool("DoShow", showRightPanel);

        UpdateFade();
        UpdateUIGroups();
        UpdateRequestedLines();

        UpdateEntityPanels();
    }

    public void UpdateLabLabels()
    {
        if (AvailablePigeonsLabel != null)
        {
            "pigeons_available".Localize("General", AvailablePigeonsLabel, DataController.genData.maxPigeons);
        }

        if (PotionsLeftLabel != null)
        {
            "potions_left".Localize(
                "General", 
                PotionsLeftLabel, 
                DataController.maximumRecipes - DataController.genData.potionsCooked);
        }
    }

    private void UpdateLines()
    {
        fpsLineTimer -= Time.deltaTime;
        if (fpsLineTimer - Time.deltaTime < 0f)
        {
            fpsLineTimer = .2f;

            int fps = (int)(1f / Time.unscaledDeltaTime);
            FpsLine.text = "FPS: " + fps.ToString();
            CoinLine.text = DataController.genData.coins.ToString();
        }
    }

    private void UpdateFade()
    {
        if (triggerFadePanel)
        {
            FadePanelAnim.SetFloat("fade_speed", fadeSpeed);
            FadePanelAnim.SetTrigger("Triggered");

            triggerFadePanel = false;
            fadeSpeed = 1f;
        }
    }

    private void UpdateUIGroups()
    {
        // Checking whether UI group activation/deactivation was scheduled
        if (groupsForActivation.First != null)
        {
            for (LinkedListNode<string> node = groupsForActivation.First; node != null; node = node.Next)
            {
                // $"Activating UI group: {node.Value}".Log(this);
                UIGroups.Find(group => group.name.Equals(node.Value)).SetActive(true);
                activeGroups.Add(node.Value);
            }
            groupsForActivation.Clear();
        }
        if (groupsForDeactivation.First != null)
        {
            for (LinkedListNode<string> node = groupsForDeactivation.First; node != null; node = node.Next)
            {
                StartCoroutine(HideUIGroup(
                    UIGroups.Find(group => group.name.Equals(node.Value)), 
                    delaysForDeactivation.First.Value));
                delaysForDeactivation.RemoveFirst();
                activeGroups.Remove(node.Value);
            }
            delaysForDeactivation.Clear();
            groupsForDeactivation.Clear();
        }
    }

    private void UpdateRequestedLines()
    {
        if (requestedSideLines.Count > 0)
        {
            foreach ((string, float, object[]) line in requestedSideLines)
            {
                GameObject sd = Instantiate(SideLine, SideLines);
                sd.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -100f * RequestedLinesNum - SideLineOffset); 
                if (line.Item3[0] is Potion pData)
                {
                    pData.LocalizePotion(sd.GetComponent<TextMeshProUGUI>());
                }
                else
                {
                    line.Item1.Localize("General", sd.GetComponent<TextMeshProUGUI>(), line.Item3);
                }
                sd.GetComponent<Animator>().SetFloat("Speed", line.Item2);
            }
            requestedSideLines.Clear();
        }
    }

    private void UpdateEntityPanels()
    {
        if (entityPanelsToSpawn.Count > 0)
        {
            foreach (string entityPanel in entityPanelsToSpawn)
            {
                try
                {
                    GameObject panel = Instantiate(
                        EntityPanelPrefab, 
                        EntityPanelsParent);
                    panel.GetComponent<Animator>().SetBool("DoShow", true);
                    panel.GetComponent<RectTransform>().position = 
                        new Vector3(0f, -100f * entityPanels.Count, 0f) + EntityPanelAnchor.position;

                    Transform panelIcon = panel.transform.Find("icon_button");
                    panelIcon.GetComponent<Image>().sprite = Array.Find(EntityPanelSprites, x => x.name.Equals($"icon_{entityPanel}"));
                    panelIcon.GetComponent<Button>().onClick.AddListener(() => TryToDespawnEntityPanel(entityPanel));
                    panelIcon.GetComponent<Button>().onClick.AddListener(() => TriggerRightPanel());
                    panelIcon.GetComponent<Button>().onClick.AddListener(() => ActivateUIGroup($"group_{entityPanel}"));

                    entityPanels.Add(entityPanel, panel);
                }
                catch (Exception)
                {
                    $"Entity panel \"{entityPanel}\" is already on the screen!".Warn(this);
                }
            }
            entityPanelsToSpawn.Clear();
        }

        if (entityPanelsToDespawn.Count > 0)
        {
            foreach (string entityPanel in entityPanelsToDespawn)
            {
                try
                {
                    entityPanels[entityPanel].GetComponent<Animator>().SetBool("DoShow", false);
                    Destroy(entityPanels[entityPanel], 1.5f);
                    entityPanels.Remove(entityPanel);
                }
                catch(Exception)
                {
                    $"No entity panel with name \"{entityPanel}\" is on the screen!".Warn(this);
                }
            }
            entityPanelsToDespawn.Clear();
        }
    }

    public void SetDebugLine(params string[] message)
    {
        string sep = " ";
        StringBuilder line = new StringBuilder("Debug: ");
        foreach (string str in message)
        {
            line.Append(str).Append(sep);
        }
        DebugLine.text = line.ToString();
    }

    public static void TriggerRightPanel()
    {
        showRightPanel = !showRightPanel;
        HeroMoveController.uiTookControl = showRightPanel;
    }

    public static void ActivateUIGroup(string name)
    {
        groupsForActivation.AddLast(name);
        ActiveGroup = name;
    }

    public static void DeactivateUIGroup(string name)
    {
        DeactivateUIGroup(name, 1f);
    }

    public static void DeactivateUIGroupInstantly(string name)
    {
        DeactivateUIGroup(name, 0f);
    }

    public static void DeactivateUIGroup(string name, float delay = 1f)
    {
        groupsForDeactivation.AddLast(name);
        delaysForDeactivation.AddLast(delay);
        ActiveGroup = "none";
    }

    public static bool IsGroupActivated(string name)
    {
        return activeGroups.Contains(name);
    }

    IEnumerator HideUIGroup(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj?.SetActive(false);
    }

    public void OnCloseButtonPressed()
    {
        Container c = LogicController.curContainer;
        if (c != null)
        {
            c.ResetSelection();
        }
        TriggerRightPanel();
        DeactivateUIGroup(ActiveGroup);
        LogicController.curContainer = null;
    }

    public static void TriggerFade(float speed = 1f)
    {
        triggerFadePanel = true;
        fadeSpeed = speed;
    }

    public void ShowLongFade()
    {
        FadePanelAnim.Play("FadeOutLong");
    }

    public void StartSceneFade(string sceneName, float delay, bool doFadeOut = true)
    {
        HeroMoveController.uiTookControl = true;
        FadePanelAnim.SetTrigger("SceneChange");
        FadePanelAnim.SetFloat("fade_speed", delay);
        UIController.doNotFadeOut = !doFadeOut;
        FadeLoadScene.sceneToLoad = sceneName;
    }

    public static void SpawnSideLine(params (string key, object[] prms)[] text)
    {
        requestedSideLines.AddRange(text.Select(t => (t.key, 1f, t.prms)));
        RequestedLinesNum += text.Length;
    }

    public static void SpawnSideLine(string text, object[] parameters, float duration = 4.5f)
    {
        requestedSideLines.Add((text, 4.5f / duration, parameters)); // 4.5f - animation duration
        RequestedLinesNum++;
    }

    public static void HideActiveGroups()
    {
        if (activeGroups.Count > 0)
        {
            TriggerRightPanel();
        }
        foreach (string group in activeGroups)
        {
            DeactivateUIGroup(group);
        }
    }

    public static void SpawnEntityPanel(string panelToSpawn)
    {
        if (activeGroups.Count == 0)
        {
            entityPanelsToSpawn.Add(panelToSpawn);
        }
    }

    public static void TryToDespawnEntityPanel(string panelToDespawn)
    {
        entityPanelsToDespawn.Add(panelToDespawn);
    }
}
