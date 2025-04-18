using System.Collections;
using UnityEngine;

[CreateAssetMenu(
    fileName = "RotatingCirclePattern",
    menuName = "Gameplay/Barrage Patterns/Rotating Circle"
)] // ★ 名前変更推奨
public class CirclePatternData : BarragePatternData // ★ クラス名も変更推奨
{
    [Header("Circle Specific")]
    public int bulletCount = 12; // 1周あたりの弾数
    public float initialAngleOffset = 0f; // ★ 最初のオフセット（ループで変わる前の値）
    public float angleIncrementPerLoop = 33f; // ★ 1ループごとの角度増分

    [Header("Loop Settings")]
    public int attackLoopCount = 3; // ループ回数
    public float timeBetweenLoops = 0.1f; // ループ間の待機時間 (秒)

    // 基底クラスの抽象メソッドを実装
    public override IEnumerator ExecutePattern(Transform firePoint, AdvancedObjectPooler pooler)
    {
        if (bulletCount <= 0 || attackLoopCount <= 0)
        {
            yield break; // 弾数かループ数が0なら何もしない
        }

        // 現在のループにおける角度オフセット
        float currentLoopOffset = initialAngleOffset;

        // 攻撃ループ
        for (int loop = 0; loop < attackLoopCount; loop++)
        {
            // 円形発射
            float angleStep = 360f / bulletCount;
            for (int i = 0; i < bulletCount; i++)
            {
                // 現在のオフセットを考慮した角度を計算
                float currentAngle = angleStep * i + currentLoopOffset;
                float rad = currentAngle * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(Mathf.Sin(rad), Mathf.Cos(rad), 0); // Y軸上が0度

                // ★ 修正: 基底クラスのヘルパー関数を使って弾を発射
                //    第2引数に使用するプレハブ (このアセットが持つもの) を指定
                ShootSingleBullet(
                    pooler,
                    this.bulletPrefab, // ★ このアセットのプレハブを使用
                    firePoint,
                    direction,
                    this.bulletSpeed // ★ このアセットの速度を使用
                // this.bulletLifetime // IBullet実装側で管理するなら不要
                );
            }

            // 次のループのためのオフセット更新
            currentLoopOffset += angleIncrementPerLoop;

            // 最後のループの後以外は待機
            if (loop < attackLoopCount - 1)
            {
                if (timeBetweenLoops > 0)
                {
                    yield return new WaitForSeconds(timeBetweenLoops);
                }
                else
                {
                    yield return null; // ゼロ待機でもフレームを挟む
                }
            }
        }
    }
}
