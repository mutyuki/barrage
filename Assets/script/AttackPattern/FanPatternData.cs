using System.Collections;
using UnityEngine;

[CreateAssetMenu(
    fileName = "FanLoopPattern",
    menuName = "Gameplay/Barrage Patterns/Fan (N-Way) Loop"
)] // ★ 名前変更推奨
public class FanPatternData : BarragePatternData // ★ クラス名も変更推奨
{
    [Header("Fan Specific")]
    public int bulletsPerFan = 5; // ★ 1回の扇形で発射する弾の数 (bulletCountから変更)
    public float totalAngle = 60f; // 扇の全体角度
    public float delayBetweenShots = 0f; // 扇形内の弾の発射遅延 (0なら同時)

    [Header("Loop Settings")]
    public int attackLoopCount = 3; // ループ回数
    public float timeBetweenLoops = 0.2f; // ★ ループ間の待機時間 (名前を明確化)

    // public bool reAimPerLoop = false;    // ★ (オプション) ループ毎に狙い直すか (プレイヤー依存になる)

    // 内部計算用変数はメソッド内で定義する方がスコープが明確
    // private float angleStep;
    // private float startAngle;

    public override IEnumerator ExecutePattern(Transform firePoint, AdvancedObjectPooler pooler)
    {
        // 弾数かループ数が0以下なら何もしない
        if (bulletsPerFan <= 0 || attackLoopCount <= 0)
        {
            yield break;
        }

        // 攻撃ループ
        for (int loop = 0; loop < attackLoopCount; loop++)
        {
            // 現在のfirePointの向きを基準方向とする
            // (オプション: reAimPerLoopがtrueならここでプレイヤー方向を向く処理を入れる)
            Vector3 baseDirection = firePoint.up;

            if (bulletsPerFan == 1)
            {
                // 1発のみの場合は基準方向(通常は正面)に発射
                ShootSingleBullet(
                    pooler,
                    this.bulletPrefab, // ★ 使用するプレハブを指定
                    firePoint,
                    baseDirection,
                    this.bulletSpeed
                // this.bulletLifetime // 不要かも
                );
            }
            else // 2発以上の場合 (扇形計算)
            {
                float angleStep = totalAngle / (bulletsPerFan - 1);
                float startAngle = -totalAngle / 2f;

                // 扇形内の各弾を発射
                for (int i = 0; i < bulletsPerFan; i++)
                {
                    float currentAngle = startAngle + angleStep * i;
                    Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);
                    Vector3 shootDirection = rotation * baseDirection; // 基準方向を回転

                    ShootSingleBullet(
                        pooler,
                        this.bulletPrefab, // ★ 使用するプレハブを指定
                        firePoint,
                        shootDirection,
                        this.bulletSpeed
                    // this.bulletLifetime // 不要かも
                    );

                    // 扇形内の待機
                    if (delayBetweenShots > 0 && i < bulletsPerFan - 1)
                    {
                        yield return new WaitForSeconds(delayBetweenShots);
                    }
                }
                // 同時発射の場合はここで最低1フレーム待機を入れると安全かも
                if (delayBetweenShots <= 0 && bulletsPerFan > 0)
                {
                    yield return null;
                }
            }

            // 最後のループの後以外はループ間待機
            if (loop < attackLoopCount - 1)
            {
                if (timeBetweenLoops > 0)
                {
                    yield return new WaitForSeconds(timeBetweenLoops);
                }
                else
                {
                    yield return null; // ゼロ待機でもフレーム挟む
                }
            }
        } // end attackLoop
    } // end ExecutePattern
}
