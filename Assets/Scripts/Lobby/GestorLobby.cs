using UnityEngine;
using TMPro; 
using Photon.Pun; 
using Photon.Realtime;

public class GestorLobby : MonoBehaviourPunCallbacks 
{
    [Header("Configuración de Aparición")]
    public string nombrePrefabJugador = "Jugador";

    [Header("UI del Lobby (Estado de Red)")]
    public TextMeshProUGUI textoEstadoConexion; // Ejemplo: "Conectando...", "Conectado", "No estas conectado"
    public TextMeshProUGUI textoCodigoSala;     // Muestra el código "SALA-XXXX" o aviso de modo local

    private bool yaAparecioJugador = false;

    void Start()
    {
        yaAparecioJugador = false;
        PhotonNetwork.NickName = PlayerPrefs.GetString("NombreJugador", "Músico Anónimo");
        if (textoEstadoConexion != null) textoEstadoConexion.text = "Conectando al servidor...";
        if (textoCodigoSala != null) textoCodigoSala.text = "";
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Iniciando conexión de fondo a los servidores...");
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            OnConnectedToMaster();
        }
    }

    public override void OnConnectedToMaster()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("SalaDestino", out object codigoObtenido) && codigoObtenido != null)
        {
            string salaDestino = (string)codigoObtenido;
            ExitGames.Client.Photon.Hashtable limpiar = new ExitGames.Client.Photon.Hashtable() { { "SalaDestino", null } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(limpiar);

            if (textoEstadoConexion != null) textoEstadoConexion.text = "Cambiando de sala...";
            PhotonNetwork.JoinRoom(salaDestino);
        }
        else
        {
            if (textoEstadoConexion != null) textoEstadoConexion.text = "Creando entorno online...";
            
            string miCodigo = "SALA-" + Random.Range(1000, 9999).ToString();
            RoomOptions opciones = new RoomOptions { MaxPlayers = 4, IsVisible = false, IsOpen = true };
            PhotonNetwork.CreateRoom(miCodigo, opciones);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("No se detectó internet. Activando modo Offline de emergencia. Motivo: " + cause);
        
        if (textoEstadoConexion != null) textoEstadoConexion.text = "No estas conectado (Modo Local)";
        
        PhotonNetwork.OfflineMode = true;
        PhotonNetwork.CreateRoom("SalaLocal");
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.OfflineMode)
        {
            if (textoEstadoConexion != null) textoEstadoConexion.text = "No estas conectado (Modo Local)";
            if (textoCodigoSala != null) textoCodigoSala.text = "Jugando en solitario";
        }
        else
        {
            if (textoEstadoConexion != null) textoEstadoConexion.text = "Conectado";
            if (textoCodigoSala != null) textoCodigoSala.text = "Código: " + PhotonNetwork.CurrentRoom.Name;
        }
        if (!yaAparecioJugador)
        {
            AparecerJugador();
            yaAparecioJugador = true;
        }
    }

    void AparecerJugador()
    {
        float xAleatorio = Random.Range(-4f, 6f);
        float zAleatorio = Random.Range(-4f, -6f);
        Vector3 posicionAparicion = new Vector3(xAleatorio, 1f, zAleatorio);
        
        Debug.Log("Apareciendo personaje: " + PhotonNetwork.NickName);
        PhotonNetwork.Instantiate(nombrePrefabJugador, posicionAparicion, Quaternion.identity);
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Error al unirse a la sala del amigo: " + message);
        string miCodigo = "SALA-" + Random.Range(1000, 9999).ToString();
        RoomOptions opciones = new RoomOptions { MaxPlayers = 4, IsVisible = false, IsOpen = true };
        PhotonNetwork.CreateRoom(miCodigo, opciones);
    }
}