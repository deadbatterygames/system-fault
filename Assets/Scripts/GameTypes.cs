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
        MissileRack
    }

    public enum AssistMode {
        NoAssist,
        Hover,
        Astro,
        Quantum
    }

    public enum DamageType {
        Yellow,
        Blue,
        Red,
        Purple,
        Physical
    }

    public enum PlayerWeaponType {
        None,
        MatterManipulator,
        Multicannon
    }

    public enum XromPartType {
        Appendage,
        Vital,
        Separator
    }

    public enum SpawnLocation {
        Moon,
        Earth,
        Gas
    }

    public enum EnergyState {
        Normal,
        Magnetize,
        Sleep
    }
}
