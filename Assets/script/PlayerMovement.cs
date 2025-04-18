using UnityEngine;

// Rigidbody2Dがないと動作しないので、自動的に追加する（または警告を出す）
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("プレイヤーの移動速度")]
    public float moveSpeed = 5f; // インスペクターで調整可能な移動速度

    private Rigidbody2D rb; // Rigidbody2Dへの参照を保持する変数
    private Vector2 moveInput; // プレイヤーの入力方向を保持する変数

    // Awake is called when the script instance is being loaded.
    void Awake()
    {
        // GetComponent で Rigidbody2D を取得し、変数 rb に格納
        rb = GetComponent<Rigidbody2D>();

        // Rigidbody2D が取得できなかった場合のエラー処理
        if (rb == null)
        {
            Debug.LogError(
                "Rigidbody2Dが見つかりません！ PlayerMovementスクリプトにはRigidbody2Dが必要です。",
                this
            );
            enabled = false; // スクリプトを無効化
            return;
        }

        // 推奨設定：弾幕ゲームでは物理挙動より直接制御のためKinematicにし、重力を0に。
        // Rigidbody2Dの設定をインスペクターで行う場合は、この行はコメントアウトまたは削除してもOK。
        // rb.isKinematic = true;
        rb.gravityScale = 0f; // 重力の影響を受けないように
        rb.freezeRotation = true; // 回転しないように固定（弾に押されて回転するなど防ぐ）
    }

    // Update is called once per frame
    void Update()
    {
        // --- 入力受付 ---
        // GetAxisRaw を使うと、入力がないときは0、入力があるときは即座に-1か1が返る（アナログ的な中間がない）
        // デジタルな動き（ピッタリ止まる）に適している
        float moveX = Input.GetAxisRaw("Horizontal"); // "Horizontal" はデフォルトで A/Dキー、←/→キーに割り当てられている
        float moveY = Input.GetAxisRaw("Vertical"); // "Vertical" はデフォルトで W/Sキー、↑/↓キーに割り当てられている

        // --- 入力ベクトルを正規化 ---
        // moveXとmoveYから入力方向ベクトルを作成し、正規化(Normalize)する
        // 正規化しないと斜め移動が √2 倍速くなってしまうため、長さを1にする
        moveInput = new Vector2(moveX, moveY).normalized;
        // 注意: moveInputが(0,0)の場合、normalizedも(0,0)を返すので安全
    }

    // FixedUpdate is called at a fixed interval and is independent of frame rate.
    // 物理演算に関する処理はこちらに書くのが推奨される
    void FixedUpdate()
    {
        // --- 移動処理 ---
        // Rigidbody2Dを使って移動させる (物理的に安全な方法)
        // rb.position: 現在の位置
        // moveInput: 移動方向 (長さ1 or 0)
        // moveSpeed: 移動速度
        // Time.fixedDeltaTime: FixedUpdateの1フレームの時間。フレームレートに依存しない移動速度にするため。
        Vector2 targetPosition = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;

        // Rigidbody2D.MovePosition を使って移動。物理エンジンが補間などを行い、他のコライダーとの衝突も考慮される。
        // (BodyType が Kinematic の場合でもこちらを使うのが良い)
        rb.MovePosition(targetPosition);

        // --- Rigidbody2Dを使わない場合 (参考) ---
        // こちらは壁抜けなど物理的な問題を無視してしまう可能性があるため注意
        // transform.Translate(moveInput * moveSpeed * Time.deltaTime, Space.World);
    }
}
