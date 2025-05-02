// IBulletBehavior.cs
using UnityEngine;

public interface IBulletBehavior
{
    /// <summary>
    /// この弾インスタンスの元となったプレハブ。
    /// オブジェクトプーラーが設定し、返却時に使用する。
    /// </summary>
    GameObject OriginalPrefab { get; set; }

    void Setup(Vector3 initialPosition, Vector3 direction, float speed, object additionalData = null);

    // ... 他の必要なメソッドやプロパティがあればここに追加 ...
}
