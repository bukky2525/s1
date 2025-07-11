using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening; // DOTween を追加

/*
 主な機能
カード画像の読み込み：Resources/Gazou フォルダからカード画像（Sprite）を読み込みます。

デッキの生成と保存：

初回起動時には全スプライトからデッキを作成します。

プレイ中に使用したカードは削除し、デッキの状態を PlayerPrefs に保存します。

手札の作成：

デッキから一定数のカードを取り出し、指定位置に表示します。

シーンをまたいだデッキの保持：

DontDestroyOnLoad によって、シーン遷移後も HandManager オブジェクトを保持します。

保存された PlayerPrefs からデッキの状態を復元できます。

 
 */

public class HandManager : MonoBehaviour
{
    public static HandManager Instance { get; private set; }

    public GameObject cardPrefab;          // カードのプレハブ
    public RectTransform cardAreaRect;     // カードを配置するCardArea
    public static int selectedCardIndex = -1;

    [Header("カード設定")]
    public int cardCount = 5;               // 配るカードの枚数を確認
    public float cardWidth = 250f;          // カードの幅
    public float cardHeight = 350f;         // カードの高さ
    
    [Header("アニメーション設定")]
    public float dealAnimationDuration = 1.0f;  // 配布アニメーションの時間
    public float flipAnimationDuration = 0.5f;  // 反転アニメーションの時間
    public float dealDelay = 0.1f;              // 各カードの配布間隔

    [Header("効果音設定")]
    public AudioSource audioSource;             // 効果音用AudioSource
    public string dealSoundPath = "SE/Card_Kubaru";  // カード配布音のパス（Resources内）
    public string flipSoundPath = "SE/Card_Mekuru";  // カード反転音のパス（Resources内）

    private List<Sprite> cardSprites = new List<Sprite>(); // 全カード画像リスト
    private List<CardData> deck = new List<CardData>();     // デッキデータ
    private List<CardData> currentHand = new List<CardData>();
    private Sprite backCardSprite; // 裏面画像
    private AudioClip dealSoundClip; // カード配布効果音
    private AudioClip flipSoundClip; // カード反転効果音
    
    // 固定の属性位置（左から3枚）
    private readonly CardAttribute[] fixedAttributes = new CardAttribute[]
    {
        CardAttribute.Fire,
        CardAttribute.Water,
        CardAttribute.Grass
    };

    private List<Sprite> usedSprites = new List<Sprite>();
    private List<string> lastHandCardNames = new List<string>();
    private List<CardData> allCards = new List<CardData>();

    private bool isFirstHand = true;
    private bool isAnimating = false; // アニメーション中フラグ

    // PlayScene用の設定
    private void SetupForPlayScene()
    {
        // Debug.Log("HandManager: PlayScene用の設定を行います");
        
        // CardAreaの参照を更新
        SetupCardArea();
        
        // 必要に応じて手札を再表示
        if (currentHand.Count > 0)
        {
            DisplayCurrentHand();
        }
    }

