using UnityEngine;
using Photon.Pun; // Necesario para la red

public class GestorLobby : MonoBehaviour
{
    [Header("Configuración de Aparición")]
    public string nombrePrefabJugador = "Jugador";

    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            AparecerJugador();
        }
    }

    void AparecerJugador()
    {
        float xAleatorio = Random.Range(-4f, 6f);
        float zAleatorio = Random.Range(-4f, -6f);
        Vector3 posicionAparicion = new Vector3(xAleatorio, 1f, zAleatorio);
        Debug.Log("Instanciando a: " + PhotonNetwork.NickName);
        PhotonNetwork.Instantiate(nombrePrefabJugador, posicionAparicion, Quaternion.identity);
    }
}