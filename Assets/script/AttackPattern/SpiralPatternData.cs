using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "SpiralPattern", menuName = "Gameplay/Barrage Patterns/Spiral")]
public class SpiralPatternData : BarragePatternData
{
    [Header("Spiral Specific")]
    [Tooltip("発射する総弾数")]
    public int bulletCount = 100;

    [Tooltip("1発あたりの回転角度 (度)")]
    public float angleStep = 10f; // ★ デフォルト値を設定 (0だと回転しない)

    [Tooltip("各弾の発射遅延 (秒)")]
    public float delayBetweenShots = 0.01f;

    [Tooltip("開始角度のオフセット (度)")]
    public float startAngleOffset = 0f;

    [Tooltip("回転方向（True:時計回り / False:反時計回り）")]
    public bool clockwise = true;

    public override IEnumerator ExecutePattern(Transform firePoint, AdvancedObjectPooler pooler)
    {
        // 弾数が0以下、または角度ステップが0で無限ループになるのを防ぐ (任意)
        if (bulletCount <= 0 || (angleStep == 0 && bulletCount > 1)) // angleStepが0でも1発だけなら撃てるように
        {
            if (bulletCount == 1 && angleStep == 0)
            {
                // angleStep 0 で bulletCount 1 の場合：正面に1発撃つ
                ShootSingleBullet(
                    pooler,
                    this.bulletPrefab,
                    firePoint,
                    firePoint.up,
                    this.bulletSpeed
                );
                yield break;
            }
            Debug.LogWarning(
                "SpiralPattern: bulletCount or angleStep is invalid for creating a spiral.",
                this
            );
            yield break;
        }

        // 回転方向係数 (-1 or 1)
        float rotationDirection = clockwise ? -1f : 1f;

        // 弾を指定数発射するループ
        for (int i = 0; i < bulletCount; i++)
        {
            // 現在の回転角度を計算
            float currentAngle = startAngleOffset + angleStep * i * rotationDirection;

            // 回転をQuaternionとして計算し、基準方向 (firePoint.up) に適用
            Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);
            Vector3 shootDirection = rotation * firePoint.up;

            // ★ 修正: 基底クラスの ShootSingleBullet を使用
            //    第2引数にこのアセットが指定する弾プレハブを渡す
            ShootSingleBullet(
                pooler,
                this.bulletPrefab, // ★ このアセットのプレハブを使用
                firePoint,
                shootDirection,
                this.bulletSpeed // ★ このアセットの速度を使用
            // this.bulletLifetime // IBullet実装側で管理
            );

            // 指定された遅延時間だけ待機
            if (delayBetweenShots > 0)
            {
                yield return new WaitForSeconds(delayBetweenShots);
            }
            else
            {
                // 遅延ゼロでも最低1フレーム待つと安全な場合がある
                yield return null;
            }
        }
        // ループ終了
    }
}
