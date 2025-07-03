using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/*
 カードネットワーク管理
 Photonを使用してカードの同期を管理するクラス
 */

public class CardNetworkManager : MonoBehaviourPun
{
    private static CardNetworkManager instance;

    void Awake()
    {
        // シングルトンパターンの実装
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // PhotonViewの設定を確認
        if (photonView == null)
        {
            Debug.LogError("[CardNetworkManager] PhotonView component is missing!");
        }
        else
        {
            Debug.Log($"[CardNetworkManager] PhotonView ID: {photonView.ViewID}");
        }
    }

    void OnDestroy()
    {
        // シーン遷移のイベントを解除
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // シーン遷移時に呼ばれる
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[CardNetworkManager] シーン '{scene.name}' がロードされました");
    }

    // カードのネットワーク同期を行うメソッド
    public void SyncCard(string cardId, Vector3 position, Quaternion rotation)
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            photonView.RPC("ReceiveCardSync", RpcTarget.Others, cardId, position, rotation);
        }
    }

    // カードの同期を受信するRPCメソッド
    [PunRPC]
    private void ReceiveCardSync(string cardId, Vector3 position, Quaternion rotation)
    {
        Debug.Log($"CardNetworkManager: カード '{cardId}' の同期を受信しました");
        // ここにカードの同期処理を記述
    }

    // カードの状態を更新するメソッド
    public void UpdateCardState(string cardId, object state)
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            photonView.RPC("ReceiveCardState", RpcTarget.Others, cardId, state);
        }
    }

    // カードの状態更新を受信するRPCメソッド
    [PunRPC]
    private void ReceiveCardState(string cardId, object state)
    {
        Debug.Log($"CardNetworkManager: カード '{cardId}' の状態更新を受信しました");
        // ここにカードの状態更新処理を記述
    }
} 