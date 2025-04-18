using UnityEngine;

// すべての弾コントローラーが実装すべきインターフェース
public interface IBullet
{
    // 発射時に必要な初期設定を行うメソッド
    // 引数は必要に応じて追加・変更可能 (例: ターゲット情報、ダメージ値など)
    void Initialize(Vector3 position, Quaternion rotation, float speed);

    // (オプション) プールに戻すためのメソッドを定義しても良い
    // void ReturnToPool();
}
