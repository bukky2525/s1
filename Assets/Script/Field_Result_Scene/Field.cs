using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 FieldシーンからPlayシーンへの移行処理
 スペースキー・タッチ操作対応
 */

public class Field : MonoBehaviour
{
    [Header("タッチ操作設定")]
    [SerializeField] private bool enableTouchInput = true; // タッチ入力を有効にするか
    
    private bool isTransitioning = false; // 移行中フラグ（重複防止）

    // Start is called before the first frame update
    void Start()
    {
        // モバイル環境の検出ログ
        Debug.Log($"[Field] プラットフォーム: {Application.platform}");
        Debug.Log($"[Field] タッチサポート: {Input.touchSupported}");
        Debug.Log($"[Field] タッチ入力有効: {enableTouchInput}");
    }

    // Update is called once per frame
    void Update()
    {
        if (isTransitioning) return; // 移行中は入力を無視

        // スペースキー入力
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("[Field] スペースキーが押されました - PlaySceneへ移行");
            TransitionToPlayScene();
        }

        // タッチ入力（スマホ・タブレット用）
        if (enableTouchInput && CheckTouchInput())
        {
            Debug.Log("[Field] タッチ入力を検出 - PlaySceneへ移行");
            TransitionToPlayScene();
        }

        // マウスクリック入力（PC用の代替）
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("[Field] マウスクリックを検出 - PlaySceneへ移行");
            TransitionToPlayScene();
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

    // PlaySceneへの移行処理
    private void TransitionToPlayScene()
    {
        if (isTransitioning) return; // 重複防止

        isTransitioning = true;
        StartCoroutine(LoadPlayScene());
    }

    // シーン移行のコルーチン
    private IEnumerator LoadPlayScene()
    {
        // フレーム待機してから移行（安全な移行のため）
        yield return new WaitForEndOfFrame();
        SceneManager.LoadScene("PlayScene");
    }
}
