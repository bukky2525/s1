using Photon.Pun;
using UnityEngine;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/*
 バトル終了受信
 Photonを使用したバトル終了通知の送受信クラス
 */

public class Battle : MonoBehaviourPunCallbacks
{
    private static Battle instance;
    private HandManager handManager;
    private const int OPPONENT_VIEW_ID = 1; // 相手のViewID
    private string currentAttribute = ""; // 現在の属性
    private GameObject currentCard;       // 現在表示中のカード
    private const string roomName = "TestRoom"; // ルーム名を定数として定義

    [Header("Fire Point Prefab")]
    [SerializeField] private GameObject firePoint;

    [Header("Water Point Prefab")]
    [SerializeField] private GameObject waterPoint;

    [Header("Grass Point Prefab")]
    [SerializeField] private GameObject grassPoint;

    private Dictionary<string, int> attributePointCounters = new Dictionary<string, int>();
    private Dictionary<string, GameObject> attributePointPrefabs = new Dictionary<string, GameObject>();
    
    // PointカードをCardAreaに生成するための参照
    private RectTransform cardAreaRect;

    void Awake()
    {
        // シングルトンパターンの実装
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // PhotonViewの設定確認
        if (photonView == null)
        {
            Debug.LogError("[Battle] PhotonView component is missing!");
        }
    }

    void Start()
    {
        handManager = FindObjectOfType<HandManager>();
        InitializeAttributePointCounters();
        InitializePointPrefabs();
        SetupCardArea(); // CardAreaの設定を追加
    }

    private void InitializeAttributePointCounters()
    {
        attributePointCounters["Fire"] = 0;
        attributePointCounters["Water"] = 0;
        attributePointCounters["Grass"] = 0;
    }

    private void InitializePointPrefabs()
    {
        // 各属性のプレハブを設定
        attributePointPrefabs["Fire"] = firePoint;
        attributePointPrefabs["Water"] = waterPoint;
        attributePointPrefabs["Grass"] = grassPoint;

        // プレハブの存在確認
        foreach (var prefab in attributePointPrefabs)
        {
            if (prefab.Value == null)
            {
                Debug.LogError($"[Battle] {prefab.Key}のポイントプレハブが設定されていません");
            }
        }
    }

    // CardAreaの設定（HandManagerと同様）
    private void SetupCardArea()
    {
        if (cardAreaRect == null)
        {
            // CardAreaオブジェクトを検索
            GameObject cardAreaObject = GameObject.Find("CardArea");
            if (cardAreaObject != null)
            {
                cardAreaRect = cardAreaObject.GetComponent<RectTransform>();
                if (cardAreaRect != null)
                {
                    Debug.Log("[Battle] CardAreaを検出しました - PointカードをCardAreaに生成します");
                }
                else
                {
                    Debug.LogWarning("[Battle] CardAreaオブジェクトにRectTransformが見つかりません");
                }
            }
            else
            {
                // CardAreaが見つからない場合はCanvasをフォールバック
                GameObject canvasObject = GameObject.Find("Canvas");
                if (canvasObject != null)
                {
                    cardAreaRect = canvasObject.GetComponent<RectTransform>();
                    Debug.Log("[Battle] CardAreaが見つからないため、Canvasをフォールバックとして使用します");
                }
                else
                {
                    Debug.LogError("[Battle] CardAreaもCanvasも見つかりません");
                }
            }
        }
    }

    // バトル終了を受信するRPCメソッド
    [PunRPC]
    private void ReceiveBattleFinish(bool isVictory, string message)
    {
        handManager = FindObjectOfType<HandManager>();
        if (handManager != null)
        {
            handManager.CreateHand();
        }
    }

    // ゲーム終了時のRPCメソッド
    //Pointの生成
    [PunRPC]
    private void OnGameEnd(string winner, string result1)
    {
        Debug.Log($"[Battle] ゲーム終了: 勝者={winner}, メッセージ={result1}");

        string attribute = "";
        if (result1.Contains("Fire"))
        {
            attribute = "Fire";
            Debug.Log($"[Battle] Fire属性を検出しました");
        }
        else if (result1.Contains("Water"))
        {
            attribute = "Water";
            Debug.Log($"[Battle] Water属性を検出しました");
        }
        else if (result1.Contains("Grass"))
        {
            attribute = "Grass";
            Debug.Log($"[Battle] Grass属性を検出しました");
        }

        if (!string.IsNullOrEmpty(attribute))
        {
            Debug.Log($"[Battle] 属性を設定: {attribute}");
            currentAttribute = attribute;
            UpdateAttributePosition(attribute);
            SpawnAttributePointCard(attribute);
        }
        else
        {
            Debug.Log($"[Battle] 未知の属性です: {result1}");
        }
    }

