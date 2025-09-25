using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 弾に関する設定データ
/// </summary>
[System.Serializable]
public class BulletData
{
    public string bulletName;
    public string addressableKey;
    public float speed; // 弾の基本速度
    public float power; // 弾の威力
    public int durability; // 耐久力
    public int scoreValue; // スコア
    // ミサイル固有のパラメータ
    public float homingDuration; // 誘導時間
    public float maxTurnSpeed; //誘導最大角度
}

/// <summary>
/// プレイヤーに関する設定データ
/// </summary>
[System.Serializable]
public class PlayerData
{
    public string addressableKey;
    public string bulletName; // プレイヤーが撃つ弾の名前
    public float speed; // 移動速度
    public float slowMagnification;// 減速率(1でそのまま、0で動けなくなる)
    public float fireRate;// 弾の発射間隔
    public int life;// 残機
    public float invincibleTime;// 無敵時間
    public int bombCount;// ボムの使用回数
    public Vector3 spawnPosition;// スポーン位置
}

/// <summary>
/// ボスの部位に関する設定データ
/// </summary>
[System.Serializable]
public class BossPartData
{
    public string addressableKey;
    public string bulletName; // 部位が撃つ弾の名前
    public float maxHp;// 最大体力
    public float fireRate;// 弾の発射間隔
    public float moveSpeed; // 移動速度
    public float moveDistanceY;// 縦方向の移動幅
    public float moveDistanceX;// 横方向の移動幅
    public Vector3 entryTargetPosition;// 登場演出先の地点
}

/// <summary>
/// ボスに関する設定データ
/// </summary>
[System.Serializable]
public class BossData
{
    public string addressableKey;
    public float maxHp;// 最大体力
    public float moveSpeed;// 移動速度
    public float moveDistance;// 縦方向の移動幅
    public Vector3 entryTargetPosition;// 登場演出先の地点
    public Vector3 initialPosition;// 生成地点
    public List<BossPartData> parts;
    public List<PhaseData> phases;
}

/// <summary>
/// ボスのフェーズ（行動パターン）に関する設定データ
/// </summary>
[System.Serializable]
public class PhaseData
{
    public string phaseName;
    public List<AttackCommand> attackSequence;
}

/// <summary>
/// ボスの攻撃パターンに関する設定データ
/// </summary>
[System.Serializable]
public class AttackCommand
{
    public string commandType;
    
    // WAIT
    public float duration;

    // SHOOT系コマンド共通
    public string bulletName; // どの弾を撃つか名前で指定
    
    // プレイヤーに向かって弾を撃つ
    public int count;
    public float interval;

    // プレイヤー側に向かって放射上に撃つ
    public float spreadAngle;

    // MOVE
    public Vector3 targetPosition;
}