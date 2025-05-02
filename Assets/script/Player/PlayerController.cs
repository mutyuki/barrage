// PlayerController_VimMovement.cs (ファイル名を変更推奨)
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour // or PlayerController_VimMovement etc.
{
  [Header("Movement Settings")]
  [SerializeField] private float moveSpeed = 7.0f; // 移動速度
  [SerializeField] private Vector2 screenPadding = new Vector2(0.5f, 0.5f); // 画面端からの余白

  // --- Private Variables ---
  private Rigidbody2D rb;
  private Vector2 minBounds;   // 移動可能範囲（左下）
  private Vector2 maxBounds;   // 移動可能範囲（右上）
  private Camera mainCamera;

  void Awake()
  {
    rb = GetComponent<Rigidbody2D>();
    mainCamera = Camera.main;

    if (rb == null)
      Debug.LogError("Rigidbody2D component not found on Player!", this);

    if (mainCamera == null)
      Debug.LogError("Main Camera not found! Cannot calculate screen bounds.", this);
  }

  void Start()
  {
    if (mainCamera != null)
    {
      CalculateMovementBounds();
    }
  }

  void Update()
  {
    HandleMovementInput(); // ★ ここを修正
    if (mainCamera != null)
    {
      ClampPosition();
    }
  }

  // --- ここから修正 ---
  void HandleMovementInput()
  {
    if (rb == null) return;

    float moveX = 0f;
    float moveY = 0f;

    // 左移動 (hキー)
    if (Input.GetKey(KeyCode.H))
    {
      moveX = -1f;
    }
    // 右移動 (lキー)
    else if (Input.GetKey(KeyCode.L)) // hとlの同時押しを避けるため else if に
    {
      moveX = 1f;
    }

    // 下移動 (jキー)
    if (Input.GetKey(KeyCode.J))
    {
      moveY = -1f;
    }
    // 上移動 (kキー)
    else if (Input.GetKey(KeyCode.K)) // jとkの同時押しを避けるため else if に
    {
      moveY = 1f;
    }

    // ★移動方向ベクトルを作成
    Vector2 moveDirection = new Vector2(moveX, moveY);

    // ★斜め移動時に速度が速くなりすぎないように正規化 (ただし、正規化すると斜め移動が若干遅く感じる場合もある)
    //   Vector2.normalized は magnitude が 0 のベクトルに対しては zero を返す
    if (moveDirection.magnitude > 1f) // 同時押しの場合 magnitude は 1 より大きくなる可能性がある (厳密には sqrt(2) だが、ここでは > 0 か > 1 かで判定すれば十分)
    {
      moveDirection = moveDirection.normalized; // 対角線方向への速度が1になるようにする
    }


    // ★目標速度を計算
    Vector2 targetVelocity = moveDirection * moveSpeed;

    // Rigidbodyの速度を設定して移動
    rb.linearVelocity = targetVelocity;
  }
  // --- 修正ここまで ---


  void CalculateMovementBounds()
  {
    if (mainCamera == null) return;
    minBounds = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane)) + (Vector3)screenPadding;
    maxBounds = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane)) - (Vector3)screenPadding;
  }

  void ClampPosition()
  {
    if (rb == null) return;
    Vector2 currentPosition = rb.position;
    Vector2 clampedPosition = currentPosition;
    clampedPosition.x = Mathf.Clamp(currentPosition.x, minBounds.x, maxBounds.x);
    clampedPosition.y = Mathf.Clamp(currentPosition.y, minBounds.y, maxBounds.y);

    if (currentPosition != clampedPosition)
    {
      rb.position = clampedPosition;
      Vector2 newVelocity = rb.linearVelocity;
      if (currentPosition.x != clampedPosition.x) newVelocity.x = 0;
      if (currentPosition.y != clampedPosition.y) newVelocity.y = 0;
      rb.linearVelocity = newVelocity;
    }
  }
}
