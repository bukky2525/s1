using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class Result : MonoBehaviourPunCallbacks, IPunObservable
{
    private new PhotonView photonView;

    void Awake()
    {
        // メッセージキューを有効化
        PhotonNetwork.IsMessageQueueRunning = true;

        // PhotonViewの設定
        photonView = GetComponent<PhotonView>();
        if (photonView == null)
        {
            photonView = gameObject.AddComponent<PhotonView>();
            photonView.Synchronization = ViewSynchronization.UnreliableOnChange;
            photonView.ObservedComponents = new System.Collections.Generic.List<Component> { this };
        }
    }

    void Start()
    {
    }

    public override void OnEnable()
    {
        base.OnEnable();
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    /// <summary>
    /// 勝利時の処理
    /// </summary>
    public void ShowWinResult()
    {
        Debug.Log("[Result] 勝利シーンに遷移します");
        
        // ルームの状態をチェック
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            Debug.Log("[Result] ルームに接続中です。RPCとキャッシュをクリアします。");
            // RPCとキャッシュをクリア
            try
            {
                PhotonNetwork.RemoveRPCs(PhotonNetwork.LocalPlayer);
                PhotonNetwork.OpCleanActorRpcBuffer(PhotonNetwork.LocalPlayer.ActorNumber);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Result] RPCクリア中にエラーが発生しました: {e.Message}");
            }
            
            // ルームから退室
            try
            {
                PhotonNetwork.LeaveRoom();
                Debug.Log("[Result] ルームから退出を試行しました");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Result] ルーム退出中にエラーが発生しました: {e.Message}");
            }
        }
        else
        {
            Debug.Log("[Result] ルームに接続されていないため、直接シーン遷移します");
            // 直接シーン遷移
            SceneManager.LoadScene("WinScene");
        }
    }

    /// <summary>
    /// 敗北時の処理
    /// </summary>
    public void ShowLoseResult()
    {
        Debug.Log("[Result] 敗北シーンに遷移します");
        
        // ルームの状態をチェック
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            Debug.Log("[Result] ルームに接続中です。RPCとキャッシュをクリアします。");
            // RPCとキャッシュをクリア
            try
            {
                PhotonNetwork.RemoveRPCs(PhotonNetwork.LocalPlayer);
                PhotonNetwork.OpCleanActorRpcBuffer(PhotonNetwork.LocalPlayer.ActorNumber);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Result] RPCクリア中にエラーが発生しました: {e.Message}");
            }
            
            // ルームから退室
            try
            {
                PhotonNetwork.LeaveRoom();
                Debug.Log("[Result] ルームから退出を試行しました");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Result] ルーム退出中にエラーが発生しました: {e.Message}");
            }
        }
        else
        {
            Debug.Log("[Result] ルームに接続されていないため、直接シーン遷移します");
            // 直接シーン遷移
            SceneManager.LoadScene("LoseScene");
        }
    }

    /// <summary>
    /// ルーム退出完了時の処理
    /// </summary>
    public override void OnLeftRoom()
    {
        Debug.Log("[Result] ルームから退出完了");
        // 現在のシーン名に基づいて適切なシーンに遷移
        if (SceneManager.GetActiveScene().name == "PlayScene")
        {
            SceneManager.LoadScene("WinScene");
        }
        else
        {
            SceneManager.LoadScene("LoseScene");
        }
    }

    /// <summary>
    /// Hostからの勝敗メッセージを受信するRPC
    /// </summary>
    [PunRPC]
    public void ReceiveBattleResult(string message)
    {
        Debug.Log("=== ReceiveBattleResult メソッドが呼び出されました ===");
        Debug.Log($"[Result] バトル結果を受信: {message}");
        Debug.Log($"[Result] メッセージの長さ: {message.Length}");
        Debug.Log($"[Result] メッセージの内容: '{message}'");
        
        // 勝敗メッセージの処理
        switch (message)
        {
            case "Win1":
                Debug.Log("[Result] Player1の勝利を処理 - Player2は敗北");
                ShowLoseResult();
                break;
            case "Lose1":
                Debug.Log("[Result] Player1の敗北を処理 - Player2は勝利");
                ShowWinResult();
                break;
            case "Win2":
                Debug.Log("[Result] Player2の勝利を処理 - Player1は敗北");
                ShowWinResult();
                break;
            case "Lose2":
                Debug.Log("[Result] Player2の敗北を処理 - Player1は勝利");
                ShowLoseResult();
                break;
            default:
                Debug.LogWarning($"[Result] 未知のバトル結果メッセージ: {message}");
                break;
        }
        
        Debug.Log("=== ReceiveBattleResult メソッド終了 ===");
    }

    [PunRPC]
    private void ReceiveAttribute(string attribute)
    {
        Debug.Log($"[Result] 属性情報を受信: {attribute}");
        // 属性情報の処理をここに実装
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsReading)
        {
            //Debug.Log("[Result] データを受信しました");
        }
    }

    void OnDestroy()
    {
        // シーン遷移時にPhotonViewを破棄
        if (photonView != null)
        {
            PhotonNetwork.Destroy(photonView.gameObject);
        }
    }
}
