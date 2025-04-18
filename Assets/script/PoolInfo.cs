// Assets/Scripts/Pooling/PoolInfo.cs (または任意のフォルダ構成)
using UnityEngine;

// プールするオブジェクトの情報を持つ構造体 (Serializableでインスペクター表示可能)
[System.Serializable]
public struct PoolInfo
{
    [Tooltip("プールするオブジェクトのプレハブ")]
    public GameObject prefab;

    [Tooltip("ゲーム開始時に生成しておく数")]
    public int initialSize;

    [Tooltip("このプールを文字列で識別するためのキー (空の場合はプレハブIDがキーになります)")]
    public string poolKey; // publicに変更してアクセス可能に

    // このプール情報の内部的な識別キーを取得するメソッド
    // (インスペクターで設定された poolKey があればそれを、なければ prefab の InstanceID を使う)
    public string GetKey()
    {
        if (!string.IsNullOrEmpty(poolKey))
        {
            return poolKey;
        }
        if (prefab != null)
        {
            // InstanceID はエディタや実行ごとに変わる可能性があるので注意が必要だが、
            // 実行中の参照としては一意なキーとして使える。
            // より堅牢にするなら、プレハブパスやGUIDを使う方法もある。
            return prefab.GetInstanceID().ToString();
        }
        return null; // プレハブもキーもない場合は無効
    }
}
