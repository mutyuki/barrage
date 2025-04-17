using System.Collections.Generic;
using UnityEngine;

public class SimpleObjectPooler : MonoBehaviour
{
    // このプールを他から簡単にアクセスできるようにするシングルトンパターン
    public static SimpleObjectPooler Instance;

    public GameObject bulletPrefab; // プールする弾のプレハブ
    public int initialPoolSize = 20; // 初期に生成しておく弾の数

    private Queue<GameObject> pooledBullets; // 非アクティブな弾を保持するキュー

    void Awake()
    {
        // シングルトンの設定
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // 既に存在する場合は自身を破棄
            return;
        }

        // プールの初期化
        pooledBullets = new Queue<GameObject>();

        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = Instantiate(bulletPrefab);
            obj.SetActive(false); // 非アクティブにしておく
            obj.transform.SetParent(this.transform); // プーラーの子にして整理
            pooledBullets.Enqueue(obj); // キューに追加
        }
    }

    // プールから弾を取得するメソッド
    public GameObject GetBullet()
    {
        if (pooledBullets.Count > 0)
        {
            GameObject bullet = pooledBullets.Dequeue(); // キューから取り出す
            // bullet.SetActive(true); // すぐに有効にするのではなく、使う側で設定してもらう
            return bullet;
        }
        else
        {
            // プールが空の場合、新しく生成（必要に応じて）
            Debug.LogWarning("Pool is empty. Instantiating new bullet.");
            GameObject obj = Instantiate(bulletPrefab);
            obj.SetActive(false); // 初期状態は非アクティブ
            obj.transform.SetParent(this.transform);
            // 新しく生成したものはすぐには使わず、次回以降のためにEnqueueしない
            // 今回のリクエストにはnullを返すか、今生成したものを返すか設計による
            // ここでは今生成したものを返すようにする
            // obj.SetActive(true); // 使う側で有効にする想定
            return obj;
            // return null; // または空の場合は null を返す設計も可能
        }
    }

    // 弾をプールに戻すメソッド
    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false); // 非アクティブにする
        // Transformのリセットなど必要ならここで行う
        // bullet.transform.position = Vector3.zero;
        // bullet.transform.rotation = Quaternion.identity;
        pooledBullets.Enqueue(bullet); // キューに戻す
    }
}
