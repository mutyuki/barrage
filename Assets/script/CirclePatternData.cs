// Assets/Scripts/Patterns/CirclePatternData.cs
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "CirclePattern", menuName = "Gameplay/Barrage Patterns/Circle")]
public class CirclePatternData : BarragePatternData
{
  [Header("Circle Specific")]
  public int bulletCount = 12; // 弾の数
  public float startAngleOffset = 0f; // 開始角度オフセット

  // 基底クラスの抽象メソッドを実装
  public override IEnumerator ExecutePattern(Transform firePoint, SimpleObjectPooler pooler)
  {
    if (bulletCount <= 0)
      yield break; // 0発なら何もしない

    float angleStep = 360f / bulletCount;
    for (int i = 0; i < bulletCount; i++)
    {
      float currentAngle = angleStep * i + startAngleOffset;
      float rad = currentAngle * Mathf.Deg2Rad;
      Vector3 direction = new Vector3(Mathf.Sin(rad), Mathf.Cos(rad), 0); // Y軸が上(0度)とする

      // 基底クラスのヘルパー関数を使って弾を発射
      ShootSingleBullet(pooler, firePoint, direction, this.bulletSpeed, this.bulletLifetime);
    }
    // yield return null; // 瞬時に全発射する場合でもIEnumeratorにはyieldが必要
  }
}
