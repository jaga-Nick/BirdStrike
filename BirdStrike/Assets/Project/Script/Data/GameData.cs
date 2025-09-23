using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BulletData
{
    public string bulletName;
    public string addressableKey;
    public float speed; // 弾の基本速度
    public float power;
    public int durability;
    public int scoreValue;
    // ミサイル固有のパラメータ
    public float homingDuration;
    public float maxTurnSpeed;
}

[System.Serializable]
public class PlayerData
{
    public string addressableKey;
    public string bulletName; // プレイヤーが撃つ弾の名前
    public float speed;
    public float slowMagnification;
    public float fireRate;
    public int life;
    public float invincibleTime;
    public int bombCount;
}

[System.Serializable]
public class BossPartData
{
    public string addressableKey;
    public string bulletName; // 部位が撃つ弾の名前
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
    public string phaseName;
    public List<AttackCommand> attackSequence;
}

[System.Serializable]
public class AttackCommand
{
    public string commandType;
    
    // WAIT
    public float duration;

    // SHOOT系コマンド共通
    public string bulletName; // どの弾を撃つか名前で指定
    
    // SHOOT_RADIAL / SHOOT_TARGET
    public int count;
    public float interval;

    // SHOOT_RADIAL
    public float spreadAngle;

    // MOVE
    public Vector3 targetPosition;
}