using UnityEngine;
using Photon.Pun; // Necesario para la red

public class GestorLobby : MonoBehaviour
{
    [Header("Configuración de Aparición")]
    public string nombrePrefabJugador = "Jugador"; // DEBE llamarse EXACTO igual que tu archivo en Resources

    void Start()
    {
        // Verificamos si estamos conectados a Photon por las dudas
        if (PhotonNetwork.IsConnected)
        {
            AparecerJugador();
        }
    }

    void AparecerJugador()
    {
        // Generamos una posición aleatoria para que no spawneen todos uno adentro del otro
        // Cambiá estos números dependiendo del tamaño de tu piso
        float xAleatorio = Random.Range(-5f, 5f);
        float zAleatorio = Random.Range(-5f, 5f);
        Vector3 posicionAparicion = new Vector3(xAleatorio, 1f, zAleatorio);

        Debug.Log("Instanciando a: " + PhotonNetwork.NickName);

        // La magia de Photon: Crea el objeto en la red
        PhotonNetwork.Instantiate(nombrePrefabJugador, posicionAparicion, Quaternion.identity);
    }
}