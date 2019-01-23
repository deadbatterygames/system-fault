//
// GameTypes.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Holds various game types
//

public class GameTypes {
	public enum ModuleType {
        EnergyPack,
        Thrusters,
        Boosters,
        QuantumDrive,
        LaserCannon,
        PulseCannon,
        Railgun,
        MissileRack
    };

    public enum AssistMode {
        NoAssist,
        Hover,
        Astro,
        Quantum
    };

    public enum DamageType {
        Yellow,
        Blue,
        Red,
        Physical
    };

    public enum PlayerWeaponType {
        MatterManipulator,
        Multicannon,
        None
    };

    public enum XromPartType {
        Appendage,
        Vital,
        Separator
    };

    public enum SpawnLocation {
        Moon,
        Earth,
        Gas
    }
}
