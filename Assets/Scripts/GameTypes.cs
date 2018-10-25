//
// GameTypes.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Holds various game types
//

public class GameTypes {
	public enum ModuleType {
        FuelPack,
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
        Pulse,
        Gel,
        Fire,
        Ice
    };

    public enum PlayerWeaponType {
        MatterManipulator,
        MultiCannon,
        None
    };

    public enum XromPartType {
        Appendage,
        Vital,
        Separator
    };
}
