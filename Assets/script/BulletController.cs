using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed = 10f; // 弾の速度
    public float lifetime = 3f; // 弾が存在する時間（秒）

    private float lifeTimer; // 寿命をカウントするタイマー
    private SimpleObjectPooler pooler; // オブジェクトプーラーへの参照

    // オブジェクトが有効化された時に呼ばれる
    void OnEnable()
    {
        // 寿命タイマーリセット
        lifeTimer = lifetime;

        // プーラーのインスタンスを取得 (Awakeより後なのでInstanceは設定されているはず)
        if (SimpleObjectPooler.Instance != null)
        {
            pooler = SimpleObjectPooler.Instance;
        }
        else
        {
            Debug.LogError("SimpleObjectPooler instance not found!");
            // プーラーが見つからない場合は自身を非アクティブ化するなどエラー処理
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // まっすぐ前（ローカル座標の上方向）に移動
        // 注意: 弾のプレハブやスプライトが「上向き」を「前」として作られている前提
        // もし「右向き」が前なら Vector3.right を使う
        transform.Translate(Vector3.up * speed * Time.deltaTime, Space.Self);

        // 寿命タイマーを減らす
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            DestroyBullet(); // 寿命が来たら消滅
        }
    }

    // 画面外に出た時の処理（オプション）
    void OnBecameInvisible()
    {
        // 画面外に出ても即座に消すかはゲームによる。寿命で消すならこれは不要な場合も。
        // DestroyBullet();
    }

    // 衝突時の処理 (Collider2D の IsTrigger が On の場合)
    void OnTriggerEnter2D(Collider2D other)
    {
        // ここで衝突相手に応じてダメージ処理などを行う
        // 例: プレイヤーに当たったらダメージを与えて消滅
        if (other.CompareTag("Player")) // 相手が"Player"タグを持っている場合
        {
            Debug.Log("Hit Player!");
            // Playerにダメージを与える処理 (例)
            // other.GetComponent<PlayerHealth>()?.TakeDamage(10);
            DestroyBullet(); // 弾を消滅させる
        }
        else if (other.CompareTag("Wall")) // 壁などに当たった場合
        {
            DestroyBullet(); // 弾を消滅させる
        }
        // 他にも Enemy に当たらないようにする条件などが必要なら追加
    }

    // 弾を消滅させる（プールに戻す）処理
    void DestroyBullet()
    {
        if (pooler != null)
        {
            pooler.ReturnBullet(gameObject); // プーラーに自身を返す
        }
        else
        {
            // プーラーが見つからなかった場合のフォールバック（Destroy）
            Debug.LogWarning("Pooler not found, destroying bullet manually.");
            Destroy(gameObject);
        }
    }

    // 必要に応じて初期化メソッドを追加することもできる
    // public void Initialize(float bulletSpeed, float bulletLifetime) {
    //     this.speed = bulletSpeed;
    //     this.lifetime = bulletLifetime;
    // }
}
