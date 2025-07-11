using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class RoomKick : MonoBehaviourPunCallbacks
{
    [Header("自動退出設定")]
    [SerializeField] private float kickTimeLimit = 90f; // 1分30秒で自動退出
    [SerializeField] private float warningTimeLimit = 15f; // 残り15秒で警告表示
    
    [Header("警告パネル設定")]
    [SerializeField] private GameObject warningPanel; // 警告パネル（テキストの親）
    
    private Text warningText; // 警告メッセージ用テキスト（パネルの子から自動取得）
    private float timer = 0f;
    private bool isKicking = false; // 退出処理中フラグ
    private bool isWarningShown = false; // 警告表示フラグ
    
    void Start()
    {
        timer = 0f;
        isWarningShown = false;
        
        // 警告パネルとテキストを初期化
        InitializeWarningPanel();
        
        Debug.Log($"[RoomKick] 自動退出タイマー開始 - {kickTimeLimit}秒（1分30秒）で強制退出します");
    }

    void Update()
    {
        if (isKicking) return; // 退出処理中は更新しない
        
        // タイマーを進める
        timer += Time.deltaTime;
        
        // 残り時間を計算
        float remainingTime = kickTimeLimit - timer;
        
        // 残り15秒以下で警告表示
        if (remainingTime <= warningTimeLimit && !isWarningShown)
        {
            ShowWarning();
        }
        
        // 警告表示中はテキストを更新
        if (isWarningShown && warningText != null)
        {
            UpdateWarningText(remainingTime);
        }
        
        // 時間制限に達したら強制退出
        if (timer >= kickTimeLimit)
        {
            Debug.Log($"[RoomKick] {kickTimeLimit}秒（1分30秒）経過 - 強制的にルームから退出します");
            ForceLeaveRoom();
        }
    }
    
    /// <summary>
    /// 強制的にルームから退出してFieldSceneに戻る
    /// </summary>
    private void ForceLeaveRoom()
    {
        if (isKicking) return; // 重複実行防止
        
        isKicking = true;
        
        Debug.Log("[RoomKick] 強制退出処理を開始します");
        
        // ルームの状態をチェック
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            Debug.Log("[RoomKick] ルームに接続中です。RPCとキャッシュをクリアします。");
            // RPCとキャッシュをクリア
            try
            {
                PhotonNetwork.RemoveRPCs(PhotonNetwork.LocalPlayer);
                PhotonNetwork.OpCleanActorRpcBuffer(PhotonNetwork.LocalPlayer.ActorNumber);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[RoomKick] RPCクリア中にエラーが発生しました: {e.Message}");
            }
            
            // ルームから退室
            try
            {
                PhotonNetwork.LeaveRoom();
                Debug.Log("[RoomKick] ルームから退出を試行しました");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[RoomKick] ルーム退出中にエラーが発生しました: {e.Message}");
                // エラーが発生した場合も直接シーン遷移
                LoadFieldScene();
            }
        }
        else
        {
            Debug.Log("[RoomKick] ルームに接続されていないため、直接FieldSceneに遷移します");
            // 直接シーン遷移
            LoadFieldScene();
        }
    }
    
    /// <summary>
    /// ルーム退出完了時の処理（Photonコールバック）
    /// </summary>
    public override void OnLeftRoom()
    {
        Debug.Log("[RoomKick] ルームから退出完了 - FieldSceneに遷移します");
        LoadFieldScene();
    }
    
    /// <summary>
    /// FieldSceneに遷移
    /// </summary>
    private void LoadFieldScene()
    {
        StartCoroutine(LoadFieldSceneCoroutine());
    }
    
    /// <summary>
    /// FieldScene遷移のコルーチン
    /// </summary>
    private IEnumerator LoadFieldSceneCoroutine()
    {
        // 1フレーム待機して確実にPhotonの処理を完了させる
        yield return new WaitForEndOfFrame();
        
        Debug.Log("[RoomKick] FieldSceneをロードします");
        SceneManager.LoadScene("FieldScene");
    }
    
    /// <summary>
    /// タイマーをリセット（プレイヤーが操作した時に呼ぶ用）
    /// </summary>
    public void ResetTimer()
    {
        if (!isKicking)
        {
            timer = 0f;
            HideWarning(); // 警告も非表示にする
            Debug.Log("[RoomKick] タイマーをリセットしました");
        }
    }
    
    /// <summary>
    /// 現在の残り時間を取得
    /// </summary>
    public float GetRemainingTime()
    {
        return Mathf.Max(0f, kickTimeLimit - timer);
    }
    
    /// <summary>
    /// 警告パネルとテキストの初期化
    /// </summary>
    private void InitializeWarningPanel()
    {
        if (warningPanel == null)
        {
            // 自動でパネルを検索（オプション）
            warningPanel = GameObject.Find("WarningPanel");
            if (warningPanel != null)
            {
                Debug.Log("[RoomKick] WarningPanelを自動検出しました");
            }
        }
        
        // パネルの子からTextコンポーネントを取得
        if (warningPanel != null)
        {
            warningText = warningPanel.GetComponentInChildren<Text>();
            if (warningText != null)
            {
                Debug.Log("[RoomKick] 警告パネルの子からTextコンポーネントを取得しました");
            }
            else
            {
                Debug.LogWarning("[RoomKick] 警告パネルの子にTextコンポーネントが見つかりません");
            }
        }
        else
        {
            Debug.LogWarning("[RoomKick] warningPanelが設定されていません");
        }
        
        // パネルを初期状態で非表示
        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// 警告表示を開始
    /// </summary>
    private void ShowWarning()
    {
        isWarningShown = true;
        
        if (warningPanel != null)
        {
            warningPanel.SetActive(true);
            Debug.Log("[RoomKick] 残り15秒以下 - 警告パネルを表示しました");
        }
        else
        {
            Debug.LogWarning("[RoomKick] warningPanelが設定されていません");
        }
    }
    
    /// <summary>
    /// 警告テキストの内容を更新
    /// </summary>
    private void UpdateWarningText(float remainingTime)
    {
        if (warningText != null)
        {
            int seconds = Mathf.CeilToInt(remainingTime);
            seconds = Mathf.Max(0, seconds); // 0以下にならないように
            
            warningText.text = $"残り{seconds}秒で\n強制退出させられます";
        }
    }
    
    /// <summary>
    /// 警告パネルを非表示にする
    /// </summary>
    private void HideWarning()
    {
        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
        }
        isWarningShown = false;
    }

    /// <summary>
    /// 自動退出を停止（手動でゲームが終了した時など）
    /// </summary>
    public void StopAutoKick()
    {
        isKicking = true;
        HideWarning(); // 警告も非表示にする
        Debug.Log("[RoomKick] 自動退出を停止しました");
    }
}
