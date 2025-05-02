using System.Collections;
using UnityEngine;

// AttackPatternSO自体は実行ロジックの骨組みを提供する抽象クラスとする
public abstract class AttackPatternSO : ScriptableObject
{
  [Header("共通設定")]
  public GameObject bulletPrefab; // 使用する弾のプレハブ
  public float delayBetweenLoops = 1.0f; // この攻撃パターンを繰り返す際の間隔

  /// <summary>
  /// 攻撃パターンを実行するコルーチンを開始するメソッド。
  /// EnemyAttackerから呼び出される。
  /// </summary>
  /// <param name="attacker">攻撃を実行する敵の MonoBehaviour</param>
  /// <param name="firePoint">弾の発射基点</param>
  /// <returns>実行中のコルーチンを返す。EnemyAttackerはこれを管理する</returns>
  public Coroutine StartAttackSequence(MonoBehaviour attacker, Transform firePoint)
  {
    // attackerのStartCoroutineを使って、このSOに定義された実行ロジックを開始する
    return attacker.StartCoroutine(AttackSequenceCoroutine(attacker, firePoint));
  }

  /// <summary>
  /// 具体的な攻撃パターンのロジックを記述するコルーチン。
  /// このメソッドを継承クラスで実装する。
  /// </summary>
  /// <param name="attacker">攻撃者</param>
  /// <param name="firePoint">発射基点</param>
  protected abstract IEnumerator AttackSequenceCoroutine(
      MonoBehaviour attacker,
      Transform firePoint
  );

  /// <summary>
  /// (ユーティリティ) 弾を発射するヘルパーメソッド
  /// </summary>
  protected void FireBullet(
      Transform firePoint,
      Vector3 direction,
      float speed,
      object additionalData = null
  )
  {
    GameObject bulletGO = SimpleObjectPooler.Instance.GetBullet(bulletPrefab); // プレハブを指定して取得（要プーラー修正）
    if (bulletGO != null)
    {
      IBulletBehavior bulletBehavior = bulletGO.GetComponent<IBulletBehavior>();
      if (bulletBehavior != null)
      {
        // Setup内で SetActive(true) される想定
        bulletBehavior.Setup(firePoint.position, direction, speed, additionalData);
      }
      else
      {
        Debug.LogError(
            $"Bullet prefab '{bulletPrefab.name}' does not have IBulletBehavior!",
            bulletPrefab
        );
        SimpleObjectPooler.Instance.ReturnBullet(bulletGO); // 使えない弾は戻す
      }
    }
    else
    {
      Debug.LogWarning(
          "Could not get bullet from pool. Pool might be empty or not handling this prefab type."
      );
    }
  }
}
