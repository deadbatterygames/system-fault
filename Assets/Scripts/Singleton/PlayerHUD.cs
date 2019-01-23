using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//
// PlayerHUD.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Toggles various HUD elements
//

public class PlayerHUD : MonoBehaviour {

    public static PlayerHUD instance = null;

    [Header("General")]
    [SerializeField] Image crosshair;
    [SerializeField] GameObject usePrompt;
    [SerializeField] GameObject dematPrompt;
    [SerializeField] float damageDelay = 0.5f;
    [SerializeField] float damageDrainSpeed = 1f;

    [Space]
    [SerializeField] CanvasGroup energySplash;
    [SerializeField] CanvasGroup damageSplash;
    [SerializeField] float splashFadeSpeed = 1f;
    [SerializeField] CanvasGroup infoPrompt;
    [SerializeField] float infoDisplayTime = 3f;
    [SerializeField] float infoFadeSpeed = 0.5f;

    [Header("Energy Pack")]
    [SerializeField] GameObject energyPackHUD;
    [SerializeField] Image shields;
    [SerializeField] Image damage;
    [SerializeField] Image energy;
    [SerializeField] Image[] availableCells;
    [SerializeField] Image[] chargedCells;

    [Header("Radar")]
    [SerializeField] GameObject shipRadar;
    [SerializeField] Image shipLeft;
    [SerializeField] Image shipRight;
    [SerializeField] GameObject planetRadar;
    [SerializeField] Image planetLeft;
    [SerializeField] Image planetRight;
    [SerializeField] Image planetUp;
    [SerializeField] Image planetDown;

    [Header("Help")]
    [SerializeField] bool showHelp;
    [SerializeField] GameObject playerHelp;
    [SerializeField] GameObject shipHelp;
    [SerializeField] GameObject printerHelp;
    GameObject[] helpObjects;

    bool animateDamage;
    bool fadeEnergySplash;
    bool fadeDamageSplash;
    bool fadeInfoPrompt;

    Text useText;
    Text dematText;

