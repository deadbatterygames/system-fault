using UnityEngine;
using UnityEngine.UI;

//
// EnergyPack.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Handles energy and player shields
//

public class EnergyPack : ShipModule {

    [SerializeField] float maxEnergy = 100f;
    [SerializeField] [Range(1, 3)] int shieldCells = 1;
    [SerializeField] float shieldChargeRate = 1f;
    [SerializeField] float shieldChargeCost = 1f;
    [SerializeField] Image energyGuage;

    const float SHIELDS_PER_CELL = 100f;

    float currentShields = 0f;
    int chargedCells = 0;

    float energy = 0f;

    protected override void Awake() {
        base.Awake();
        moduleType = GameTypes.ModuleType.EnergyPack;

        if (GameManager.instance.IsInTestMode()) AddEnergy(maxEnergy);
    }

    public void AddEnergy(float amount) {
        energy += amount;
        if (energy > maxEnergy) energy = maxEnergy;

        UpdateEnergyGauge();
        PlayerHUD.instance.UpdateEnergyGauge(GetEnergyPercentage());
    }

    public void DrainEnergy(float amount) {
        energy -= amount;
        if (energy < 0f) energy = 0f;

        UpdateEnergyGauge();
        PlayerHUD.instance.UpdateEnergyGauge(GetEnergyPercentage());
    }

    public void AddShields(float amount) {
        currentShields += amount;
        if (currentShields > SHIELDS_PER_CELL) {
            if (chargedCells < shieldCells) {
                chargedCells++;
                currentShields -= SHIELDS_PER_CELL;
                PlayerHUD.instance.ChargeShieldCell(chargedCells);
            } else currentShields = SHIELDS_PER_CELL;
        }

        PlayerHUD.instance.AddShields(GetShieldPercentage());
    }

    public void ChargeShields() {
        if (energy >= shieldChargeCost * shieldChargeRate * Time.deltaTime) {
            if (chargedCells < shieldCells || currentShields < SHIELDS_PER_CELL) {
                DrainEnergy(shieldChargeCost * shieldChargeRate * Time.deltaTime);
                PlayerHUD.instance.UpdateEnergyGauge(GetEnergyPercentage());

                AddShields(shieldChargeRate * Time.deltaTime);
            }
        } else PlayerHUD.instance.SetInfoPrompt("Not enough Energy to charge Shield Cell");
    }

    public float AbsorbDamage(float amount) {
        currentShields -= amount;
        float leftOverDamage = 0f;

        if (currentShields <= 0) {
            if (chargedCells > 0) {
                chargedCells--;
                currentShields += SHIELDS_PER_CELL;
                PlayerHUD.instance.RemoveShieldCell(chargedCells);
            } else {
                leftOverDamage = Mathf.Abs(currentShields);
                currentShields = 0f;
            }
        }

        PlayerHUD.instance.DamageShields(GetShieldPercentage());
        return leftOverDamage;
    }

    public void UpdateEnergyGauge() {
        energyGuage.fillAmount = energy / maxEnergy;
    }

    public float GetEnergy() {
        return energy;
    }

    public float GetEnergyPercentage() {
        return energy / maxEnergy;
    }

    public float GetShieldPercentage() {
        return currentShields / SHIELDS_PER_CELL;
    }

    public int GetChargedCells() {
        return chargedCells;
    }

    public int GetAvailableCells() {
        return shieldCells;
    }

    public bool IsEmpty() {
        return energy == 0f;
    }

    public bool IsFull() {
        return energy == maxEnergy;
    }

    public void ShowPack(bool show) {
        if (show) {
            GetComponent<MeshRenderer>().enabled = true;
            energyGuage.enabled = true;
        } else {
            GetComponent<MeshRenderer>().enabled = false;
            energyGuage.enabled = false;
        }
    }
}
