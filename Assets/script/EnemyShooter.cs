using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public Transform firePoint; // 弾の発射位置を示すTransform
    public float fireRate = 1f; // 発射間隔（秒）

    // public GameObject bulletPrefab;  // プーラーを使うので直接は不要になるかも
    public float bulletSpeed = 10f; // 発射する弾の速度（BulletControllerのデフォルトを使うなら不要）
    public float bulletLifetime = 3f; // 発射する弾の寿命（BulletControllerのデフォルトを使うなら不要）

    private float nextFireTime = 0f; // 次に発射可能な時間
    private SimpleObjectPooler pooler; // オブジェクトプーラーへの参照

    void Start()
    {
        // プーラーのインスタンスを取得
        pooler = SimpleObjectPooler.Instance;
        if (pooler == null)
        {
            Debug.LogError("SimpleObjectPooler instance not found! EnemyShooter cannot fire.");
        }
        if (firePoint == null)
        {
            Debug.LogError("Fire Point not set on EnemyShooter!");
            enabled = false; // 発射ポイントがないなら動作しないようにする
        }
    }

    void Update()
    {
        if (pooler == null)
            return; // プーラーがない場合は何もしない

        // 発射時間になったら
        if (Time.time >= nextFireTime)
        {
            Shoot(); // 発射処理
            nextFireTime = Time.time + fireRate; // 次の発射時間を設定
        }
    }

    void Shoot()
    {
        // プールから弾を取得
        GameObject bulletGO = pooler.GetBullet();

        if (bulletGO != null)
        {
            // 弾の位置と向きをFirePointに合わせる
            bulletGO.transform.position = firePoint.position;
            bulletGO.transform.rotation = firePoint.rotation;

            // (オプション) 弾の速度や寿命をここで上書きする場合
            BulletController bulletController = bulletGO.GetComponent<BulletController>();
            if (bulletController != null)
            {
                bulletController.speed = this.bulletSpeed; // この敵固有の速度設定など
                bulletController.lifetime = this.bulletLifetime;
                // bulletController.Initialize(this.bulletSpeed, this.bulletLifetime); // Initializeメソッドがある場合
            }

            // 弾をアクティブにする
            bulletGO.SetActive(true);

            Debug.Log("Fired a bullet!");
        }
        else
        {
            Debug.LogWarning("Could not get bullet from pool."); // プールから取得失敗
        }
    }
}
