using UnityEngine;
using UnityEngine.AddressableAssets;
using static GameConstants;

[DefaultExecutionOrder(-100)]
public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance;

    [field: SerializeField] public Player Player { get; private set; }
    [field: SerializeField] public Transform Camera { get; private set; }
    [field: SerializeField] public Hud Hud { get; private set; }
    [field: SerializeField] public ShurikenProjectile[] ShurikenProjectiles { get; private set; }
    [field: SerializeField] public Mesh[] ShurikenModels { get; private set; }
    [field: SerializeField] public EnemyProjectile[] EnemyProjectiles { get; private set; }
    [field: SerializeField] public XPText[] XpTexts { get; private set; }

    [SerializeField] GameObject coinsParent, enemiesParent, breakablesParent;
    const string TestAccount = "Player", TestDifficulty = "Medium";
    int currentCoins, maxCoins, currentKills, maxKills, currentBreaks, maxBreaks;
    bool isHudOn = true, canToggleHud = true;

    void SetUpScene()
    {
        Camera = UnityEngine.Camera.main.transform;
        Player = FindFirstObjectByType<Player>();
        XpTexts = FindObjectsByType<XPText>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        ShurikenProjectiles = FindObjectsByType<ShurikenProjectile>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        EnemyProjectiles = FindObjectsByType<EnemyProjectile>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }

    void Awake()
    {
        Instance = this;
        if (SaveFile.Current == null && !SaveFile.LoadFirstFilledSlot()) SaveFile.StartNew(0, TestAccount, TestDifficulty);
        Player.SM = this;
        Cam.Player = Player;
        ShurikenProjectile.SM = this;
        Enemy.SM = this;
        Enemy.Player = Player;
        EnemyProjectile.SM = this;
        EnemyProjectile.Player = Player;
        Hazard.SM = this;
        XPText.Cam = Camera;
        Hud.gameObject.SetActive(true);
        Hud.Bind();
        LoadData();
    }

    public void PlaySound(AudioSource audioSource, params SoundType[] variants) => Addressables.LoadAssetAsync<AudioClip>(variants[Random.Range(0, variants.Length)].ToString()).Completed += h => audioSource.PlayOneShot(h.Result);

    public void ToggleHud()
    {
        if (!canToggleHud) return;
        isHudOn = !isHudOn;
        Hud.SetShown(isHudOn);
        canToggleHud = false;
        this.Wait(ToggleHudCD, s => s.canToggleHud = true);
    }

    public void AddCoin(GameObject coin)
    {
        currentCoins++;
        Hud.SetCoins(currentCoins, maxCoins);
        coin.SetActive(false);
    }

    public void AddKill()
    {
        currentKills++;
        Hud.SetKills(currentKills, maxKills);
    }

    public void AddBreak()
    {
        currentBreaks++;
        Hud.SetBreaks(currentBreaks, maxBreaks);
    }

    public void EventText(int Event) => Hud.ShowEvent(Event == 1 ? "You are dead" : Event == 2 ? "Level Up!" : Event == 3 ? "You won!" : null, Event == 1 ? Color.red : Color.green);

    void LoadData()
    {
        LevelSave level = SaveFile.Current.GetLevel(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        currentCoins = level.takenCoins.Count;
        currentKills = level.kills;

        maxCoins = coinsParent.transform.childCount;
        maxKills = enemiesParent.transform.childCount;
        maxBreaks = breakablesParent.transform.childCount;

        Hud.SetCoins(currentCoins, maxCoins);
        Hud.SetKills(currentKills, maxKills);
        Hud.SetBreaks(currentBreaks, maxBreaks);

        foreach (Transform coin in coinsParent.transform) coin.gameObject.SetActive(!level.takenCoins.Contains(coin.name));
    }

    public void SaveGame()
    {
        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        LevelSave level = SaveFile.Current.GetLevel(scene);
        level.takenCoins.Clear();
        foreach (Transform coin in coinsParent.transform)
            if (!coin.gameObject.activeSelf) level.takenCoins.Add(coin.name);
        level.kills = currentKills;
        Player.WriteSave(SaveFile.Current.character, level);
        SaveFile.Current.lastScene = scene;
        SaveFile.Write();
    }
}