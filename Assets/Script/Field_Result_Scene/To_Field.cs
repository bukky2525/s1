using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

/*
 ResultシーンからFieldシーンへの移行処理
 スペースキー・タッチ操作対応
 20秒後の自動移行機能付き
 */

public class To_Field : MonoBehaviour
{
    private bool isTransitioning = false;
    private PhotonView photonView;
    private float waitTime = 20f; // 20秒待つ
    private float timer = 0f;
    [SerializeField] private string backSceneName = "FieldScene"; // 戻るシーン名（必要に応じて変更）
    
    [Header("タッチ操作設定")]
    [SerializeField] private bool enableTouchInput = true; // タッチ入力を有効にするか

    // Start is called before the first frame update
    void Start()
    {
        timer = 0f;
        
        // モバイル環境の検出ログ
        Debug.Log($"[To_Field] プラットフォーム: {Application.platform}");
        Debug.Log($"[To_Field] タッチサポート: {Input.touchSupported}");
        Debug.Log($"[To_Field] タッチ入力有効: {enableTouchInput}");
    }

    // Update is called once per frame
    void Update()
    {
        if (isTransitioning) return; // 移行中は処理をスキップ

        // スペースキーが押されたらFieldSceneへ移行
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("[To_Field] スペースキーが押されました - FieldSceneへ移行します");
            StartTransition();
        }

        // タッチ入力（スマホ・タブレット用）
        if (enableTouchInput && CheckTouchInput())
        {
            Debug.Log("[To_Field] タッチ入力を検出 - FieldSceneへ移行します");
            StartTransition();
        }

        // マウスクリック入力（PC用の代替）
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("[To_Field] マウスクリックを検出 - FieldSceneへ移行します");
            StartTransition();
        }

        // タイマーを進める（自動移行用）
        timer += Time.deltaTime;
        if (timer >= waitTime)
        {
            Debug.Log($"[To_Field] {waitTime}秒間待機したのでFieldSceneへ移行します");
            StartTransition();
        }
    }

    // タッチ入力をチェック
    private bool CheckTouchInput()
    {
        // タッチ入力の確認
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                return true; // タッチ開始を検出
            }
        }
        return false;
    }

    // 移行処理を開始
    private void StartTransition()
    {
        if (isTransitioning) return; // 重複防止

        isTransitioning = true;
        
        // PhotonViewを破棄
        if (photonView != null)
        {
            PhotonNetwork.Destroy(photonView);
            photonView = null;
        }
        
        // シーン遷移前に少し待機
        StartCoroutine(LoadFieldScene());
    }

    private IEnumerator LoadFieldScene()
    {
        // 1フレーム待機してPhotonViewの破棄を確実にする
        yield return new WaitForEndOfFrame();
        SceneManager.LoadScene("FieldScene");
    }

    private IEnumerator BackToPrevScene()
    {
        yield return new WaitForEndOfFrame();
        SceneManager.LoadScene(backSceneName);
    }
}