    // カード画像名を受信するRPCメソッド
    [PunRPC]
    private void ReceiveCardImageName(string result2, PhotonMessageInfo info)
    {
        // Debug.Log($"[Battle] メッセージを受信しました: {result2}");

        // // Player2の場合、_Point_2を含むメッセージのみを処理
        // if (!PhotonNetwork.IsMasterClient && !result2.Contains("_Point_2"))
        // {
        //     Debug.Log($"[Battle] Player2は_Point_2を含むメッセージのみを処理します");
        //     return;
        // }

        // // メッセージから属性部分のみを抽出
        // string attribute = "";
        // if (result2.StartsWith("Fire_Point_2"))
        // {
        //     attribute = "Fire";
        //     Debug.Log($"[Battle] Fire属性を抽出しました");
        // }
        // else if (result2.StartsWith("Water_Point_2"))
        // {
        //     attribute = "Water";
        //     Debug.Log($"[Battle] Water属性を抽出しました");
        // }
        // else if (result2.StartsWith("Grass_Point_2"))
        // {
        //     attribute = "Grass";
        //     Debug.Log($"[Battle] Grass属性を抽出しました");
        // }

        // if (!string.IsNullOrEmpty(attribute))
        // {
        //     Debug.Log($"[Battle] 属性を設定: {attribute}");
        //     currentAttribute = attribute;
        //     SpawnAttributePointCard(attribute);
        // }
        // else
        // {
        //     Debug.Log($"[Battle] 未知の属性です: {result2}");
        // }
    }

    // カード同期用のRPCメソッド
    [PunRPC]
    public void ReceiveCardSync(string result2)
    {
        // Player2の場合、result2または_Point_2を含むメッセージのみを処理
        if (!PhotonNetwork.IsMasterClient && !result2.Contains("result2") && !result2.Contains("_Point_2"))
        {
            Debug.Log($"[Battle] Player2はresult2または_Point_2を含むメッセージのみを処理します: {result2}");
            return;
        }

        Debug.Log($"[Battle] カード同期を受信: 結果={result2}");

        string attribute = "";
        if (result2.Contains("Fire"))
        {
            attribute = "Fire";
            Debug.Log($"[Battle] Fire属性を検出しました");
        }
        else if (result2.Contains("Water"))
        {
            attribute = "Water";
            Debug.Log($"[Battle] Water属性を検出しました");
        }
        else if (result2.Contains("Grass"))
        {
            attribute = "Grass";
            Debug.Log($"[Battle] Grass属性を検出しました");
        }

        if (!string.IsNullOrEmpty(attribute))
        {
            Debug.Log($"[Battle] 属性を設定: {attribute}");
            currentAttribute = attribute;
            UpdateAttributePosition(attribute);
            SpawnAttributePointCard(attribute);
        }
        else
        {
            Debug.Log($"[Battle] 未知の属性です: {result2}");
        }
    }

    // カード状態同期用のRPCメソッド
    [PunRPC]
    public void ReceiveCardState(string result2)
    {
        // Debug.Log($"[Battle] ReceiveCardState開始 - result2: {result2}");

        // // Hostからのメッセージを処理
        // if (PhotonNetwork.IsMasterClient)
        // {
        //     Debug.Log($"[Battle] Hostからのメッセージを受信: {result2}");
        //     string attribute = "";
        //     if (result2.Contains("Fire") || result2.Contains("Fire_Point_2"))
        //     {
        //         attribute = "Fire";
        //         Debug.Log($"[Battle] Fire属性を検出しました");
        //     }
        //     else if (result2.Contains("Water") || result2.Contains("Water_Point_2"))
        //     {
        //         attribute = "Water";
        //         Debug.Log($"[Battle] Water属性を検出しました");
        //     }
        //     else if (result2.Contains("Grass") || result2.Contains("Grass_Point_2"))
        //     {
        //         attribute = "Grass";
        //         Debug.Log($"[Battle] Grass属性を検出しました");
        //     }

        //     if (!string.IsNullOrEmpty(attribute))
        //     {
        //         Debug.Log($"[Battle] 属性を設定: {attribute}");
        //         currentAttribute = attribute;
        //         UpdateAttributePosition(attribute);
        //         SpawnAttributePointCard(attribute);
        //     }
        //     else
        //     {
        //         Debug.Log($"[Battle] 未知の属性です: {result2}");
        //     }
        //     return;
        // }

        // // 通常のresult2メッセージの処理
        // if (!result2.Contains("result2") && !result2.Contains("_Point_2"))
        // {
        //     Debug.Log($"[Battle] result2以外のメッセージを無視します: {result2}");
        //     return;
        // }
        // Debug.Log($"[Battle] result2メッセージを受信: {result2}");

        // string attribute2 = "";
        // if (result2.Contains("Fire") || result2.Contains("Fire_Point_2"))
        // {
        //     attribute2 = "Fire";
        //     Debug.Log($"[Battle] Fire属性を検出しました");
        // }
        // else if (result2.Contains("Water") || result2.Contains("Water_Point_2"))
        // {
        //     attribute2 = "Water";
        //     Debug.Log($"[Battle] Water属性を検出しました");
        // }
        // else if (result2.Contains("Grass") || result2.Contains("Grass_Point_2"))
        // {
        //     attribute2 = "Grass";
        //     Debug.Log($"[Battle] Grass属性を検出しました");
        // }

        // if (!string.IsNullOrEmpty(attribute2))
        // {
        //     Debug.Log($"[Battle] 属性を設定: {attribute2}");
        //     currentAttribute = attribute2;
        //     UpdateAttributePosition(attribute2);
        //     SpawnAttributePointCard(attribute2);
        // }
        // else
        // {
        //     Debug.Log($"[Battle] 未知の属性です: {result2}");
        // }
    }

