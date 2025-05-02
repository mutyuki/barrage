using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttacker : MonoBehaviour
{
  [Header("攻撃パターン設定")]
  [SerializeField]
  private List<AttackPatternSO> attackPatterns; // 実行する攻撃パターンのリスト

  [SerializeField]
  private bool loopSequence = true; // パターンリストをループ実行するか

  [SerializeField]
  private float delayBetweenPatterns = 1.0f; // 各パターン間の待機時間 (秒)

  [Header("発射設定")]
  [SerializeField]
  private Transform firePoint; // 弾の発射基点 (nullなら自身)

  private Coroutine sequenceCoroutine; // パターンシーケンス実行中のコルーチン
  private int currentPatternIndex = 0; // 現在実行中のパターンのリスト内インデックス

  void Start()
  {
    // リストが空か、有効なパターンがないかチェック
    if (attackPatterns == null || attackPatterns.Count == 0 || attackPatterns[0] == null)
    {
      Debug.LogWarning("Attack Patterns list is empty or invalid.", this);
      enabled = false; // パターンがなければ何もしない
      return;
    }

    if (firePoint == null)
    {
      firePoint = transform; // FirePointがなければ自身のTransformを使用
    }

    StartAttackSequence(); // パターンシーケンスの開始
  }

  /// <summary>
  /// 攻撃パターンのシーケンスを開始します。
  /// </summary>
  public void StartAttackSequence()
  {
    // 既に実行中の場合は一度停止してから再開（あるいは何もしない設計も可能）
    StopAttackSequence();

    // 有効なパターンがある場合のみ開始
    if (attackPatterns != null && attackPatterns.Count > 0)
    {
      currentPatternIndex = 0; // 最初から開始
      sequenceCoroutine = StartCoroutine(ExecutePatternSequence());
      Debug.Log("Started Attack Pattern Sequence.", this);
    }
    else
    {
      Debug.LogWarning("Cannot start sequence, Attack Patterns list is empty.", this);
    }
  }

  /// <summary>
  /// 攻撃パターンのシーケンスを停止します。
  /// </summary>
  public void StopAttackSequence()
  {
    if (sequenceCoroutine != null)
    {
      // 実行中の個別パターンも停止させる必要がある
      // AttackPatternSO側が自身のコルーチンを管理している場合、
      // EnemyAttacker側で明示的に個別の攻撃コルーチンを停止するのは難しい。
      // AttackPatternSOのStartAttackSequenceが返すコルーチンを保持しておき、
      // それもStopCoroutineする必要があるかもしれないが、複雑になる。
      // ここでは、EnemyAttacker側のシーケンス管理コルーチンのみ停止する。
      // AttackPatternSO側のコルーチンは、次のyield returnのタイミングや、
      // attacker.isActiveAndEnabled チェックなどで自然に停止することを期待する。
      StopCoroutine(sequenceCoroutine);
      sequenceCoroutine = null;
      Debug.Log("Stopped Attack Pattern Sequence.", this);
    }
    // もしAttackPatternSOが生成したコルーチンも明示的に止めたい場合、
    // その参照を保持・管理する仕組みを追加する必要がある。
  }


  /// <summary>
  /// 攻撃パターンリストを順番に実行するコルーチン。
  /// </summary>
  private IEnumerator ExecutePatternSequence()
  {
    // シーケンス実行中の無限ループ（ループしない場合は条件が変わる）
    while (true)
    {
      // 現在のインデックスがリストの範囲内かチェック
      if (currentPatternIndex < 0 || currentPatternIndex >= attackPatterns.Count)
      {
        Debug.LogError($"Invalid pattern index: {currentPatternIndex}. Stopping sequence.", this);
        yield break; // インデックスがおかしければコルーチン終了
      }

      AttackPatternSO currentPattern = attackPatterns[currentPatternIndex];

      // パターンがnullでないかチェック
      if (currentPattern == null)
      {
        Debug.LogWarning($"Attack Pattern at index {currentPatternIndex} is null. Skipping.", this);
      }
      else
      {
        // --- 現在の攻撃パターンを実行 ---
        Debug.Log($"Executing Pattern: {currentPattern.name} (Index: {currentPatternIndex})", this);
        // AttackPatternSOのStartAttackSequenceがコルーチンを返すので、
        // それが終了するまで待機する
        yield return currentPattern.StartAttackSequence(this, firePoint);

        // 念のため、このGameObjectが無効になったら即座に抜けるチェック
        if (!this.isActiveAndEnabled) { yield break; }

        Debug.Log($"Finished Pattern: {currentPattern.name}", this);
      }


      // --- 次のパターンへ ---
      currentPatternIndex++;

      // リストの最後に到達したか？
      if (currentPatternIndex >= attackPatterns.Count)
      {
        if (loopSequence)
        {
          Debug.Log("Reached end of sequence, looping back to start.", this);
          currentPatternIndex = 0; // ループするならインデックスを0に戻す
        }
        else
        {
          Debug.Log("Reached end of sequence, stopping.", this);
          yield break; // ループしないならコルーチン終了
        }
      }

      // --- パターン間の待機 ---
      if (delayBetweenPatterns > 0)
      {
        //Debug.Log($"Waiting for {delayBetweenPatterns} seconds before next pattern.", this);
        yield return new WaitForSeconds(delayBetweenPatterns);
        // 待機中に無効になったら終了
        if (!this.isActiveAndEnabled) yield break;
      }
    }
  }

  // 以前のSetAttackPatternはリスト操作と競合するため削除。
  // 必要であれば、リストを操作するメソッド（AddPattern, RemovePattern, ClearPatternsなど）を別途実装する。
  /*
  public void SetAttackPattern(AttackPatternSO newPattern, bool restartAttack = true)
  {
      // このメソッドはリスト管理モデルとは相性が悪いのでコメントアウト
      // StopAttackSequence();
      // attackPatterns.Clear(); // リストをクリアして単一要素にする？設計による
      // if(newPattern != null) attackPatterns.Add(newPattern);
      // if (restartAttack)
      // {
      //    StartAttackSequence();
      // }
  }
  */

  // 敵が無効化されたり破壊されたりしたらシーケンスを止める
  void OnDisable()
  {
    StopAttackSequence();
  }
}