    // CardAreaを検索・設定する
    private void SetupCardArea()
    {
        // まずCardAreaという名前のGameObjectを検索
        GameObject cardAreaObject = GameObject.Find("CardArea");
        
        if (cardAreaObject != null)
        {
            cardAreaRect = cardAreaObject.GetComponent<RectTransform>();
            if (cardAreaRect != null)
            {
                Debug.Log("[HandManager] CardAreaを検出しました: " + cardAreaObject.name);
            }
            else
            {
                Debug.LogWarning("[HandManager] CardAreaにRectTransformがありません");
            }
        }
        
        // CardAreaが見つからない場合はCanvasをフォールバックとして使用
        if (cardAreaRect == null)
        {
            Debug.LogWarning("[HandManager] CardAreaが見つかりません。Canvasを代わりに使用します");
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                cardAreaRect = canvas.GetComponent<RectTransform>();
                Debug.Log("[HandManager] フォールバック: Canvasを使用します");
            }
            else
            {
                Debug.LogError("[HandManager] CanvasもCardAreaも見つかりません");
            }
        }
    }

    // 現在の手札を表示
    private void DisplayCurrentHand()
    {
        // Debug.Log($"HandManager: 現在の手札（{currentHand.Count}枚）を表示します");
        
        if (cardAreaRect == null)
        {
            Debug.LogError("[HandManager] cardAreaRectがnullです");
            return;
        }

        Vector2[] positions = new Vector2[]
        {
            new Vector2(-600f, -315f), // カード1の位置
            new Vector2(-300f, -315f), // カード2の位置
            new Vector2(   0f, -315f), // カード3の位置
            new Vector2( 300f, -315f), // カード4の位置
            new Vector2( 600f, -315f)  // カード5の位置
        };

        // 手札のカードをインスタンス化してCardArea内に配置
        for (int i = 0; i < currentHand.Count; i++)
        {
            GameObject card = Instantiate(cardPrefab, cardAreaRect); // CardArea内にカードのインスタンスを生成
            RectTransform rt = card.GetComponent<RectTransform>(); // RectTransformコンポーネントを取得
            rt.sizeDelta = new Vector2(cardWidth, cardHeight); // カードのサイズを設定
            rt.anchoredPosition = positions[i]; // カードの位置を設定
            card.GetComponent<Image>().sprite = currentHand[i].cardImage; // カードの画像を設定
        }
    }

    void Start()
    {
        LoadCardSprites();
        LoadBackCardSprite(); // 裏面画像を読み込み
        LoadSoundEffects(); // 効果音を読み込み
        SetupAudioSource(); // AudioSourceをセットアップ
        SetupCardArea(); // CardAreaを設定
        CreateAllCards();
        CreateHand();
    }

    // 裏面画像を読み込み
    void LoadBackCardSprite()
    {
        backCardSprite = Resources.Load<Sprite>("Ura");
        if (backCardSprite == null)
        {
            Debug.LogError("[HandManager] 裏面画像 'Ura' が見つかりません。Resources/Ura に画像があることを確認してください。");
        }
        else
        {
            Debug.Log("[HandManager] 裏面画像を読み込みました");
        }
    }

    // 効果音を読み込み
    void LoadSoundEffects()
    {
        // カード配布音を読み込み
        dealSoundClip = Resources.Load<AudioClip>(dealSoundPath);
        if (dealSoundClip == null)
        {
            Debug.LogWarning($"[HandManager] カード配布音 '{dealSoundPath}' が見つかりません。Resources/{dealSoundPath}.wav(mp3) に音声ファイルがあることを確認してください。");
        }
        else
        {
            Debug.Log($"[HandManager] カード配布音を読み込みました: {dealSoundPath}");
        }

        // カード反転音を読み込み
        flipSoundClip = Resources.Load<AudioClip>(flipSoundPath);
        if (flipSoundClip == null)
        {
            Debug.LogWarning($"[HandManager] カード反転音 '{flipSoundPath}' が見つかりません。Resources/{flipSoundPath}.wav(mp3) に音声ファイルがあることを確認してください。");
        }
        else
        {
            Debug.Log($"[HandManager] カード反転音を読み込みました: {flipSoundPath}");
        }
    }

    // AudioSourceをセットアップ
    void SetupAudioSource()
    {
        // AudioListenerの重複チェックと整理
        AudioListener[] allListeners = FindObjectsOfType<AudioListener>();
        Debug.Log($"[HandManager] 現在のAudioListener数: {allListeners.Length}");

        if (allListeners.Length == 0)
        {
            // AudioListenerが見つからない場合は追加
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                AudioListener listener = mainCamera.gameObject.AddComponent<AudioListener>();
                Debug.Log("[HandManager] メインカメラにAudioListenerを追加しました");
            }
            else
            {
                Camera anyCamera = FindObjectOfType<Camera>();
                if (anyCamera != null)
                {
                    AudioListener listener = anyCamera.gameObject.AddComponent<AudioListener>();
                    Debug.Log($"[HandManager] カメラ '{anyCamera.name}' にAudioListenerを追加しました");
                }
                else
                {
                    AudioListener listener = gameObject.AddComponent<AudioListener>();
                    Debug.Log("[HandManager] HandManagerにAudioListenerを追加しました");
                }
            }
        }
        else if (allListeners.Length > 1)
        {
            // AudioListenerが複数ある場合は1つを残して削除
            Debug.LogWarning($"[HandManager] AudioListenerが{allListeners.Length}個見つかりました。1つを残して削除します。");
            
            // 優先順位: メインカメラ > 他のカメラ > その他
            AudioListener keepListener = null;
            
            // まずメインカメラのListenerを探す
            foreach (var listener in allListeners)
            {
                if (listener.GetComponent<Camera>() != null && listener.GetComponent<Camera>() == Camera.main)
                {
                    keepListener = listener;
                    break;
                }
            }
            
            // メインカメラにない場合は、任意のカメラのListenerを探す
            if (keepListener == null)
            {
                foreach (var listener in allListeners)
                {
                    if (listener.GetComponent<Camera>() != null)
                    {
                        keepListener = listener;
                        break;
                    }
                }
            }
            
            // カメラにもない場合は最初のListenerを残す
            if (keepListener == null)
            {
                keepListener = allListeners[0];
            }
            
            // 残すListener以外を削除
            int deletedCount = 0;
            foreach (var listener in allListeners)
            {
                if (listener != keepListener)
                {
                    Debug.Log($"[HandManager] 重複AudioListenerを削除: {listener.name}");
                    DestroyImmediate(listener);
                    deletedCount++;
                }
            }
            
            Debug.Log($"[HandManager] {deletedCount}個のAudioListenerを削除しました。残ったAudioListener: {keepListener.name}");
        }
        else
        {
            Debug.Log($"[HandManager] AudioListenerが適切に1つ存在します: {allListeners[0].name}");
        }

        if (audioSource == null)
        {
            // AudioSourceが設定されていない場合は自動で追加
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                Debug.Log("[HandManager] AudioSourceを自動追加しました");
            }
        }

        // AudioSourceの基本設定
        audioSource.playOnAwake = false;
        audioSource.volume = 0.7f; // 音量を70%に設定
    }

    // 効果音を再生
    void PlaySoundEffect(AudioClip clip, float volumeScale = 1.0f)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volumeScale);
        }
    }

    private IEnumerator ShowCardsWithDelay()
    {
        yield return new WaitForSeconds(2f);
        CreateHand();
    }

    void CreateAllCards()
    {
        allCards.Clear();
        foreach (var sprite in cardSprites)
        {
            CardAttribute attr = CardAttribute.Fire;
            if (sprite.name.Contains("Water")) attr = CardAttribute.Water;
            else if (sprite.name.Contains("Grass")) attr = CardAttribute.Grass;
            // 画像名から属性を判定
            allCards.Add(new CardData { attribute = attr, cardImage = sprite });
        }
    }

    void CreateRandomDeck()
    {
        deck.Clear();
        usedSprites.Clear();

        // 固定の3属性を追加
        foreach (var attribute in fixedAttributes)
        {
            AddCardToDeck(attribute);
        }

        // ランダムで2枚属性を追加
        for (int i = 0; i < 2; i++)
        {
            // Fire, Water, Grass からランダムに選ぶ
            CardAttribute randomAttr = (CardAttribute)Random.Range(0, 3);
            AddCardToDeck(randomAttr);
        }

        // Debug.Log("Deck size: " + deck.Count);
    }

    private void AddCardToDeck(CardAttribute targetAttribute)
    {
        // 属性に合致する未使用のスプライトを探す
        Sprite selectedSprite = null;
        List<Sprite> matchingSprites = new List<Sprite>();

        // 属性に合致するスプライトをすべて集める
        foreach (var sprite in cardSprites)
        {
            if ((targetAttribute == CardAttribute.Fire && sprite.name.Contains("Fire")) ||
                (targetAttribute == CardAttribute.Water && sprite.name.Contains("Water")) ||
                (targetAttribute == CardAttribute.Grass && sprite.name.Contains("Grass")))
            {
                if (!usedSprites.Contains(sprite))
                {
                    matchingSprites.Add(sprite);
                }
            }
        }

        // 合致するスプライトからランダムに1つ選択
        if (matchingSprites.Count > 0)
        {
            selectedSprite = matchingSprites[Random.Range(0, matchingSprites.Count)];
        }
        else
        {
            // 合致するスプライトが見つからない場合は、未使用のスプライトからランダムに選択
            do
            {
                selectedSprite = cardSprites[Random.Range(0, cardSprites.Count)];
            } while (usedSprites.Contains(selectedSprite));
        }

        usedSprites.Add(selectedSprite);
        deck.Add(new CardData { attribute = targetAttribute, cardImage = selectedSprite });
    }

    // Gazouフォルダからカードスプライトを全て読み込む
    void LoadCardSprites()
    {
        Sprite[] loadedSprites = Resources.LoadAll<Sprite>("Gazou");
        cardSprites = loadedSprites.ToList();
    }

    // デッキ作成（スプライトからCardDataを作る）
    void CreateDeck() { /* 削除 */ }

    // デッキを保存
    void SaveDeckState() { /* 削除 */ }

    // デッキを読み込み
    bool LoadDeckState() { return false; /* 削除 */ }

    // カードを非表示にするメソッド
    public void HideCards()
    {
        if (isAnimating)
        {
            Debug.Log("[HandManager] アニメーション中のため、カード削除をスキップします");
            return;
        }

        bool anyCardHidden = false;
        // CardArea内のカードを削除
        if (cardAreaRect != null)
        {
            foreach (Transform child in cardAreaRect)
            {
                if (child.GetComponent<SelectCard>() != null)
                {
                    Destroy(child.gameObject);
                    anyCardHidden = true;
                }
            }
        }

        if (anyCardHidden)
        {
            Debug.Log("[HandManager] CardArea内のカードを削除しました");
        }
        else
        {
            Debug.Log("[HandManager] 削除するカードが見つかりませんでした");
        }
    }

    // カードを表示するメソッド
    public void ShowCards()
    {
        if (isAnimating)
        {
            Debug.Log("[HandManager] アニメーション中のため、新しいカード生成をスキップします");
            return;
        }
        // 新しい手札を作成
        CreateHand();
    }

    private int WeightedRandomNumber()
    {
        float rand = Random.value;
        if (rand < 0.5f) return Random.Range(1, 7); // 1-6: 50%
        else if (rand < 0.8f) return Random.Range(7, 9); // 7-8: 30%
        else if (rand < 0.95f) return 9; // 9: 15%
        else return 10; // 10: 5%
    }

    private CardData PickCard(List<CardData> candidates)
    {
        // 数字ごとの重みを考慮してランダム選択
        List<CardData> weighted = new List<CardData>();
        foreach (var card in candidates)
        {
            string name = card.cardImage.name;
            int num = 1;
            // 画像名の末尾から数字を取得
            for (int i = name.Length - 1; i >= 0; i--)
            {
                if (char.IsDigit(name[i]))
                {
                    int.TryParse(name.Substring(i), out num);
                    break;
                }
            }
            // 重み付け
            int weight = 1;
            if (num >= 1 && num <= 6) weight = 50;
            else if (num == 7 || num == 8) weight = 30;
            else if (num == 9) weight = 15;
            else if (num == 10) weight = 5;
            for (int w = 0; w < weight; w++) weighted.Add(card);
        }
        if (weighted.Count == 0) return null;
        return weighted[Random.Range(0, weighted.Count)];
    }

    // 手札を作成（アニメーション付き）
    public void CreateHand()
    {
        if (isFirstHand)
        {
            // 最初の1回目はLoadingから呼ばれるので、ここでは何もしない
            isFirstHand = false;
            return;
        }

        if (cardAreaRect == null)
        {
            Debug.LogError("[HandManager] cardAreaRectがnullです。カードを表示できません。");
            return;
        }

        if (isAnimating)
        {
            Debug.Log("[HandManager] 既にアニメーション中です");
            return;
        }

        StartCoroutine(CreateHandWithAnimation());
    }

    // アニメーション付きでカードを配る
    private IEnumerator CreateHandWithAnimation()
    {
        isAnimating = true;

        currentHand.Clear();

        // 1枚目：火
        var fireCards = allCards.Where(card => card.attribute == CardAttribute.Fire).ToList();
        currentHand.Add(PickCard(fireCards));

        // 2枚目：火 or 水
        var fireWaterCards = allCards.Where(card => card.attribute == CardAttribute.Fire || card.attribute == CardAttribute.Water).ToList();
        currentHand.Add(PickCard(fireWaterCards));

        // 3枚目：水
        var waterCards = allCards.Where(card => card.attribute == CardAttribute.Water).ToList();
        currentHand.Add(PickCard(waterCards));

        // 4枚目：水 or 草
        var waterGrassCards = allCards.Where(card => card.attribute == CardAttribute.Water || card.attribute == CardAttribute.Grass).ToList();
        currentHand.Add(PickCard(waterGrassCards));

        // 5枚目：草
        var grassCards = allCards.Where(card => card.attribute == CardAttribute.Grass).ToList();
        currentHand.Add(PickCard(grassCards));

        Debug.Log($"[HandManager] アニメーション付きで新しい手札を配ります（{currentHand.Count}枚）");

        Vector2[] targetPositions = new Vector2[]
        {
            new Vector2(-600f, -315f),
            new Vector2(-300f, -315f),
            new Vector2(   0f, -315f),
            new Vector2( 300f, -315f),
            new Vector2( 600f, -315f)
        };

        // 画面中央の位置
        Vector2 centerPosition = Vector2.zero;
        List<GameObject> backCards = new List<GameObject>();

        // 裏面カードを中央に5枚重ねてCardArea内に生成
        for (int i = 0; i < currentHand.Count; i++)
        {
            GameObject backCard = Instantiate(cardPrefab, cardAreaRect);
            RectTransform rt = backCard.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(cardWidth, cardHeight);
            rt.anchoredPosition = centerPosition;
            
            // 裏面画像を設定
            Image cardImage = backCard.GetComponent<Image>();
            if (backCardSprite != null)
            {
                cardImage.sprite = backCardSprite;
            }
            
            // SelectCardコンポーネントを無効化（アニメーション中は選択不可）
            var selectCard = backCard.GetComponent<SelectCard>();
            if (selectCard != null)
            {
                selectCard.enabled = false;
            }

            backCards.Add(backCard);
        }

        yield return new WaitForSeconds(0.5f); // 少し待つ

                 // 各カードを目標位置に移動
        for (int i = 0; i < backCards.Count; i++)
        {
            int cardIndex = i; // ローカル変数でキャプチャ
            GameObject backCard = backCards[i];
            RectTransform rt = backCard.GetComponent<RectTransform>();

            // 配布音を再生（少し小さめの音量で）
            PlaySoundEffect(dealSoundClip, 0.5f);

            // 目標位置に移動
            rt.DOAnchorPos(targetPositions[i], dealAnimationDuration)
                .SetDelay(i * dealDelay)
                .SetEase(Ease.OutQuart);

            yield return new WaitForSeconds(dealDelay); // 次のカードまで少し待つ
        }

        // 全てのカードが移動完了するまで待つ
        yield return new WaitForSeconds(dealAnimationDuration);

        // カードを反転して表面に変更
        for (int i = 0; i < backCards.Count; i++)
        {
            int cardIndex = i; // ローカル変数でキャプチャ（クロージャー問題を回避）
            GameObject backCard = backCards[i];
            RectTransform rt = backCard.GetComponent<RectTransform>();
            Image cardImage = backCard.GetComponent<Image>();

            // 反転音を再生
            PlaySoundEffect(flipSoundClip, 0.6f);

            // Y軸回転で反転アニメーション
            Sequence flipSequence = DOTween.Sequence();
            
            // 90度まで回転（裏面が見えなくなる）
            flipSequence.Append(rt.DORotate(new Vector3(0, 90, 0), flipAnimationDuration / 2))
                .AppendCallback(() =>
                {
                    // 画像を表面に変更（正しいインデックスを使用）
                    if (cardIndex < currentHand.Count && currentHand[cardIndex] != null)
                    {
                        cardImage.sprite = currentHand[cardIndex].cardImage;
                        Debug.Log($"[HandManager] カード{cardIndex}を表面に反転: {currentHand[cardIndex].cardImage.name}");
            }
            else
                    {
                        Debug.LogError($"[HandManager] カード{cardIndex}の反転に失敗: インデックス範囲外またはnull");
                    }
                })
                // 0度まで回転（表面が見える）
                .Append(rt.DORotate(new Vector3(0, 0, 0), flipAnimationDuration / 2));

            yield return new WaitForSeconds(0.1f); // 各カードの反転間隔
        }

        // 全ての反転アニメーションが完了するまで待つ
        yield return new WaitForSeconds(flipAnimationDuration);

        // SelectCardコンポーネントを有効化し、初期位置とインデックスを設定
        for (int i = 0; i < backCards.Count; i++)
        {
            var selectCard = backCards[i].GetComponent<SelectCard>();
            if (selectCard != null)
            {
                selectCard.enabled = true;
                selectCard.SetInitialPosition(targetPositions[i]);
                selectCard.cardIndex = i;
            }
        }

        Debug.Log($"[HandManager] カード配布アニメーション完了");
        
        // 新しいカード生成時にタイマーを再開
        CountDown countDown = FindObjectOfType<CountDown>();
        if (countDown != null)
        {
            countDown.RestartTimerForNewCards();
            Debug.Log("[HandManager] 新しいカード生成後、タイマーを再開しました");
        }
        else
        {
            Debug.LogWarning("[HandManager] CountDownが見つからないため、タイマーを再開できません");
        }

        isAnimating = false;
    }

    // Loadingから最初の1回目だけ直接呼ぶ用
    public void CreateHandFirst()
    {
        // 1回目のカード配り処理
        // ...（既存のCreateHandの処理）...
        isFirstHand = false;
    }

    public void ReplaceUsedCard(int usedIndex)
    {
        // 使用済みのカードをデッキから新しいカードで置き換える
        if (usedIndex >= 0 && usedIndex < currentHand.Count && deck.Count > 0)
        {
            currentHand[usedIndex] = deck[0]; // デッキの先頭のカードで置き換え
            deck.RemoveAt(0); // デッキから取得したカードを削除
            SaveDeckState(); // デッキの状態を保存
        }
    }

    // カードの属性を定義
    public enum CardAttribute
    {
        Fire,  // 火属性
        Water, // 水属性
        Grass  // 草属性
    }

    // カードのデータ構造
    [System.Serializable]
    public class CardData
    {
        public CardAttribute attribute; // カードの属性
        public Sprite cardImage;        // カードの画像
    }

    private CardAttribute WeightedRandomAttribute(CardAttribute[] attrs)
    {
        float rand = Random.value;
        if (attrs.Length == 2)
        {
            // 火・水 or 水・草
            if (attrs.Contains(CardAttribute.Fire) && attrs.Contains(CardAttribute.Water))
            {
                // 火75%, 水25%
                if (rand < 0.75f) return CardAttribute.Fire;
                else return CardAttribute.Water;
            }
            else if (attrs.Contains(CardAttribute.Water) && attrs.Contains(CardAttribute.Grass))
            {
                // 草75%, 水25%
                if (rand < 0.25f) return CardAttribute.Water;
                else return CardAttribute.Grass;
            }
            else if (attrs.Contains(CardAttribute.Fire) && attrs.Contains(CardAttribute.Grass))
            {
                // 火75%, 草25%
                if (rand < 0.75f) return CardAttribute.Fire;
                else return CardAttribute.Grass;
            }
        }
        // 1属性だけ
        return attrs[0];
    }
}
