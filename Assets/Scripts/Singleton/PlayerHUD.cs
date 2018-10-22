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

    [Header("Fuel Pack")]
    [SerializeField] GameObject fuelPackHUD;
    [SerializeField] Image shields;
    [SerializeField] Image damage;
    [SerializeField] Image fuel;
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

    bool animateDamage;
    bool fadeEnergySplash;
    bool fadeDamageSplash;
    bool fadeInfoPrompt;

    void Awake() {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    void Start () {
		if (!crosshair) Debug.LogError("PlayerHUD: Crosshair not set");
        if (!usePrompt) Debug.LogError("PlayerHUD: Use Prompt not set");
        if (!fuelPackHUD) Debug.LogError("PlayerHUD: Fuel Pack HUD not set");
        if (!infoPrompt) Debug.LogError("PlayerHUD: Info Prompt not set");

        ResetHUD();
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

    public void ToggleCrosshair(bool show) {
        if (show) crosshair.enabled = true;
        else crosshair.enabled = false;
    }

    public void ToggleUsePrompt(bool show) {
        if (show) usePrompt.SetActive(true);
        else usePrompt.SetActive(false);
    }

    public void ToggleDematPrompt(bool show) {
        if (show) dematPrompt.SetActive(true);
        else dematPrompt.SetActive(false);
    }

    public void ToggleShipRadar(bool show) {
        if (show) shipRadar.SetActive(true);
        else shipRadar.SetActive(false);
    }

    public void TogglePlanetRadar(bool show) {
        if (show) planetRadar.SetActive(true);
        else planetRadar.SetActive(false);
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

    public void EnableFuelPackHUD(FuelPack fuelPack) {
        fuel.fillAmount = fuelPack.GetFuelPercentage();
        shields.fillAmount = fuelPack.GetShieldPercentage();
        damage.fillAmount = shields.fillAmount;
        for (int i = 0; i < fuelPack.GetChargedCells(); i++) chargedCells[i].enabled = true;
        for (int i = 0; i < fuelPack.GetAvailableCells(); i++) availableCells[i].enabled = true;

        fuelPackHUD.SetActive(true);
    }

    public void DisableFuelPackHUD() {
        foreach (Image cell in chargedCells) cell.enabled = false;
        foreach (Image cell in availableCells) cell.enabled = false;

        fuelPackHUD.SetActive(false);
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

    public void UpdateFuelGauge(float fuelPercentage) {
        fuel.fillAmount = fuelPercentage;
    }

    public void ResetHUD() {
        DisableFuelPackHUD();
        ToggleCrosshair(false);
        TogglePlanetRadar(false);
        ToggleDematPrompt(false);
        ToggleUsePrompt(false);
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
