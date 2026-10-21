using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The "ransom" sequence, ported from the ransomdoors project and re-cast for this game.
///
///   click the glitched cloud
///     -> warning window: clicking is forbidden for a few seconds
///          survived (no click)  -> everything quietly returns to normal
///          clicked inside it   -> jumpscare
///     -> jumpscare + fake download screen with glitched tabs
///     -> collect the coins that drop onto the ground before the timer expires
///          all collected       -> thank-you animation, play continues
///          timer expired       -> score and every upgrade are wiped
///
/// The four values the designer asked to tune live in the Inspector on the
/// "RansomEvent" object in the Main scene; see the [Header] blocks below.
/// </summary>
public class RansomEvent : MonoBehaviour
{
    // ---------------------------------------------------------------- config

    [Header("1. Glitched cloud")]
    [Tooltip("Percent chance that the cloud turns glitched each time it wraps back to the left edge.")]
    [Range(0f, 100f)]
    [SerializeField] private float _glitchChancePercent = 5f;

    [Header("2. Warning window")]
    [Tooltip("Seconds after clicking the glitched cloud during which clicking is forbidden. " +
             "A click inside this window triggers the jumpscare; surviving it cancels the ransom.")]
    [SerializeField] private float _clickBlockDuration = 3f;

    [Header("3. Coins")]
    [Tooltip("How many coins must be collected to pay the ransom.")]
    [SerializeField] private int _coinsToCollect = 8;

    [Header("4. Time limit")]
    [Tooltip("Seconds allowed to collect every coin. Running out wipes the score and all upgrades.")]
    [SerializeField] private float _collectTimeLimit = 20f;

    [Header("Scene references (auto-found by name when left empty)")]
    [SerializeField] private SpriteRenderer _sky;
    [SerializeField] private Transform _ground;

    [Header("Sound volumes")]
    [Range(0f, 1f)]
    [SerializeField] private float _sfxVolume = 0.8f;

    [Range(0f, 1f)]
    [SerializeField] private float _musicVolume = 0.4f;

    /// <summary>Read by GlitchCloud so the cloud and the controller share one value.</summary>
    public float GlitchChancePercent => _glitchChancePercent;

    // ---------------------------------------------------------------- sprites

    private const string StopSignPath = "Ransom/stop_sign";
    private const string IdlePath = "Ransom/ransom_idle";
    private const string AttackPath = "Ransom/ransom_attack";
    private const string CoinPath = "Ransom/gold";
    private const string OkSignPath = "Ransom/ok_sign";
    private const string ThanksPath = "Ransom/thanks";

    // Sound names and their trigger points both come from the ransomdoors project.
    private const string SpawnSoundPath = "Ransom/Sounds/spawn";
    private const string AttackSoundPath = "Ransom/Sounds/attack";
    private const string InstallSoundPath = "Ransom/Sounds/install";
    private const string CashSoundPath = "Ransom/Sounds/cash";
    private const string ThankYouSoundPath = "Ransom/Sounds/thankyou";
    private const string TauntSpawnSoundPath = "Ransom/Sounds/tauntSpawn";
    private const string TauntLeaveSoundPath = "Ransom/Sounds/tauntLeave";
    private const string Layer1Path = "Ransom/Sounds/layer1";
    private const string Layer2Path = "Ransom/Sounds/layer2";
    private const string Layer3Path = "Ransom/Sounds/layer3";

    private Sprite _stopSign;
    private Sprite _idle;
    private Sprite _attack;
    private Sprite _coinSprite;
    private Sprite _okSign;
    private Sprite _thanks;
    private Sprite[] _glitchTabs;

    private AudioClip _spawnSound;
    private AudioClip _attackSound;
    private AudioClip _installSound;
    private AudioClip _cashSound;
    private AudioClip _thankYouSound;
    private AudioClip _tauntSpawnSound;
    private AudioClip _tauntLeaveSound;
    private AudioClip _layer1;
    private AudioClip _layer2;
    private AudioClip _layer3;

