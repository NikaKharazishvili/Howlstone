using UnityEngine;
using UnityEngine.AddressableAssets;
using static GameConstants;

[DefaultExecutionOrder(-100)]
public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance;

    [field: SerializeField] public string AccountAndSceneName { get; private set; }
    [field: SerializeField] public Player Player { get; private set; }
    [field: SerializeField] public Transform Camera { get; private set; }
    [field: SerializeField] public Hud Hud { get; private set; }
    [field: SerializeField] public ShurikenProjectile[] ShurikenProjectiles { get; private set; }
    [field: SerializeField] public Mesh[] ShurikenModels { get; private set; }
    [field: SerializeField] public EnemyProjectile[] EnemyProjectiles { get; private set; }
    [field: SerializeField] public XPText[] XpTexts { get; private set; }

    [SerializeField] GameObject coinsParent, enemiesParent, breakablesParent;
    int currentCoins, maxCoins, currentKills, maxKills, currentBreaks, maxBreaks;
    bool isHudOn = true, canToggleHud = true;

    void DeleteAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        foreach (Transform coin in coinsParent.transform) coin.gameObject.SetActive(true);
    }

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
        AccountAndSceneName = $"{PlayerPrefs.GetString("Account")}_{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}_";
        currentCoins = PlayerPrefs.GetInt(AccountAndSceneName + "TakenCoinsCount", 0);
        currentKills = PlayerPrefs.GetInt(AccountAndSceneName + "Kills", 0);
        currentBreaks = PlayerPrefs.GetInt(AccountAndSceneName + "Breaks", 0);

        maxCoins = coinsParent.transform.childCount;
        maxKills = enemiesParent.transform.childCount;
        maxBreaks = breakablesParent.transform.childCount;

        Hud.SetCoins(currentCoins, maxCoins);
        Hud.SetKills(currentKills, maxKills);
        Hud.SetBreaks(currentBreaks, maxBreaks);

        foreach (Transform coin in coinsParent.transform) coin.gameObject.SetActive(PlayerPrefs.GetInt(AccountAndSceneName + coin.name, 0) == 0);
    }

    void SaveData()
    {
        int takenCoinsCount = 0;
        foreach (Transform coin in coinsParent.transform)
            if (!coin.gameObject.activeSelf)
            {
                takenCoinsCount++;
                PlayerPrefs.SetInt(AccountAndSceneName + coin.name, 1);
            }
        PlayerPrefs.SetInt(AccountAndSceneName + "TakenCoinsCount", takenCoinsCount);
    }
}