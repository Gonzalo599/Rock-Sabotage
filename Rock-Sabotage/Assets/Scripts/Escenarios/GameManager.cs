using UnityEngine;
using Photon.Pun;

// Cambiamos a MonoBehaviourPunCallbacks para poder escuchar los eventos de la sala
public class GameManager : MonoBehaviourPunCallbacks 
{
    void Start()
    {
        // Si cargamos la escena y ya estamos adentro de la sala, lo hacemos aparecer
        if (PhotonNetwork.InRoom)
        {
            AparecerJugador();
        }
    }

    // Si la escena cargó rapidísimo y la red venía demorada, este evento nos salva:
    // Se ejecuta automáticamente en el instante exacto en que por fin entramos a la sala
    public override void OnJoinedRoom()
    {
        AparecerJugador();
    }

    void AparecerJugador()
    {
        // Chequeamos que no hayamos creado ya un jugador para no duplicarlo por error
        if (PhotonNetwork.LocalPlayer.TagObject == null) 
        {
            Vector3 posicionAleatoria = new Vector3(Random.Range(-9f, 9f), 1f, Random.Range(1f, 19f));
            
            // Instanciamos la cápsula
            GameObject miJugador = PhotonNetwork.Instantiate("Player", posicionAleatoria, Quaternion.identity);
            
            // Le ponemos una "etiqueta" invisible para avisarle a la red que ya spawneamos
            PhotonNetwork.LocalPlayer.TagObject = miJugador;
        }
    }

    [PunRPC]
    public void RPC_SolicitarDestruccion(int viewId)
    {
        // Solo el Master puede entrar aquí porque el RPC fue enviado a RpcTarget.MasterClient
        PhotonView targetView = PhotonView.Find(viewId);
        if (targetView != null)
        {
            PhotonNetwork.Destroy(targetView.gameObject);
        }
    }
}