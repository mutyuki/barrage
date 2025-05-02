// StarFormationTargetingPatternSO.cs (修正版 - 1回実行して終了)
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "StarTargetingPattern", menuName = "ScriptableObjects/Attack Patterns/Star Formation Targeting")]
public class StarFormationTargetingPatternSO : AttackPatternSO
{
  [Header("星形 設定")]
  public int points = 5;
  public int bulletsPerEdge = 3;
  public float radius = 2.0f;
  public float formationOffsetZ = 1.0f;
  public float initialRotation = 90f;
  public float spinOnFire = 30f; // 発射時に加える回転角度（毎秒ではなく、1回あたり）

  [Header("弾 設定")]
  public float bulletSpeed = 5f;

  // --- 内部変数 ---
  private Transform playerTransform = null;
  private float currentFormationRotation = 0f; // 星形の向きを保持・累積させる変数

  /// <summary>
  /// 攻撃シーケンスのメインコルーチン (1回実行)
  /// </summary>
  protected override IEnumerator AttackSequenceCoroutine(MonoBehaviour attacker, Transform firePoint)
  {
    // whileループを削除

    // 最初に attacker と Player の状態をチェック
    if (attacker == null || !attacker.isActiveAndEnabled) yield break;
    FindPlayer(); // プレイヤーを探す

    // --- 現在のプレイヤー位置を取得 ---
    Vector3 currentTargetPosition;
    if (playerTransform != null && playerTransform.gameObject.activeInHierarchy)
    {
      currentTargetPosition = playerTransform.position;
    }
    else
    {
      // プレイヤーがいない場合はこのパターンを実行しない
      Debug.LogWarning("Player not found or inactive. Skipping star pattern fire for this loop.", attacker);
      yield break; // このパターンの実行を中断して終了
    }

    // --- 星形配置と発射方向計算 ---
    int totalBullets = points * (bulletsPerEdge + 1);
    if (totalBullets <= 0) yield break;

    Debug.Log($"Firing Star Targeting Pattern ({totalBullets} bullets) towards {currentTargetPosition}", attacker);

    // 発射時の回転を適用 (前回からの累積 + 今回の回転)
    currentFormationRotation += spinOnFire; // 固定角度を毎回加算
                                            // または Random.Range(-spinRange, spinRange) のように毎回ランダムに回転させても良い
    currentFormationRotation = Mathf.Repeat(currentFormationRotation, 360f);

    Vector3 formationCenter = firePoint.position + attacker.transform.up * formationOffsetZ;
    int[] starOrder = { 0, 2, 4, 1, 3 };

    for (int p = 0; p < points; p++)
    {
      // 再度attackerチェック（任意だがより安全）
      if (attacker == null || !attacker.isActiveAndEnabled) yield break;

      int currentVertexIndex = starOrder[p];
      int nextVertexIndex = starOrder[(p + 1) % points];

      // 今回の星形の向きで頂点計算
      float angle1 = initialRotation + currentFormationRotation + (360f / points) * currentVertexIndex;
      float angle2 = initialRotation + currentFormationRotation + (360f / points) * nextVertexIndex;

      Vector3 vertexPos1 = formationCenter + (Quaternion.AngleAxis(angle1, Vector3.forward) * attacker.transform.up * radius);
      Vector3 vertexPos2 = formationCenter + (Quaternion.AngleAxis(angle2, Vector3.forward) * attacker.transform.up * radius);

      for (int i = 0; i <= bulletsPerEdge; i++)
      {
        float t = (float)i / (bulletsPerEdge + 1);
        Vector3 spawnPosition = Vector3.Lerp(vertexPos1, vertexPos2, t);

        Vector3 direction = (currentTargetPosition - spawnPosition).normalized;
        if (direction == Vector3.zero) direction = attacker.transform.up; // ゼロ除算回避

        FireBullet(spawnPosition, direction, bulletSpeed, null);

        // (任意) ずらして発射
        // if (attacker == null || !attacker.isActiveAndEnabled) yield break;
        // yield return new WaitForSeconds(0.005f);
      }
    }

    // --- 1回の星形発射完了 ---

    // このパターン固有の待機(delayBetweenLoops)は不要になったので削除
    // 待機は EnemyAttacker の delayBetweenPatterns が担当する

    // Debug.Log($"StarFormationTargetingPatternSO Sequence Finished for {attacker.name}");

    // ここでコルーチンが自然に終了する
  }

  // プレイヤー検索ロジック (変更なし)
  private void FindPlayer()
  {
    if (playerTransform == null || !playerTransform.gameObject.activeInHierarchy)
    {
      GameObject playerObj = GameObject.FindWithTag("Player");
      playerTransform = (playerObj != null) ? playerObj.transform : null;
      // if (playerTransform != null) Debug.Log("Player found for targeting.", this);
    }
  }

  // FireBullet ヘルパー (変更なし、基底クラスまたは前回定義したものを使う)
  protected void FireBullet(Vector3 spawnPos, Vector3 direction, float speed, object additionalData)
  {
    if (bulletPrefab == null) { /* エラー */ return; }
    GameObject bulletGO = SimpleObjectPooler.Instance.GetBullet(bulletPrefab);
    if (bulletGO != null)
    {
      IBulletBehavior bulletBehavior = bulletGO.GetComponent<IBulletBehavior>();
      if (bulletBehavior != null)
      {
        bulletBehavior.Setup(spawnPos, direction, speed, additionalData);
      }
      else { /* エラー */ SimpleObjectPooler.Instance.ReturnBullet(bulletGO); }
    }
    else { /* エラー */ }
  }
}
