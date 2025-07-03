using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

/*
 参加、退出確認
 */

public class RoomManager : MonoBehaviourPunCallbacks
{
    // ルームにプレイヤーが入ったときに呼ばれる
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " がルームに入ってきました！");
    }

    // 他にも、プレイヤーが退出した時などの処理も可能
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log(otherPlayer.NickName + " がルームから退出しました！");
    }
}
