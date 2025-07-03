using UnityEngine;
using Photon.Pun;

/*
 カード送信機能
 Photonを使用してカードの画像名を他のプレイヤーに送信するクラス
*/ 

public class CardSendRPC : MonoBehaviourPun
{
    [Header("効果音設定")]
    public AudioSource audioSource;             // 効果音用AudioSource
    public string sendSoundPath = "SE/Card_Kubaru";  // カード送信音のパス（Resources内）

    private HandManager handManager;
    private PhotonView _photonView;
    private AudioClip sendSoundClip; // カード送信効果音

    void Awake()
    {
        // PhotonViewコンポーネントを取得
        _photonView = GetComponent<PhotonView>();
        if (_photonView == null)
        {
            Debug.LogError("[CardSendRPC] PhotonViewコンポーネントが見つかりません。コンポーネントを追加してください。");
        }
    }

    void Start()
    {
        handManager = FindObjectOfType<HandManager>();
        LoadSoundEffect(); // 効果音を読み込み
        SetupAudioSource(); // AudioSourceをセットアップ
    }

    // 効果音を読み込み
    void LoadSoundEffect()
    {
        sendSoundClip = Resources.Load<AudioClip>(sendSoundPath);
        if (sendSoundClip == null)
        {
            Debug.LogWarning($"[CardSendRPC] カード送信音 '{sendSoundPath}' が見つかりません。Resources/{sendSoundPath}.wav(mp3) に音声ファイルがあることを確認してください。");
        }
        else
        {
            Debug.Log($"[CardSendRPC] カード送信音を読み込みました: {sendSoundPath}");
        }
    }

    // AudioSourceをセットアップ
    void SetupAudioSource()
    {
        if (audioSource == null)
        {
            // AudioSourceが設定されていない場合は自動で追加
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                Debug.Log("[CardSendRPC] AudioSourceを自動追加しました");
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

    // カード画像名を送信するメソッド
    public void SendCardImageName(string cardImageName)
    {
        if (_photonView == null)
        {
            Debug.LogError("[CardSendRPC] PhotonViewが初期化されていません。");
            return;
        }

        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            Debug.Log($"[CardSendRPC] カード画像名を送信します: {cardImageName}");
            
            // カードを非表示にする
            if (handManager != null)
            {
                Debug.Log("[CardSendRPC] カードを非表示にします");
                handManager.HideCards();
                
                // カード削除と同時に送信効果音を再生
                PlaySoundEffect(sendSoundClip, 0.8f);
                Debug.Log("[CardSendRPC] カード送信効果音を再生しました");
            }
            else
            {
                Debug.LogError("[CardSendRPC] HandManagerが見つかりません。カードを非表示にできません。");
            }
            
            // タイマーと点滅を非表示にする
            CountDown countDown = FindObjectOfType<CountDown>();
            if (countDown != null)
            {
                countDown.HideTimer();
                countDown.HideBlink();
                Debug.Log("[CardSendRPC] カード送信後、タイマーと点滅を非表示にしました");
            }

            // カード画像名を他のプレイヤーに送信
            _photonView.RPC("ReceiveCardImageName", RpcTarget.Others, cardImageName);
        }
        else
        {
            Debug.LogWarning("[CardSendRPC] Photonに接続されていないか、ルームに参加していないため、カード画像名を送信できません");
        }
    }
}
