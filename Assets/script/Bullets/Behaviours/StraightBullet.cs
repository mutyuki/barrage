// StraightBullet.cs
using UnityEngine;

public class StraightBullet : MonoBehaviour, IBulletBehavior
{
  // --- IBulletBehavior の実装 ---
  public GameObject OriginalPrefab { get; set; }
  // -----------------------------

  private Vector3 moveDirection;
  private float moveSpeed;
  private bool isSetup = false;

  public void Setup(Vector3 initialPosition, Vector3 direction, float speed, object additionalData = null)
  {
    transform.position = initialPosition;
    this.moveDirection = direction.normalized;
    this.moveSpeed = speed;
    this.isSetup = true;
    gameObject.SetActive(true); // Setup時にアクティブ化
  }

  void Update()
  {
    if (!isSetup) return;
    transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    CheckAndReturnIfOffScreen();
  }

  private void CheckAndReturnIfOffScreen()
  {
    Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
    if (viewportPos.x < -0.1f || viewportPos.x > 1.1f || viewportPos.y < -0.1f || viewportPos.y > 1.1f)
    {
      ReturnToPool();
    }
  }

  private void ReturnToPool()
  {
    isSetup = false;
    // OriginalPrefabプロパティを持つことを前提としてプーラーに渡す
    SimpleObjectPooler.Instance.ReturnBullet(gameObject);
  }

  void OnDisable()
  {
    isSetup = false;
  }
}
