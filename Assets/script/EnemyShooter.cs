// EnemyController.cs や EnemyShooter.cs を修正・作成
using System.Collections;
using System.Collections.Generic; // List を使う場合
using UnityEngine;

public class EnemyPatternController : MonoBehaviour
{
    public Transform firePoint;
    public List<BarragePatternData> attackPatterns; // 攻撃パターンのリスト
    public float attackInterval = 2f; // 次の攻撃までの間隔

    private AdvancedObjectPooler pooler;
    private int currentPatternIndex = 0;

    void Start()
    {
        pooler = AdvancedObjectPooler.Instance;
        if (pooler == null)
        {
            Debug.LogError("Pooler not found!");
            enabled = false;
            return;
        }
        if (firePoint == null)
        {
            Debug.LogError("FirePoint not set!");
            enabled = false;
            return;
        }
        if (attackPatterns == null || attackPatterns.Count == 0)
        {
            Debug.LogError("No Attack Patterns assigned!");
            enabled = false;
            return;
        }

        // 定期的に攻撃を開始するコルーチンを開始
        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        while (true) // 無限ループ（敵が生きている間）
        {
            // パターンリストから実行するパターンを選択（ここでは順番に実行）
            if (currentPatternIndex >= attackPatterns.Count)
            {
                currentPatternIndex = 0; // 最後まで行ったら最初に戻る
            }

            BarragePatternData currentPattern = attackPatterns[currentPatternIndex];

            if (currentPattern != null)
            {
                Debug.Log($"Executing Pattern: {currentPattern.name}");
                // 選択したパターンの実行メソッドを呼び出す (コルーチンで実行)
                yield return StartCoroutine(currentPattern.ExecutePattern(firePoint, pooler));
            }
            else
            {
                Debug.LogWarning($"Pattern at index {currentPatternIndex} is null.");
            }

            currentPatternIndex++; // 次のパターンへ

            // 次の攻撃までの待機時間
            yield return new WaitForSeconds(attackInterval);
        }
    }
}
