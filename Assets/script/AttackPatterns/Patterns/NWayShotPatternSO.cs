// NWayShotPatternSO.cs (修正版 - 1回実行して終了)
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NWayShotPattern", menuName = "ScriptableObjects/Attack Patterns/N-Way Shot")]
public class NWayShotPatternSO : AttackPatternSO
{
  [Header("N-Wayショット設定")]
  public float bulletSpeed = 5f;
  public int numberOfWays = 5;
  public float totalAngle = 60f;
  public int burstCount = 3;
  public float delayBetweenBursts = 0.1f;
  // ★ delayBetweenLoops はこのSO内では使われなくなります (EnemyAttackerのdelayBetweenPatternsが役割を担う)

  protected override IEnumerator AttackSequenceCoroutine(MonoBehaviour attacker, Transform firePoint)
  {
    // while ループを削除し、攻撃アクションを1回だけ実行する

    // attacker が有効かどうかのチェックは最初に一度行う（任意）
    if (attacker == null || !attacker.isActiveAndEnabled)
    {
      yield break; // 開始時点で無効なら何もしない
    }

    // --- 1回のバースト射撃を実行 ---
    float angleStep = (numberOfWays > 1) ? totalAngle / (numberOfWays - 1) : 0f;
    float startAngle = (numberOfWays > 1) ? -totalAngle / 2f : 0f;

    for (int b = 0; b < burstCount; b++)
    {
      // attacker のチェックをループ内に入れるとより安全
      if (attacker == null || !attacker.isActiveAndEnabled) yield break;

      for (int i = 0; i < numberOfWays; i++)
      {
        float currentAngle = startAngle + (angleStep * i);
        Vector3 direction = Quaternion.AngleAxis(currentAngle, Vector3.forward) * attacker.transform.up;
        FireBullet(firePoint, direction, bulletSpeed); // 基底クラスのFireBullet呼び出しに変更した方が良いかも
      }

      if (b < burstCount - 1 && delayBetweenBursts > 0)
      {
        // attacker チェック
        if (attacker == null || !attacker.isActiveAndEnabled) yield break;
        yield return new WaitForSeconds(delayBetweenBursts);
      }
    }
    // --- バースト射撃 終了 ---

    // 攻撃パターン内の繰り返し (while) は無くなったので、
    // このコルーチンはここで自然に終了します。

    // ★このパターンSO内での delayBetweenLoops 待機は削除

    // (任意) デバッグ用に終了ログ
    // Debug.Log($"NWayShotPatternSO Sequence Finished for {attacker.name}");
  }

  // 基底クラスに firePoint 引数版がある前提。もしなければ下記を使う。
  protected void FireBullet(Transform firePoint, Vector3 direction, float speed)
  {
    base.FireBullet(firePoint, direction, speed, null); // 基底のヘルパーを呼び出す
  }

  /* もし spawnPosition指定版のFireBulletしかない場合はこちらを定義
   protected void FireBullet(Transform firePoint, Vector3 direction, float speed)
   {
       if (bulletPrefab == null) { Debug.LogError("Bullet Prefab not set!", this); return; }
       GameObject bulletGO = SimpleObjectPooler.Instance.GetBullet(bulletPrefab);
       if (bulletGO != null)
       {
           IBulletBehavior bulletBehavior = bulletGO.GetComponent<IBulletBehavior>();
           if (bulletBehavior != null)
           {
               // 直進弾のSetupを呼び出す
               bulletBehavior.Setup(firePoint.position, direction, speed, null);
           } else { /* エラー処理 */
  // }
  //          } else { /* エラー処理 */ }
  //      }
  //      */
}
