using System.Collections.Generic;
using UnityEngine; // Vector3のため

// このファイルに、JSON化したいデータをすべて定義していきます。

[System.Serializable]
public class PlayerData
{
    public string addressableKey;
    public float speed;
    public float slowMagnification; // 低速時の速度倍率
    public float fireRate;
    public int initialLife;
    public float invincibleTime;
}

[System.Serializable]
public class BossPartData
{
    public string addressableKey;
    public float maxHp;
    public float fireRate;
    public float moveSpeed; 
    public float moveDistance;
    public Vector3 entryTargetPosition;

}

[System.Serializable]
public class BossData
{
    public string addressableKey;
    public float maxHp;
    public float moveSpeed;
    public float moveDistance;
    public Vector3 entryTargetPosition;
    public Vector3 initialPosition;
    public List<BossPartData> parts;
    public List<PhaseData> phases;
}

[System.Serializable]
public class PhaseData
{
    public string phaseName; // "Normal", "Angry", "Serious"
    public List<AttackCommand> attackSequence;
}

[System.Serializable]
public class AttackCommand
{
    public string commandType; // "WAIT", "SHOOT_RADIAL", "SHOOT_TARGET", "MOVE", "ULTRA_ATTACK"
    
    // WAIT
    public float duration;

    // SHOOT
    public string bulletAddressableKey;
    public int count;
    public float spreadAngle;
    public float interval; // 弾を発射する間隔

    // MOVE
    public Vector3 targetPosition;
}