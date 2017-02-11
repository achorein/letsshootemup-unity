/// <summary>
///  Achievement
/// </summary>
public class HF {
    public string id;
    public string description;
    public int nb = 0;
    public TYPE_HF type;
    public int gold = 0;
    public bool special = false;

    public enum TYPE_HF { Kill, Bonus, Level, Weapon, Other };

    public HF(TYPE_HF type, string description, int nb, int gold, string id) {
        this.description = description;
        this.nb = nb;
        this.type = type;
        this.gold = gold;
        this.id = id;
    }

    public HF(TYPE_HF type, string description, int nb, int gold, bool special, string id) {
        this.description = description;
        this.nb = nb;
        this.type = type;
        this.gold = gold;
        this.id = id;
        this.special = special;
    }
}
