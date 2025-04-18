using System.Collections;
using UnityEngine;

[CreateAssetMenu(
    fileName = "AimedBurstLoopPattern", // 分かりやすい名前に変更推奨
    menuName = "Gameplay/Barrage Patterns/Aimed Burst Loop" // メニュー名も変更推奨
)]
public class AimedPatternData : BarragePatternData // クラス名も変えた方が良いかも？
{
    [Header("Aimed Specific")]
    public string targetTag = "Player";

    [Header("Burst & Loop Settings")]
    [Tooltip("1回のバーストで発射する弾数")]
    public int bulletsPerBurst = 3; // 名前を bulletCount から変更推奨

    [Tooltip("バースト内の弾の発射間隔 (秒)")]
    public float timeBetweenBullets = 0.1f;

    [Tooltip("バーストを繰り返す回数")]
    public int attackLoopCount = 3;

    [Tooltip("次のバーストまでの待機時間 (秒)")]
    public float timeBetweenLoops = 0.5f; // 名前を timeBetweenLoops に変更

    public override IEnumerator ExecutePattern(Transform firePoint, AdvancedObjectPooler pooler)
    {
        // ターゲットの検索はループの外で行う (毎回探す必要はないかもしれない)
        GameObject target = GameObject.FindWithTag(targetTag);
        Vector3 lastKnownDirection = firePoint.up; // ターゲットが見つからない場合や最初のループ用の方向

        if (target == null)
        {
            Debug.LogWarning($"Target with tag '{targetTag}' not found initially. Firing forward.");
        }

        // 攻撃ループ
        for (int loop = 0; loop < attackLoopCount; loop++)
        {
            // ループごとにターゲット方向を再計算する (ターゲットが動く場合)
            if (target != null)
            {
                lastKnownDirection = (target.transform.position - firePoint.position).normalized;
                // Debug.Log($"Loop {loop + 1}: Aiming at {lastKnownDirection}");
            }
            else
            {
                // ターゲットがいない場合は初期の前方方向を維持
                lastKnownDirection = firePoint.up;
            }

            // バースト発射
            for (int burst = 0; burst < bulletsPerBurst; burst++)
            {
                // ★ 修正: ShootSingleBullet を呼び出す。第2引数に発射するプレハブを指定。
                ShootSingleBullet(
                    pooler,
                    this.bulletPrefab, // ★ このScriptableObjectが持つ弾プレハブを使う
                    firePoint,
                    lastKnownDirection, // このループで狙う方向
                    this.bulletSpeed // このScriptableObjectの速度設定
                // 寿命(lifetime)は IBullet 実装側で管理される想定なので渡さなくても良いが、
                // ShootSingleBulletの定義に合わせて渡す。StraightBullet は Initialize で無視する想定
                );

                // バースト内の最後の弾でなければ待機
                if (burst < bulletsPerBurst - 1 && timeBetweenBullets > 0)
                {
                    yield return new WaitForSeconds(timeBetweenBullets);
                }
            }

            // 攻撃ループの最後のループでなければ待機
            if (loop < attackLoopCount - 1 && timeBetweenLoops > 0)
            {
                yield return new WaitForSeconds(timeBetweenLoops);
            }
            // 待機時間がゼロでも最低限フレームは進める
            else if (loop < attackLoopCount - 1)
            {
                yield return null;
            }
        }
    }
}
