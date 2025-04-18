// 例: AimedPatternData.cs や SplitterPatternData.cs など
using System.Collections;
using UnityEngine;

public abstract class BarragePatternData : ScriptableObject
{
    // 共通設定 (各派生クラスで使う弾プレハブを設定)
    public GameObject bulletPrefab; // ★ 発射する弾のプレハブ
    public float bulletSpeed = 5f;
    public float bulletLifetime = 3f; // ★ 注意: このlifetimeはStraightBulletでは使われるが、SplitterBulletでは使われない設計になっている

    // 子クラスが実装するメソッド (AdvancedObjectPoolerを使う)
    public abstract IEnumerator ExecutePattern(Transform firePoint, AdvancedObjectPooler pooler);

    // ★ ShootSingleBullet ヘルパー関数を IBullet 対応に修正
    protected void ShootSingleBullet(
        AdvancedObjectPooler pooler,
        GameObject prefabToShoot,
        Transform firePoint,
        Vector3 direction,
        float speed
    )
    {
        if (pooler == null || prefabToShoot == null)
            return;

        GameObject bulletGO = pooler.GetObject(prefabToShoot); // ★ 使用するプレハブを指定
        if (bulletGO != null)
        {
            IBullet bulletInterface = bulletGO.GetComponent<IBullet>(); // ★ IBullet を取得
            if (bulletInterface != null)
            {
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
                bulletInterface.Initialize(firePoint.position, rotation, speed); // ★ Initialize を呼び出す
                bulletGO.SetActive(true); // Initialize の後にアクティブ化
            }
            else
            {
                Debug.LogError(
                    $"弾プレハブ {prefabToShoot.name} に IBullet 実装がありません！",
                    prefabToShoot
                );
                pooler.ReturnObject(bulletGO); // 使えないので戻す
            }
        }
        else
        {
            Debug.LogWarning($"弾 {prefabToShoot.name} の取得に失敗 (プール空か未登録?)");
        }
    }

    // (旧) ExecutePattern(Transform firePoint, SimpleObjectPooler pooler, float dynamicAngleOffset = 0f) は不要になる
}
