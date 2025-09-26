using System;

/// <summary>
/// ゲーム内のUI更新イベントを管理する静的クラス
/// </summary>
public static class GameEvents
{
    /// <summary>
    /// スコアが更新された時に発行されるイベント
    /// </summary>
    public static Action<int> OnScoreUpdated;

    /// <summary>
    /// 残機が更新された時に発行されるイベント
    /// </summary>
    public static Action<int> OnLifeUpdated;

    /// <summary>
    /// ボスのHPが更新された時に発行されるイベント (引数: 現在HP, 最大HP)
    /// </summary>
    public static Action<float, float> OnBossHpUpdated;
}