    private AudioSource _sfxSource;
    private AudioSource _musicSource;

    // ---------------------------------------------------------------- runtime

    private enum Phase
    {
        Idle,
        Warning,
        Jumpscare,
        Collect,
        Result
    }

    private static readonly int OrderWarning = 220;
    private static readonly int OrderTabs = 150;
    private static readonly int OrderCoins = 200;
    private static readonly int OrderAttack = 260;

    private Phase _phase = Phase.Idle;
    private readonly List<GameObject> _overlays = new List<GameObject>();
    private readonly List<RansomCoin> _coins = new List<RansomCoin>();
    private readonly List<Transform> _tabs = new List<Transform>();

    private Color _skyOriginalColor;
    private int _collected;
    private float _hudTimeLeft;
    private float _tabTimer;
    private GUIStyle _hudStyle;

    public static RansomEvent Instance { get; private set; }

    // ---------------------------------------------------------------- setup

    private void Awake()
    {
        Instance = this;

        _stopSign = LoadSprite(StopSignPath);
        _idle = LoadSprite(IdlePath);
        _attack = LoadSprite(AttackPath);
        _coinSprite = LoadSprite(CoinPath);
        _okSign = LoadSprite(OkSignPath);
        _thanks = LoadSprite(ThanksPath);
        _glitchTabs = Resources.LoadAll<Sprite>("Ransom");

        _spawnSound = LoadClip(SpawnSoundPath);
        _attackSound = LoadClip(AttackSoundPath);
        _installSound = LoadClip(InstallSoundPath);
        _cashSound = LoadClip(CashSoundPath);
        _thankYouSound = LoadClip(ThankYouSoundPath);
        _tauntSpawnSound = LoadClip(TauntSpawnSoundPath);
        _tauntLeaveSound = LoadClip(TauntLeaveSoundPath);
        _layer1 = LoadClip(Layer1Path);
        _layer2 = LoadClip(Layer2Path);
        _layer3 = LoadClip(Layer3Path);

        // One source for one-shot effects, one for the looping ransom layers.
        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;
        _sfxSource.spatialBlend = 0f;

        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.playOnAwake = false;
        _musicSource.loop = true;
        _musicSource.spatialBlend = 0f;

        if (_sky == null)
        {
            GameObject skyObject = GameObject.Find("Sky");
            if (skyObject != null)
            {
                _sky = skyObject.GetComponent<SpriteRenderer>();
            }
        }

        if (_ground == null)
        {
            GameObject groundObject = GameObject.Find("ground");
            if (groundObject != null)
            {
                _ground = groundObject.transform;
            }
        }

        if (_sky != null)
        {
            _skyOriginalColor = _sky.color;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private static Sprite LoadSprite(string path)
    {
        // LoadAll keeps working whether the texture imported as Single or as a sliced sheet.
        Sprite[] sprites = Resources.LoadAll<Sprite>(path);
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning($"RansomEvent: no Sprite at 'Resources/{path}'. " +
                             "Set that texture's Import Settings to Texture Type 'Sprite'.");
            return null;
        }

        return sprites[0];
    }

    // ---------------------------------------------------------------- audio

    private static AudioClip LoadClip(string path)
    {
        AudioClip clip = Resources.Load<AudioClip>(path);
        if (clip == null)
        {
            Debug.LogWarning($"RansomEvent: no AudioClip at 'Resources/{path}'.");
        }

        return clip;
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null || _sfxSource == null)
        {
            return;
        }

        _sfxSource.PlayOneShot(clip, _sfxVolume);
    }

    /// <summary>
    /// Swaps the looping ransom layer. ransomdoors plays layer1, then 2, then 3 as
    /// its countdown runs down; here the switch points follow the configured
    /// time limit instead of a fixed 90 second infection.
    /// </summary>
    private void PlayLayer(AudioClip clip)
    {
        if (_musicSource == null)
        {
            return;
        }

        _musicSource.Stop();

        if (clip == null)
        {
            return;
        }

        _musicSource.clip = clip;
        _musicSource.volume = _musicVolume;
        _musicSource.loop = true;
        _musicSource.Play();
    }