    private void SpawnAttributePointCard(string attribute)
    {
        Debug.Log($"[Battle] ポイントカード生成開始: {attribute}");
        
        // カウンターを更新（上限3枚）
        attributePointCounters[attribute] = Mathf.Min(attributePointCounters[attribute] + 1, 3);
        Debug.Log($"[Battle] ポイント数: {attributePointCounters[attribute]}");

        // 勝利条件のチェック
        CheckVictoryCondition();

        // カードの位置を設定（重ねて表示）
        Vector3 basePosition = GetAttributePosition(attribute);
        // 獲得したポイント数に応じてY座標を下げる（0.5ずつ）
        float yOffset = (attributePointCounters[attribute] - 1) * 0.5f;
        Vector3 position = new Vector3(basePosition.x, basePosition.y - yOffset, basePosition.z);
        Debug.Log($"[Battle] 生成位置: {position}");

        // ポイントカードを生成
        if (attributePointPrefabs.ContainsKey(attribute) && attributePointPrefabs[attribute] != null)
        {
            Debug.Log($"[Battle] プレファブを生成: {attribute} Point");
            GameObject newCard = Instantiate(attributePointPrefabs[attribute], position, Quaternion.identity);
            
            // CardAreaを親として設定（HandManagerと同様）
            if (cardAreaRect != null)
            {
                newCard.transform.SetParent(cardAreaRect);
                Debug.Log($"[Battle] {attribute} PointをCardAreaに生成しました。位置: {position}");
            }
            else
            {
                // フォールバック：Battleオブジェクトを親にする
                newCard.transform.SetParent(transform);
                Debug.LogWarning($"[Battle] CardAreaが設定されていないため、Battleオブジェクトを親にしました");
            }
        }
        else
        {
            Debug.LogError($"[Battle] プレハブが見つかりません: {attribute} Point");
        }
    }

    private void CheckVictoryCondition()
    {
        // 各属性のポイント数を取得
        int firePoints = attributePointCounters["Fire"];
        int waterPoints = attributePointCounters["Water"];
        int grassPoints = attributePointCounters["Grass"];

        // 条件1: 各属性1つずつ
        bool hasAllAttributes = firePoints >= 1 && waterPoints >= 1 && grassPoints >= 1;

        // 条件2: いずれかの属性が3つ
        bool hasThreeOfOneAttribute = firePoints >= 3 || waterPoints >= 3 || grassPoints >= 3;

        // 完全勝利の判定
        if (hasAllAttributes || hasThreeOfOneAttribute)
        {
            Debug.Log("[Battle] 完全勝利！");
            if (hasAllAttributes)
            {
                Debug.Log("[Battle] 勝利条件: 全属性を獲得");
            }
            else
            {
                string winningAttribute = firePoints >= 3 ? "Fire" : (waterPoints >= 3 ? "Water" : "Grass");
                Debug.Log($"[Battle] 勝利条件: {winningAttribute}属性を3回獲得");
            }
        }
    }

    private Vector3 GetAttributePosition(string attribute)
    {
        Vector3 position = Vector3.zero;
        switch (attribute)
        {
            case "Fire":
                position = new Vector3(-7.5f, 3.7f, 0f);
                break;
            case "Water":
                position = new Vector3(-5.552f, 3.7f, 0f);
                break;
            case "Grass":
                position = new Vector3(-3.5f, 3.72f, 0f);
                break;
        }
        return position;
    }

    // 属性に応じて位置を更新するメソッド
    private void UpdateAttributePosition(string attribute)
    {
        Vector3 position = Vector3.zero;
        
        switch (attribute)
        {
            case "Fire":
                position.x = -5.5f;
                position.y = 5f;
                break;
            case "Water":
                position.x = -3.5f;
                position.y = 3f;
                break;
            case "Grass":
                position.x = -1.5f;
                position.y = 1f;
                break;
        }
    }

