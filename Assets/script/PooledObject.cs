// Assets/Scripts/Pooling/PooledObject.cs (または任意のフォルダ構成)
using UnityEngine;

// プールされる GameObject にアタッチされるコンポーネント
// 自身がどのプールに属し、どのプーラーによって管理されているかの情報を持つ
public class PooledObject : MonoBehaviour
{
    [Tooltip("このオブジェクトが属するプールの識別キー")]
    public string poolKey; // public のままで良い

    [Tooltip("このオブジェクトを管理しているプーラーのインスタンス")]
    public AdvancedObjectPooler ownerPooler; // public のままで良い

    // (オプション) プールに戻る処理をここに集約することも可能
    // public void ReturnToPool()
    // {
    //     if (ownerPooler != null)
    //     {
    //         ownerPooler.ReturnObject(this.gameObject);
    //     }
    //     else
    //     {
    //          Debug.LogWarning($"OwnerPooler not set for {gameObject.name}, cannot return via PooledObject. Destroying.", this);
    //         Destroy(gameObject);
    //     }
    // }
}