    private void StopLayer()
    {
        if (_musicSource != null)
        {
            _musicSource.Stop();
        }
    }

    // ---------------------------------------------------------------- API

    /// <summary>Called by GlitchCloud when the player clicks the glitched cloud.</summary>
    public void BeginFromGlitchCloud()
    {
        if (_phase != Phase.Idle)
        {
            return;
        }

        StartCoroutine(RunSequence());
    }

    // ---------------------------------------------------------------- sequence

    private IEnumerator RunSequence()
    {
        // ---- phase 1: warning. Clicking is forbidden; a click here is punished.
        _phase = Phase.Warning;
        GameData.ScoreBlocked = true;
        SetSky(new Color(0.30f, 0.02f, 0.02f, 1f));
        PlaySfx(_spawnSound);

        // Both faces show up at random spots; margins keep them fully on screen.
        float idleSize = 2.4f;
        float stopSize = 2.6f;
        float idleMargin = idleSize * 0.5f + 0.2f;
        float stopMargin = stopSize * 0.5f + 0.2f;

        Vector3 idleBase = RandomScreenPoint(idleMargin);
        Vector3 stopBase = RandomScreenPoint(stopMargin);

        SpriteRenderer idleRenderer = SpawnOverlay(
            "RansomIdle", _idle, idleBase, idleSize, OrderWarning);
        SpriteRenderer stopRenderer = SpawnOverlay(
            "RansomStopSign", _stopSign, stopBase, stopSize, OrderWarning + 1);

        float waited = 0f;
        float nextJump = 0f;
        bool punished = false;
        while (waited < _clickBlockDuration)
        {
            waited += Time.deltaTime;
            _hudTimeLeft = Mathf.Max(0f, _clickBlockDuration - waited);

            // Every so often both faces blink to a fresh random spot.
            if (waited >= nextJump)
            {
                nextJump = waited + 0.55f;
                idleBase = RandomScreenPoint(idleMargin);
                stopBase = RandomScreenPoint(stopMargin);
            }

            JitterWarningOverlays(idleRenderer, idleBase, stopRenderer, stopBase);

            if (waited > 0.15f && AnyInputThisFrame())
            {
                punished = true;
                break;
            }

            yield return null;
        }

        if (!punished)
        {
            // The player stopped clicking in time and dodged the ransom.
            yield return StartCoroutine(Finish(false, false));
            yield break;
        }

        // ---- phase 2: jumpscare, then the fake download screen.
        _phase = Phase.Jumpscare;
        PlaySfx(_attackSound);
        DestroyOverlay(ref idleRenderer);
        DestroyOverlay(ref stopRenderer);
        SetSky(new Color(0.45f, 0.03f, 0.03f, 1f));

        SpriteRenderer attackRenderer = SpawnOverlay(
            "RansomAttack", _attack, Vector3.zero, 9.5f, OrderAttack);

        for (float t = 0f; t < 0.9f; t += Time.deltaTime)
        {
            if (attackRenderer != null)
            {
                attackRenderer.transform.position = new Vector3(
                    Random.Range(-0.35f, 0.35f),
                    Random.Range(-0.35f, 0.35f),
                    0f);
                attackRenderer.transform.localScale *= 1f + Random.Range(-0.02f, 0.02f);
            }

            yield return null;
        }

        DestroyOverlay(ref attackRenderer);
        SpawnGlitchTabs(6);
        PlaySfx(_installSound);
        PlaySfx(_tauntSpawnSound);

        for (float t = 0f; t < 1.8f; t += Time.deltaTime)
        {
            UpdateGlitchTabs(Time.deltaTime);
            yield return null;
        }

        // ---- phase 3: collect the coins.
        _phase = Phase.Collect;
        _collected = 0;
        _hudTimeLeft = _collectTimeLimit;
        int target = Mathf.Max(1, _coinsToCollect);
        SpawnCoins(target);

        // ransomdoors runs layer1, then 2, then 3 across its countdown; here the
        // switch points are thirds of the configured time limit.
        int layerStage = 0;
        PlayLayer(_layer1);

        float remaining = _collectTimeLimit;
        while (remaining > 0f && _collected < target)
        {
            remaining -= Time.deltaTime;
            _hudTimeLeft = Mathf.Max(0f, remaining);
            UpdateGlitchTabs(Time.deltaTime);

            if (layerStage == 0 && remaining <= _collectTimeLimit * 0.66f)
            {
                layerStage = 1;
                PlayLayer(_layer2);
            }
            else if (layerStage == 1 && remaining <= _collectTimeLimit * 0.33f)
            {
                layerStage = 2;
                PlayLayer(_layer3);
            }

            if (Input.GetMouseButtonDown(0))
            {
                TryPickCoin();
            }

            yield return null;
        }

        bool paid = _collected >= target;
        yield return StartCoroutine(Finish(paid, true));
    }

