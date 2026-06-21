using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro; // Necesario para leer el texto de la UI

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Interfaz")]
    public TMP_InputField inputNombre; // Arrastrá acá el recuadro donde escriben su nombre
    public GameObject botonJugar;      // Arrastrá acá el botón de jugar (opcional)

    void Start()
    {
        Debug.Log("Conectando a los servidores de Photon...");
        
        // Apagamos el botón de jugar hasta que haya internet
        if (botonJugar != null) botonJugar.SetActive(false); 

        PhotonNetwork.ConnectUsingSettings(); 
        PhotonNetwork.AutomaticallySyncScene = true; 
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("¡Conectado al servidor! Esperando al jugador...");
        
        // Prendemos el botón porque ya estamos listos para buscar partida
        if (botonJugar != null) botonJugar.SetActive(true); 
    }

    // A ESTA FUNCIÓN la tenés que enlazar al evento OnClick() de tu botón "Jugar"
    public void UnirsePartidaAleatoria()
    {
        // 1. Asignamos el nombre
        if (!string.IsNullOrEmpty(inputNombre.text))
        {
            PhotonNetwork.NickName = inputNombre.text;
        }
        else
        {
            PhotonNetwork.NickName = "Jugador" + Random.Range(1000, 9999);
        }

        Debug.Log("Buscando sala aleatoria...");
        // 2. Buscamos cualquier sala abierta
        PhotonNetwork.JoinRandomRoom();
    }

    // Si fallamos en unirnos (porque no hay salas o están llenas de 4 personas)
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No hay salas disponibles. Creando una nueva...");
        RoomOptions opcionesSala = new RoomOptions { MaxPlayers = 4 };
        PhotonNetwork.CreateRoom("Sala" + Random.Range(100, 999), opcionesSala);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("¡Entramos a la sala! Vamos a la Sala de Espera...");
        
        // ACÁ CAMBIAMOS LA RUTA: Ahora vamos al Lobby en vez del juego directo
        if (PhotonNetwork.IsMasterClient)
        {
            // Asegurate de que esta escena exista en File > Build Settings
            PhotonNetwork.LoadLevel("MenuLobby"); 
        }
    }
}