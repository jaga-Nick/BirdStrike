using System; // Actionを使うために必要

/// <summary>
/// ゲーム内のUI更新イベントを管理する静的クラス
/// </summary>
public static class GameEvents
{
    // スコアが更新された時に発行されるイベント
    public static Action<int> OnScoreUpdated;

    // 残機が更新された時に発行されるイベント
    public static Action<int> OnLifeUpdated;

    // ボスのHPが更新された時に発行されるイベント (引数: 現在HP, 最大HP)
    public static Action<float, float> OnBossHpUpdated;
}