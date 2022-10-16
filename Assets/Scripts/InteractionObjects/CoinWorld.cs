using UnityEngine;
using TMPro;

public class CoinWorld : ItemWorld 
{
    public TextMeshProUGUI[] Nums;    

    public int Count 
    { 
        get => _count; 
        set 
        { 
            _count = value < 1 ? 1 : value;
            UpdateText();
        } 
    }

    [SerializeField]
    private int _count; // How many coins are in this stack

    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateText();
    }

    private void UpdateText()
    {
        foreach(TextMeshProUGUI num in Nums)
        {
            num.text = Count.ToString();
        }
    }
}