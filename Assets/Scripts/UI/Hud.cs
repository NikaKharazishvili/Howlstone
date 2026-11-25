using UnityEngine;
using UnityEngine.UIElements;

public class Hud : MonoBehaviour
{
    const string HiddenClass = "hidden", TiredClass = "energy-tired";
    [SerializeField] UIDocument document;
    VisualElement healthFill, energyFill, throwFill, xpFill;
    VisualElement[] slidingParts;
    Label coins, kills, breaks, health, xp, level, eventText;

    public void Bind()
    {
        VisualElement root = document.rootVisualElement;
        slidingParts = new[] { root.Q("Top"), root.Q("Quest"), root.Q("Progress"), root.Q("Bottom") };
        healthFill = root.Q("HealthFill");
        energyFill = root.Q("EnergyFill");
        throwFill = root.Q("ThrowFill");
        xpFill = root.Q("XpFill");
        coins = root.Q<Label>("Coins");
        kills = root.Q<Label>("Kills");
        breaks = root.Q<Label>("Breaks");
        health = root.Q<Label>("Health");
        xp = root.Q<Label>("Xp");
        level = root.Q<Label>("Level");
        eventText = root.Q<Label>("Event");
    }

    public void SetShown(bool shown) { foreach (VisualElement part in slidingParts) part.EnableInClassList(HiddenClass, !shown); }

    public void SetCoins(int current, int max) => coins.text = $"{current:00}/{max:00}";

    public void SetKills(int current, int max) => kills.text = $"{current:00}/{max:00}";

    public void SetBreaks(int current, int max) => breaks.text = $"{current:00}/{max:00}";

    public void SetHealth(float current, float max)
    {
        health.text = $"{current} / {max}";
        SetFill(healthFill, current / max);
    }

    public void SetEnergy(float fraction, bool canSprint)
    {
        SetFill(energyFill, fraction);
        energyFill.EnableInClassList(TiredClass, !canSprint);
    }

    public void SetThrow(float fraction) => SetFill(throwFill, fraction);

    public void SetXp(int current, int max)
    {
        xp.text = $"{current} / {max}";
        SetFill(xpFill, (float)current / max);
    }

    public void SetLevel(int newLevel) => level.text = newLevel.ToString();

    public void ShowEvent(string text, Color color)
    {
        eventText.text = text;
        eventText.style.color = color;
        this.Wait(3, h => h.eventText.text = null);
    }

    static void SetFill(VisualElement fill, float fraction) => fill.style.width = Length.Percent(Mathf.Clamp01(fraction) * 100);
}