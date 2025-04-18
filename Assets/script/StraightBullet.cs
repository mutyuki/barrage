using UnityEngine;

// 直進し、一定時間後に消滅する弾
[RequireComponent(typeof(PooledObject))] // プーラーに戻すために必須
public class StraightBullet : MonoBehaviour, IBullet // IBulletを実装
{
    // --- Inspector 設定 ---
    [Tooltip("弾の生存期間（秒）")]
    public float lifetime = 3.0f; // ★ ここで寿命を3秒に設定

    // IBullet インターフェースの実装用 (Initializeで設定)
    private float currentSpeed;

    // コンポーネント参照 (キャッシュ)
    public PooledObject pooledObject;

    // 内部状態
    private float lifeTimer; // 残り寿命タイマー

    // ★ OnEnable でタイマーをリセット
    void OnEnable()
    {
        // オブジェクトがプールから取り出されて有効化されたときに寿命タイマーを設定
        lifeTimer = lifetime;
    }

    // IBullet インターフェースの実装
    public void Initialize(Vector3 position, Quaternion rotation, float speed)
    {
        transform.position = position;
        transform.rotation = rotation;
        this.currentSpeed = speed;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // Rigidbody2Dなら velocity を使うのが一般的
            rb.angularVelocity = 0f;
        }
        // lifeTimer = lifetime; // ★ Initialize ではなく OnEnable でリセットするのが確実
    }

    void Update()
    {
        // 前進処理
        transform.Translate(Vector3.up * currentSpeed * Time.deltaTime, Space.Self);

        // --- ★ 寿命タイマーのカウントダウンとチェック ---
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            // Debug.Log($"Lifetime expired for {gameObject.name}");
            ReturnToPool(); // 寿命が尽きたらプールに戻る
        }
        // ---------------------------------------------
    }

    // --- 画面外判定メソッドは不要なので削除またはコメントアウト ---
    // void CheckOffScreen() { ... }
    // void OnBecameInvisible() { ... }

    // 衝突判定は残す (任意)

    // プールに戻る処理 (ガード節を入れた安全なバージョン)
    public void ReturnToPool() // IBulletに含める場合はpublic
    {
        // 既に非アクティブなら何もしない (重要！)
        if (!gameObject.activeSelf)
            return;

        if (pooledObject == null || pooledObject.ownerPooler == null)
        {
            Debug.LogWarning(
                $"Could not return {gameObject.name} to pool (PooledObject info missing!). Destroying.",
                this
            );
            if (gameObject.activeSelf)
                Destroy(gameObject);
            return;
        }

        try
        {
            pooledObject.ownerPooler.ReturnObject(this.gameObject);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error returning object {gameObject.name} to pool: {ex.Message}", this);
            if (gameObject.activeSelf)
                Destroy(gameObject);
        }
    }
}