    void Awake() {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    void Start () {
		if (!crosshair) Debug.LogError("PlayerHUD: Crosshair not set");
        if (!usePrompt) Debug.LogError("PlayerHUD: Use Prompt not set");
        if (!energyPackHUD) Debug.LogError("PlayerHUD: Energy Pack HUD not set");
        if (!infoPrompt) Debug.LogError("PlayerHUD: Info Prompt not set");

        useText = usePrompt.GetComponentInChildren<Text>();
        dematText = dematPrompt.GetComponentInChildren<Text>();

        helpObjects = GameObject.FindGameObjectsWithTag("UIHelp");

        ClearHUD();
        infoPrompt.alpha = 0;
    }

    void Update() {
        if (animateDamage) {
            damage.fillAmount = Mathf.MoveTowards(damage.fillAmount, shields.fillAmount, damageDrainSpeed * Time.deltaTime);
            if (damage.fillAmount == shields.fillAmount) animateDamage = false;
        }

        if (fadeEnergySplash) {
            energySplash.alpha -= splashFadeSpeed * Time.deltaTime;
            if (energySplash.alpha <= 0) fadeEnergySplash = false;
        }
        if (fadeDamageSplash) {
            damageSplash.alpha -= splashFadeSpeed * Time.deltaTime;
            if (damageSplash.alpha <= 0) fadeDamageSplash = false;
        }
        if (fadeInfoPrompt) {
            infoPrompt.alpha -= infoFadeSpeed * Time.deltaTime;
            if (infoPrompt.alpha <= 0) fadeInfoPrompt = false;
        }

        if (Input.GetKeyDown(KeyCode.Slash)) {
            showHelp = !showHelp;
            foreach (GameObject helpObject in helpObjects) helpObject.SetActive(showHelp);
        }
    }

    public void ShowEnergySplash() {
        energySplash.alpha = 1f;
        fadeEnergySplash = true;
    }

    public void ShowDamageSplash() {
        damageSplash.alpha = 1f;
        fadeDamageSplash = true;
    }

    public void SetInfoPrompt(string prompt) {
        infoPrompt.GetComponentInChildren<Text>().text = prompt;
        fadeInfoPrompt = false;
        StopCoroutine("ShowInfoPrompt");
        StartCoroutine("ShowInfoPrompt");
    } 

    public void ToggleCrosshair(bool toggle) {
        crosshair.enabled = toggle;
    }

    public void ToggleUsePrompt(bool toggle, string message = "") {
        if (toggle) useText.text = "Use " + message;
        usePrompt.SetActive(toggle);
    }

    public void ToggleDematPrompt(bool toggle, string message = "") {
        if (toggle) dematText.text = "Dematerialize\n" + message;
        dematPrompt.SetActive(toggle);
    }

    public void ToggleShipRadar(bool toggle) {
        shipRadar.SetActive(toggle);
    }

    public void TogglePlanetRadar(bool toggle) {
        planetRadar.SetActive(toggle);
    }

    public void TogglePlayerHelp(bool toggle) {
        playerHelp.SetActive(toggle);
    }

    public void ToggleShipHelp(bool toggle) {
        shipHelp.SetActive(toggle);
    }

    public void TogglePrinterHelp(bool toggle) {
        printerHelp.SetActive(toggle);
    }

    public void UpdateShipRadar(int direction) {
        switch (direction) {
            case 1:
                shipLeft.enabled = false;
                shipRight.enabled = true;
                break;
            case -1:
                shipLeft.enabled = true;
                shipRight.enabled = false;
                break;
            case 0:
                shipLeft.enabled = true;
                shipRight.enabled = true;
                break;
            default:
                Debug.LogError("PlayerHUD: Invalid argument passed to UpdateShipRadar");
                break;
        }
    }

    public void UpdatePlanetRadar(int rightLeft, int upDown) {
        switch (rightLeft) {
            case 1:
                planetLeft.enabled = false;
                planetRight.enabled = true;
                break;
            case -1:
                planetLeft.enabled = true;
                planetRight.enabled = false;
                break;
            case 0:
                planetLeft.enabled = true;
                planetRight.enabled = true;
                break;
        }

        switch (upDown) {
            case 1:
                planetUp.enabled = true;
                planetDown.enabled = false;
                break;
            case -1:
                planetUp.enabled = false;
                planetDown.enabled = true;
                break;
            case 0:
                planetUp.enabled = true;
                planetDown.enabled = true;
                break;
        }
    }

    public void EnableEnergyPackHUD(EnergyPack energyPack) {
        energy.fillAmount = energyPack.GetEnergyPercentage();
        shields.fillAmount = energyPack.GetShieldPercentage();
        damage.fillAmount = shields.fillAmount;
        for (int i = 0; i < energyPack.GetChargedCells(); i++) chargedCells[i].enabled = true;
        for (int i = 0; i < energyPack.GetAvailableCells(); i++) availableCells[i].enabled = true;

        energyPackHUD.SetActive(true);
    }

    public void DisableEnergyPackHUD() {
        foreach (Image cell in chargedCells) cell.enabled = false;
        foreach (Image cell in availableCells) cell.enabled = false;

        energyPackHUD.SetActive(false);
    }

    public void AddShields(float shieldPercentage) {
        shields.fillAmount = shieldPercentage;
        if (!animateDamage && damage.fillAmount < shieldPercentage) damage.fillAmount = shieldPercentage;
    }

    public void DamageShields(float shiedPercentage) {
        shields.fillAmount = shiedPercentage;
        if (shields.fillAmount > damage.fillAmount) damage.fillAmount = 1f;

        StopCoroutine("ShowDamage");
        StartCoroutine("ShowDamage");
    }

    public void ChargeShieldCell(int cells) {
        damage.fillAmount = 0f;
        for (int i = 0; i < cells; i++) chargedCells[i].enabled = true;
    }

    public void RemoveShieldCell(int cells) {
        for (int i = cells; i < 3; i++) chargedCells[i].enabled = false;
    }

    public void UpdateEnergyGauge(float energyPercentage) {
        energy.fillAmount = energyPercentage;
    }

    public void ClearHUD() {
        DisableEnergyPackHUD();
        ToggleCrosshair(false);
        TogglePlanetRadar(false);
        ToggleDematPrompt(false);
        ToggleUsePrompt(false);
        TogglePrinterHelp(false);
        ToggleShipHelp(false);
        TogglePlayerHelp(false);
        infoPrompt.alpha = 0f;
    }

    IEnumerator ShowDamage() {
        animateDamage = false;
        yield return new WaitForSeconds(damageDelay);
        animateDamage = true;
    }

    IEnumerator ShowInfoPrompt() {
        infoPrompt.alpha = 1f;
        yield return new WaitForSeconds(infoDisplayTime);
        fadeInfoPrompt = true;
    } 
}
