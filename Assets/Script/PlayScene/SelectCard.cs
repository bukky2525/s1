using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
 カードの選択機能
 ダブルクリック、ドラッグ＆ドロップ
 範囲外でのドロップの場合には
 削除を行うカード削除
 */

public class SelectCard : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private RectTransform rectTransform; // カードのRectTransformコンポーネント
    private Canvas canvas; // カードが配置されているキャンバス
    private Image image; // カードの画像コンポーネント
    private CardSendRPC cardSender; // CardSendRPCの参照

    private Vector2 initialPosition; // 初期の位置を記録
    private float lastClickTime = 0f; // 最後にクリックされた時間
    private const float doubleClickThreshold = 0.3f; // ダブルクリックと判定する時間間隔
    private bool isDragging = false; // ドラッグ中かどうかのフラグ

    public int cardIndex; // カードのインデックス

    // カードの初期位置を設定するメソッド
    public void SetInitialPosition(Vector2 position)
    {
        initialPosition = position; // 初期位置を記録
        transform.localPosition = position; // カードの位置を設定
    }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>(); // RectTransformコンポーネントを取得
        image = GetComponent<Image>(); // Imageコンポーネントを取得
        canvas = GetComponentInParent<Canvas>(); // 親のCanvasコンポーネントを取得
        initialPosition = rectTransform.anchoredPosition; // 初期位置を現在の位置に設定
        cardSender = FindObjectOfType<CardSendRPC>(); // CardSendRPCコンポーネントを取得
    }

    // マウスボタンが押されたときに呼ばれる
    public void OnPointerDown(PointerEventData eventData)
    {
        // ダブルクリックの判定
        if (Time.time - lastClickTime < doubleClickThreshold)
        {
            TryDelete("DoubleClick"); // ダブルクリックで削除を試みる
        }
        lastClickTime = Time.time; // クリック時間を更新
    }

    // ドラッグ中に呼ばれる
    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true; // ドラッグ中フラグを立てる

        Vector2 localPoint;
        // スクリーン座標をローカル座標に変換
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        rectTransform.anchoredPosition = localPoint; // カードの位置を更新
    }

    // マウスボタンが離されたときに呼ばれる
    public void OnPointerUp(PointerEventData eventData)
    {
        if (isDragging)
        {
            float y = rectTransform.anchoredPosition.y; // 現在のY座標を取得

            if (y > 150f)
            {
                TryDelete($"DragDrop Y={y}"); // 一定の高さを超えたら削除を試みる
            }
            else
            {
                // 元の位置に戻す
                rectTransform.anchoredPosition = initialPosition;
                Debug.Log($"[戻す] Y={y} の {image.sprite.name}");
            }
        }

        isDragging = false; // ドラッグ中フラグを解除
    }

    // カードを削除するメソッド
    private void TryDelete(string reason)
    {
        if (image != null && image.sprite != null)
        {
            string spriteName = image.sprite.name; // スプライト名を取得
            //Debug.Log($"[{reason}] 削除: {spriteName}");

            // 削除を通知
            if (cardSender != null)
            {
                cardSender.SendCardImageName(spriteName); // スプライト名を送信
            }
            else
            {
                Debug.LogWarning("[SelectCard] CardSendRPCコンポーネントが見つかりません。カードの送信ができません。");
            }
        }
        else
        {
            Debug.Log($"[{reason}] 削除: 不明な画像");
        }

        Destroy(gameObject); // ゲームオブジェクトを削除
    }

    // 初期位置を取得するメソッド
    public Vector2 GetInitialPosition()
    {
        return initialPosition;
    }
}
