[System.Serializable]
public class Ingredient : GameDataEntry
{
    public string ing_name; // Localization link
    public float cooldown; // How much of a delay adds this ingredient to the cauldron
    public float breakChance; // How likely is that this ingredient will break the recipe
    public bool hasBeenDiscovered = false; // When a player comes to the location of the ingredient
    public string location; // In what location this ingredient is being spawned
    public int rarity; // How rare is this ingredient in the wolrd (lower is rarer)

    public Potion potionData; // In case this ingredient is potion
    public bool isPotion;

    // Used to calculate potion's color
    public float color_r;
    public float color_g;
    public float color_b;
    public float color_a;

    public Ingredient(
        int id,
        string ing_name,
        float cooldown,
        float breakChance,
        int rarity,
        float r,
        float g,
        float b,
        float a,
        string location,
        Potion potionData = null)
    {
        this.id = id;
        this.ing_name = ing_name;
        this.cooldown = cooldown;
        this.breakChance = breakChance;
        this.rarity = rarity;
        color_r = r;
        color_g = g;
        color_b = b;
        color_a = a;
        this.location = location;
        this.potionData = potionData;
        isPotion = potionData != null;
    }
}