    // Player1勝利時のRPCメソッド
    [PunRPC]
    public void SendBattleResult1(string result1)
    {
        Debug.Log($"[Battle] SendBattleResult1 RPC開始 - result1: {result1}");
        Debug.Log($"[Battle] IsMasterClient: {PhotonNetwork.IsMasterClient}");
        
        // Player1勝利時の処理（属性ベース）
        string attribute = "";
        if (result1.Contains("Fire"))
        {
            attribute = "Fire";
            Debug.Log($"[Battle] Fire属性を検出しました");
        }
        else if (result1.Contains("Water"))
        {
            attribute = "Water";
            Debug.Log($"[Battle] Water属性を検出しました");
        }
        else if (result1.Contains("Grass"))
        {
            attribute = "Grass";
            Debug.Log($"[Battle] Grass属性を検出しました");
        }

        if (!string.IsNullOrEmpty(attribute))
        {
            Debug.Log($"[Battle] 属性を設定: {attribute}");
            currentAttribute = attribute;
            UpdateAttributePosition(attribute);
            SpawnAttributePointCard(attribute);
            
            // 属性ポイントの反映
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log($"[Battle] Player1のポイント更新処理開始");
                // Player1のポイントを更新
                if (!attributePointCounters.ContainsKey("Fire"))
                {
                    attributePointCounters["Fire"] = 0;
                    Debug.Log($"[Battle] Fire属性のカウンターを初期化");
                }
                attributePointCounters["Fire"]++;
                Debug.Log($"[Battle] Player1のポイントを更新: {attributePointCounters["Fire"]}");
                
                // ポイントの更新を全員に通知
                photonView.RPC("UpdatePoints", RpcTarget.All, attributePointCounters["Fire"], attributePointCounters["Water"], attributePointCounters["Grass"]);
                Debug.Log($"[Battle] UpdatePoints RPCを呼び出し");
            }
            else
            {
                Debug.Log($"[Battle] Player1ではないため、ポイント更新をスキップ");
            }
        }
        else
        {
            Debug.Log($"[Battle] 未知の属性です: {result1}");
        }
    }

    // Player2勝利時のRPCメソッド
    [PunRPC]
    public void SendBattleResult2(string result2)
    {
        Debug.Log($"SendBattleResult2 RPC開始 - result2: {result2}");
        
        // 勝敗メッセージの処理（Win2, Lose1など）
        if (result2 == "Win2" || result2 == "Lose1" || result2 == "Win1" || result2 == "Lose2")
        {
            Debug.Log($"[Battle] 勝敗メッセージを検出: {result2}");
            // Resultクラスに勝敗メッセージを送信
            var result = FindObjectOfType<Result>();
            if (result != null)
            {
                Debug.Log($"[Battle] Resultクラスに勝敗メッセージを送信: {result2}");
                result.ReceiveBattleResult(result2);
            }
            else
            {
                Debug.LogError("[Battle] Resultクラスが見つかりません");
            }
            return;
        }
        
        // Player2勝利時の処理（属性ベース）
        string attribute = "";
        if (result2.Contains("Fire"))
        {
            attribute = "Fire";
            Debug.Log($"[Battle] Fire属性を検出しました");
        }
        else if (result2.Contains("Water"))
        {
            attribute = "Water";
            Debug.Log($"[Battle] Water属性を検出しました");
        }
        else if (result2.Contains("Grass"))
        {
            attribute = "Grass";
            Debug.Log($"[Battle] Grass属性を検出しました");
        }

        if (!string.IsNullOrEmpty(attribute))
        {
            Debug.Log($"[Battle] 属性を設定: {attribute}");
            currentAttribute = attribute;
            UpdateAttributePosition(attribute);
            SpawnAttributePointCard(attribute);
            
            // 属性ポイントの反映
            if (!PhotonNetwork.IsMasterClient)
            {
                // Player2のポイントを更新
                attributePointCounters["Water"]++;
                Debug.Log($"[Battle] Player2のポイントを更新: {attributePointCounters["Water"]}");
                photonView.RPC("UpdatePoints", RpcTarget.All, attributePointCounters["Fire"], attributePointCounters["Water"], attributePointCounters["Grass"]);
            }
        }
        else
        {
           // Debug.Log($"[Battle] 未知の属性です: {result2}");
        }
    }

    void OnDestroy()
    {
        // シーン遷移時にPhotonViewを破棄
        if (photonView != null && photonView.IsMine)
        {
            PhotonNetwork.Destroy(photonView.gameObject);
        }
    }
}
