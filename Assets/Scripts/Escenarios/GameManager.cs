using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks 
{
    void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            AparecerJugador();
        }
    }
    public override void OnJoinedRoom()
    {
        AparecerJugador();
    }

    void AparecerJugador()
    {
        if (PhotonNetwork.LocalPlayer.TagObject == null) 
        {
            Vector3 posicionAleatoria = new Vector3(Random.Range(-9f, 9f), 1f, Random.Range(1f, 19f));

            GameObject miJugador = PhotonNetwork.Instantiate("Player", posicionAleatoria, Quaternion.identity);
            PhotonNetwork.LocalPlayer.TagObject = miJugador;
        }
    }

    [PunRPC]
    public void RPC_SolicitarDestruccion(int viewId)
    {
        PhotonView targetView = PhotonView.Find(viewId);
        if (targetView != null)
        {
            PhotonNetwork.Destroy(targetView.gameObject);
        }
    }
}