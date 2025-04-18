using UnityEngine;

// 指定Y座標で分裂する弾
[RequireComponent(typeof(PooledObject))]
public class SplitterBullet : MonoBehaviour, IBullet // IBulletを実装
{
    [Header("Splitting Config")]
    public float splitYPosition = -4f; // 分裂するY座標
    public GameObject childBulletPrefab; // 分裂後の子弾プレハブ (インスペクター設定)
    public int splitCount = 6; // 分裂数
    public float childSpeed = 5f; // 子弾の速度

    // IBullet インターフェースの実装
    private float currentSpeed; // 親弾の速度

    // コンポーネント参照と内部状態
    public PooledObject pooledObject;
    private bool hasSplit = false;

    void OnEnable()
    {
        hasSplit = false; // 有効化時にリセット
    }

    // IBullet インターフェースの実装
    public void Initialize(Vector3 position, Quaternion rotation, float speed)
    {
        transform.position = position;
        transform.rotation = rotation;
        this.currentSpeed = speed;
        // Rigidbody があれば速度をリセット
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    void Update()
    {
        // 移動 (通常は下向きに発射される想定で Initialize された向きに進む)
        transform.Translate(Vector3.up * currentSpeed * Time.deltaTime, Space.Self);

        // 分裂判定
        if (!hasSplit && transform.position.y <= splitYPosition)
        {
            Split();
            hasSplit = true;
            // 分裂したら即座にプールに戻る
            ReturnToPool();
        }
    }

    void Split()
    {
        if (childBulletPrefab == null)
        {
            Debug.LogError("ChildBulletPrefabが未設定", this);
            return;
        }
        AdvancedObjectPooler pooler = AdvancedObjectPooler.Instance;
        if (pooler == null)
        {
            Debug.LogError("AdvancedObjectPoolerが見つかりません", this);
            return;
        }

        float angleStep = 360f / splitCount;
        for (int i = 0; i < splitCount; i++)
        {
            float currentAngle = angleStep * i;
            float rad = currentAngle * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Sin(rad), Mathf.Cos(rad), 0); // Y+ を0度とする

            GameObject childGO = pooler.GetObject(childBulletPrefab); // ★ 種類を指定して取得
            if (childGO != null)
            {
                IBullet childBullet = childGO.GetComponent<IBullet>(); // ★ インターフェースを取得
                if (childBullet != null)
                {
                    // 子弾を初期化してアクティブ化
                    Quaternion childRotation = Quaternion.FromToRotation(Vector3.up, direction);
                    childBullet.Initialize(transform.position, childRotation, childSpeed);
                    childGO.SetActive(true);
                }
                else
                {
                    Debug.LogError(
                        $"子弾プレハブ {childBulletPrefab.name} に IBullet 実装がありません",
                        this
                    );
                    pooler.ReturnObject(childGO); // 使わないので戻す
                }
            }
            else
            {
                Debug.LogWarning($"子弾 {childBulletPrefab.name} の取得失敗 (プール空か未登録?)");
            }
        }
    }

    // 親弾の衝突処理や画面外処理 (StraightBullet同様)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasSplit && (other.CompareTag("Wall") || other.CompareTag("Player")))
        {
            ReturnToPool();
        }
    }

    void OnBecameInvisible()
    {
        if (!hasSplit)
            ReturnToPool();
    }

    // プールに戻る処理 (StraightBullet同様)
    private void ReturnToPool()
    {
        if (pooledObject != null && pooledObject.ownerPooler != null && gameObject.activeSelf)
        {
            pooledObject.ownerPooler.ReturnObject(this.gameObject);
        }
        else if (gameObject.activeSelf)
        {
            Debug.LogWarning($"Could not return {gameObject.name} to pool.", this);
            Destroy(gameObject);
        }
    }
}