    private IEnumerator Finish(bool paid, bool showResult)
    {
        _phase = Phase.Result;
        _hudTimeLeft = 0f;
        DestroyGlitchTabs();
        ClearCoins();
        StopLayer();

        if (paid)
        {
            SpriteRenderer okRenderer = SpawnOverlay(
                "RansomOkSign", _okSign, new Vector3(0f, 1.1f, 0f), 2.6f, OrderAttack);
            SpriteRenderer thanksRenderer = SpawnOverlay(
                "RansomThanks", _thanks, new Vector3(0f, -1.6f, 0f), 4.5f, OrderAttack);

            PlaySfx(_thankYouSound);

            for (float t = 0f; t < 0.6f; t += Time.deltaTime)
            {
                float grow = Mathf.SmoothStep(0.1f, 1f, t / 0.6f);
                if (okRenderer != null)
                {
                    okRenderer.transform.localScale = Vector3.one * (grow * ScaleFor(_okSign, 2.6f));
                }

                yield return null;
            }

            for (float t = 0f; t < 0.5f; t += Time.deltaTime)
            {
                float grow = Mathf.SmoothStep(0.1f, 1f, t / 0.5f);
                if (thanksRenderer != null)
                {
                    thanksRenderer.transform.localScale = Vector3.one * (grow * ScaleFor(_thanks, 4.5f));
                }

                yield return null;
            }

            yield return new WaitForSeconds(2.2f);
            DestroyOverlay(ref okRenderer);
            DestroyOverlay(ref thanksRenderer);
        }
        else
        {
            // Failed the collection (or never got here): the ransom is paid the hard way.
            if (showResult)
            {
                GameData.ResetAll();
                PlaySfx(_attackSound);

                SpriteRenderer lostRenderer = SpawnOverlay(
                    "RansomAttack", _attack, Vector3.zero, 6.5f, OrderAttack);
                yield return new WaitForSeconds(1.4f);
                DestroyOverlay(ref lostRenderer);
            }
        }

        SetSky(_skyOriginalColor);
        CleanupOverlays();
        GameData.ScoreBlocked = false;
        _phase = Phase.Idle;
    }

    // ---------------------------------------------------------------- coins

    private void SpawnCoins(int count)
    {
        float minX = -7f;
        float maxX = 7f;
        float landY = -2.3f;

        SpriteRenderer groundRenderer = _ground != null ? _ground.GetComponent<SpriteRenderer>() : null;
        if (groundRenderer != null)
        {
            Bounds bounds = groundRenderer.bounds;
            minX = bounds.min.x + bounds.size.x * 0.08f;
            maxX = bounds.max.x - bounds.size.x * 0.08f;
            landY = bounds.max.y - 0.05f;
        }

        for (int i = 0; i < count; i++)
        {
            if (_coinSprite == null)
            {
                break;
            }

            GameObject coinObject = new GameObject($"RansomCoin{i}");
            coinObject.transform.position = new Vector3(
                Random.Range(minX, maxX),
                ViewHalfHeight + Random.Range(0.5f, 4f),
                0f);

            SpriteRenderer renderer = coinObject.AddComponent<SpriteRenderer>();
            renderer.sprite = _coinSprite;
            renderer.sortingOrder = OrderCoins;

            RansomCoin coin = coinObject.AddComponent<RansomCoin>();
            coin.Initialize(_coinSprite, 1.0f, landY, Random.Range(4f, 7f));
            _coins.Add(coin);
        }
    }

