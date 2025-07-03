using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

/*
 ルームへの接続（Playシーン専用）
 */

public class PhotonStarter : MonoBehaviourPunCallbacks
{
    public string roomName = "TestRoom";
    private string playerName = "Player2";

    private bool isReadyToJoinRoom = false;

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "PlayScene")
        {
            InitializePhoton();
            // ここでは直接JoinRoomを呼ばず、フラグだけ立てる
            isReadyToJoinRoom = true;
            TryJoinRoom();
        }
        else
        {
            // PlayScene以外になったらフラグを下ろす
            isReadyToJoinRoom = false;
        }
    }

    private void InitializePhoton()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.LocalPlayer.NickName = playerName;

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    private void TryJoinRoom()
    {
        if (isReadyToJoinRoom && PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.InRoom)
        {
            PhotonNetwork.JoinRoom(roomName);
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("マスターサーバーに接続されました");
        TryJoinRoom();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    }


    // プレイヤーネームを設定するメソッド
    public void SetPlayerName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }
        playerName = name;
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
        }
    }

    // 現在のプレイヤーネームを取得するメソッド
    public string GetPlayerName()
    {
        return playerName;
    }

    // ルーム参加に失敗したときに呼ばれる
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        // Playシーン以外では動作しないようにする
        if (SceneManager.GetActiveScene().name != "PlayScene")
        {
            return;
        }

        // ルームが存在しない場合は新しく作成
        if (returnCode == ErrorCode.GameDoesNotExist)
        {
            RoomOptions roomOptions = new RoomOptions
            {
                MaxPlayers = 4, // 最大プレイヤー数
                IsVisible = true, // ルームを表示
                IsOpen = true // ルームをオープン
            };
            
            PhotonNetwork.CreateRoom(roomName, roomOptions);
        }
    }

    // ルーム作成に失敗したときに呼ばれる
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
    }

    // ルームに参加したときに呼ばれる
    public override void OnJoinedRoom()
    {
        Debug.Log("ルームに参加しました");
        var loading = FindObjectOfType<Loading>();
        if (loading != null)
        {
            loading.HideLoadingAndDeal();
        }
        else
        {
            Debug.LogWarning("Loadingクラスが見つかりませんでした");
        }
    }

    // 接続が切断されたときに呼ばれる
    public override void OnDisconnected(DisconnectCause cause)
    {
    }

    // プレイヤーがルームに入室したときに呼ばれる
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
    }

    // プレイヤーがルームから退室したときに呼ばれる
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
    }
}