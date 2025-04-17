// Assets/Scripts/Patterns/BarragePatternData.cs
using System.Collections;
using UnityEngine;

// ScriptableObjectを作成するためのメニュー項目を追加
[CreateAssetMenu(fileName = "NewBarragePattern", menuName = "Gameplay/Barrage Pattern Data")]
public abstract class BarragePatternData : ScriptableObject
{
    // 全パターン共通で使いそうな設定
    public GameObject bulletPrefab; // 使用する弾のプレハブ
    public float bulletSpeed = 5f;
    public float bulletLifetime = 3f;

    // public AudioClip fireSound; // 効果音など

    // このパターンを実行する抽象メソッド (派生クラスで具体的な処理を実装)
    // 引数には発射位置などの情報が必要
    // コルーチンで実行することが多いので IEnumerator を返す設計も一般的
    public abstract IEnumerator ExecutePattern(Transform firePoint, SimpleObjectPooler pooler);

    // --- ヘルパー関数 (共通で使う発射処理など) ---
    // 派生クラスから使えるように protected または public にする
    protected void ShootSingleBullet(
        SimpleObjectPooler pooler,
        Transform firePoint,
        Vector3 direction,
        float speed,
        float lifetime
    )
    {
        GameObject bulletGO = pooler.GetBullet();
        if (bulletGO != null)
        {
            // ここで指定されたプレハブを使うように調整が必要かもしれない
            // (現状 pooler は1種類しか扱えないため、複数弾種対応には pooler の改造も必要)
            // ※ 今回は PatternData 側の bulletPrefab は一旦無視して Pooler のを使用

            bulletGO.transform.position = firePoint.position;
            bulletGO.transform.up = direction.normalized;

            BulletController bc = bulletGO.GetComponent<BulletController>();
            if (bc != null)
            {
                bc.speed = speed;
                bc.lifetime = lifetime;
            }
            bulletGO.SetActive(true);
        }
    }
}