    private void TryPickCoin()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        Vector2 pointer = cam.ScreenToWorldPoint(Input.mousePosition);

        RansomCoin best = null;
        float bestDistance = float.MaxValue;

        foreach (RansomCoin coin in _coins)
        {
            if (coin == null || coin.Collected || !coin.ContainsPoint(pointer))
            {
                continue;
            }

            float distance = Vector2.Distance(pointer, coin.transform.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = coin;
            }
        }

        if (best == null)
        {
            return;
        }

        best.Collect();
        _coins.Remove(best);
        _collected++;
        PlaySfx(_cashSound);
    }

    private void ClearCoins()
    {
        foreach (RansomCoin coin in _coins)
        {
            if (coin != null)
            {
                Destroy(coin.gameObject);
            }
        }

        _coins.Clear();
    }

    // ---------------------------------------------------------------- glitch tabs

    private void SpawnGlitchTabs(int count)
    {
        if (_glitchTabs == null)
        {
            return;
        }

        // The Ransom folder also holds the ransom art; only the glitch photos are tabs.
        List<Sprite> candidates = new List<Sprite>();
        foreach (Sprite sprite in _glitchTabs)
        {
            if (sprite != null && sprite.name.ToLowerInvariant().StartsWith("glitch"))
            {
                candidates.Add(sprite);
            }
        }

        if (candidates.Count == 0)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            GameObject tabObject = new GameObject($"GlitchTab{i}");
            SpriteRenderer renderer = tabObject.AddComponent<SpriteRenderer>();
            renderer.sprite = candidates[Random.Range(0, candidates.Count)];
            renderer.sortingOrder = OrderTabs;
            _tabs.Add(tabObject.transform);

            RandomizeTab(tabObject.transform, renderer);
        }
    }

    private void RandomizeTab(Transform tab, SpriteRenderer renderer)
    {
        tab.position = new Vector3(
            Random.Range(-ViewHalfWidth * 0.8f, ViewHalfWidth * 0.8f),
            Random.Range(-ViewHalfHeight * 0.8f, ViewHalfHeight * 0.8f),
            0f);

        float size = Random.Range(1.6f, 3.4f);
        float scale = renderer != null ? ScaleFor(renderer.sprite, size) : size;
        tab.localScale = new Vector3(scale, scale, 1f);
        tab.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-12f, 12f));
    }

    private void UpdateGlitchTabs(float deltaTime)
    {
        _tabTimer -= deltaTime;
        if (_tabTimer > 0f)
        {
            return;
        }

        _tabTimer = 0.3f;
        for (int i = 0; i < _tabs.Count; i++)
        {
            Transform tab = _tabs[i];
            if (tab == null)
            {
                continue;
            }

            RandomizeTab(tab, tab.GetComponent<SpriteRenderer>());
        }
    }

    private void DestroyGlitchTabs()
    {
        if (_tabs.Count > 0)
        {
            PlaySfx(_tauntLeaveSound);
        }

        foreach (Transform tab in _tabs)
        {
            if (tab != null)
            {
                Destroy(tab.gameObject);
            }
        }

        _tabs.Clear();
    }

    // ---------------------------------------------------------------- overlays

    private SpriteRenderer SpawnOverlay(string name, Sprite sprite, Vector3 position, float worldSize, int order)
    {
        GameObject overlay = new GameObject(name);
        overlay.transform.position = position;

        SpriteRenderer renderer = overlay.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = order;

        if (sprite != null)
        {
            float scale = ScaleFor(sprite, worldSize);
            overlay.transform.localScale = new Vector3(scale, scale, 1f);
        }

        _overlays.Add(overlay);
        return renderer;
    }

    private void DestroyOverlay(ref SpriteRenderer renderer)
    {
        if (renderer == null)
        {
            return;
        }

        GameObject overlay = renderer.gameObject;
        _overlays.Remove(overlay);
        Destroy(overlay);
        renderer = null;
    }

    private void CleanupOverlays()
    {
        foreach (GameObject overlay in _overlays)
        {
            if (overlay != null)
            {
                Destroy(overlay);
            }
        }

        _overlays.Clear();
    }

    /// <summary>Uniform scale that makes a sprite <paramref name="worldSize"/> units across.</summary>
    private static float ScaleFor(Sprite sprite, float worldSize)
    {
        if (sprite == null)
        {
            return 1f;
        }

        float width = Mathf.Max(sprite.bounds.size.x, 0.0001f);
        return worldSize / width;
    }

    private void SetSky(Color color)
    {
        if (_sky != null)
        {
            _sky.color = color;
        }
    }

    /// <summary>
    /// Nudges both faces around their own random anchor. The anchor is passed in
    /// because an absolute jitter would drag them back to the centre of the screen,
    /// which defeated the random placement.
    /// </summary>
    private static void JitterWarningOverlays(
        SpriteRenderer idleRenderer,
        Vector3 idleBase,
        SpriteRenderer stopRenderer,
        Vector3 stopBase)
    {
        if (idleRenderer != null)
        {
            idleRenderer.transform.position = idleBase + new Vector3(
                Random.Range(-0.18f, 0.18f),
                Random.Range(-0.18f, 0.18f),
                0f);
        }

        if (stopRenderer != null)
        {
            stopRenderer.transform.position = stopBase + new Vector3(
                Random.Range(-0.18f, 0.18f),
                Random.Range(-0.18f, 0.18f),
                0f);
            stopRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-6f, 6f));
            stopRenderer.enabled = Mathf.Repeat(Time.time * 6f, 1f) > 0.15f;
        }
    }

    private Vector3 RandomScreenPoint(float margin)
    {
        return new Vector3(
            Random.Range(-ViewHalfWidth + margin, ViewHalfWidth - margin),
            Random.Range(-ViewHalfHeight + margin, ViewHalfHeight - margin),
            0f);
    }

    private static float ViewHalfWidth
    {
        get
        {
            Camera cam = Camera.main;
            return cam != null ? cam.orthographicSize * cam.aspect : 8.9f;
        }
    }

    private static float ViewHalfHeight
    {
        get
        {
            Camera cam = Camera.main;
            return cam != null ? cam.orthographicSize : 5f;
        }
    }

    // ---------------------------------------------------------------- HUD

    /// <summary>
    /// A click or any key press. The game scores on both (see clickable_behaviour),
    /// so the warning window has to watch for both as well.
    /// </summary>
    private static bool AnyInputThisFrame()
    {
        if (Input.GetMouseButtonDown(0))
        {
            return true;
        }

        return Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
    }

    private void OnGUI()
    {
        // The warning phase deliberately shows no text at all; the two faces carry it.
        if (_phase == Phase.Idle || _phase == Phase.Warning)
        {
            return;
        }

        if (_hudStyle == null)
        {
            _hudStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 34,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }

        float width = Screen.width;
        string message;

        switch (_phase)
        {
            case Phase.Jumpscare:
                message = "DOWNLOADING...";
                break;
            case Phase.Collect:
                message = $"COINS {_collected}/{Mathf.Max(1, _coinsToCollect)}\nTIME {_hudTimeLeft:0.0}";
                break;
            default:
                message = _collected >= Mathf.Max(1, _coinsToCollect) ? "THANK YOU" : "RANSOM PAID";
                break;
        }

        _hudStyle.normal.textColor = _phase == Phase.Collect ? Color.white : new Color(1f, 0.25f, 0.2f);
        GUI.Label(new Rect(0f, Screen.height * 0.06f, width, Screen.height * 0.25f), message, _hudStyle);
    }
}
