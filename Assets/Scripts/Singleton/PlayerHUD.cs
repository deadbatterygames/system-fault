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
    [SerializeField] Text usePrompt;
    [SerializeField] Text dematPrompt;
    [SerializeField] float damageDelay = 0.5f;
    [SerializeField] float damageDrainSpeed = 1f;

    [Space]
    [SerializeField] CanvasGroup energySplash;
    [SerializeField] CanvasGroup damageSplash;
    [SerializeField] float splashFadeSpeed = 1f;

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
    bool showEnergySplash;
    bool showDamageSplash;

    void Awake() {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    void Start () {
		if (!crosshair) Debug.LogError("PlayerHUD: Crosshair not set");
        if (!usePrompt) Debug.LogError("PlayerHUD: Use prompt not set");
        if (!fuelPackHUD) Debug.LogError("PlayerHUD: Fuel pack HUD not set");

        DisableFuelPackHUD();
        ToggleCrosshair(false);
        TogglePlanetRadar(false);
    }

    void Update() {
        if (animateDamage) {
            damage.fillAmount = Mathf.MoveTowards(damage.fillAmount, shields.fillAmount, damageDrainSpeed * Time.deltaTime);
            if (damage.fillAmount == shields.fillAmount) animateDamage = false;
        }

        if (showEnergySplash) {
            energySplash.alpha -= splashFadeSpeed * Time.deltaTime;
            if (energySplash.alpha <= 0) showEnergySplash = false;
        }
        if (showDamageSplash) {
            damageSplash.alpha -= splashFadeSpeed * Time.deltaTime;
            if (damageSplash.alpha <= 0) showDamageSplash = false;
        }
    }

    public void ShowEnergySplash() {
        energySplash.alpha = 1f;
        showEnergySplash = true;
    }

    public void ShowDamageSplash() {
        damageSplash.alpha = 1f;
        showDamageSplash = true;
    }

    public void ToggleCrosshair(bool show) {
        if (show) {
            crosshair.enabled = true;
        } else {
            crosshair.enabled = false;
        }
    }

    public void ToggleUsePrompt(bool show) {
        if (show) usePrompt.enabled = true;
        else usePrompt.enabled = false;
    }

    public void ToggleDematPrompt(bool show) {
        if (show) dematPrompt.enabled = true;
        else dematPrompt.enabled = false;
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

    IEnumerator ShowDamage() {
        animateDamage = false;
        yield return new WaitForSeconds(damageDelay);
        animateDamage = true;
    }
}
