using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class GestorLobby : MonoBehaviourPunCallbacks
{
    [Header("Configuración de Aparición")]
    public string nombrePrefabJugador = "Personaje"; 
    public GameObject prefabJugadorLocal; 

    [Header("UI del Lobby (Estado de Red)")]
    public TextMeshProUGUI textoEstadoConexion;
    public TextMeshProUGUI textoCodigoSala;

    private GameObject miPersonajeActual; 
    private bool yaAparecioJugadorEnEstaSala = false;
    private string salaPendiente = ""; 

    void Start()
    {
        // Limpiamos la memoria fantasma de los instrumentos al arrancar
        SelectorInstrumento.instrumentoLocalEquipado = "";

        AparecerLocalmente();

        PhotonNetwork.NickName = PlayerPrefs.GetString("NombreJugador", "Músico Anónimo");
        if (textoEstadoConexion != null) textoEstadoConexion.text = "Conectando al servidor...";
        if (textoCodigoSala != null) textoCodigoSala.text = "";

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            OnConnectedToMaster(); 
        }
    }

    void AparecerLocalmente()
    {
        if (miPersonajeActual != null) Destroy(miPersonajeActual);

        Vector3 posicionAparicion = new Vector3(Random.Range(-4f, 6f), 1f, Random.Range(-4f, -6f));
        miPersonajeActual = Instantiate(prefabJugadorLocal, posicionAparicion, Quaternion.identity);

        PhotonView pv = miPersonajeActual.GetComponent<PhotonView>();
        if (pv != null) pv.enabled = false;
    }

    public void UnirseASalaEspecifica(string codigoIngresado)
    {
        string codigoLimpio = codigoIngresado.Trim().ToUpper();
        if (string.IsNullOrEmpty(codigoLimpio)) return;
        string codigoFinal = codigoLimpio;
        if (!codigoFinal.StartsWith("SALA-")) 
        {
            codigoFinal = "SALA-" + codigoFinal;
        }

        if (codigoFinal == "SALA-")
        {
            Debug.LogWarning("Intento de conexión cancelado: Faltan los números de la sala.");
            if (textoEstadoConexion != null) textoEstadoConexion.text = "Ingresá un número válido";
            Invoke("ResetearEstadoUI", 2f);
            return; 
        }
        if (PhotonNetwork.InRoom)
        {
            salaPendiente = codigoFinal;
            if (textoEstadoConexion != null) textoEstadoConexion.text = "Saliendo de la sala actual...";
            PhotonNetwork.LeaveRoom(); 
        }
        else
        {
            PhotonNetwork.JoinRoom(codigoFinal);
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
        else if (!string.IsNullOrEmpty(salaPendiente))
        {
            if (textoEstadoConexion != null) textoEstadoConexion.text = "Entrando a " + salaPendiente + "...";
            PhotonNetwork.JoinRoom(salaPendiente);
            salaPendiente = "";
        }
        else 
        {
            if (textoEstadoConexion != null) textoEstadoConexion.text = "Creando entorno online...";
            string miCodigo = "SALA-" + Random.Range(1000, 9999).ToString();
            RoomOptions opciones = new RoomOptions { MaxPlayers = 4, IsVisible = false, IsOpen = true };
            PhotonNetwork.CreateRoom(miCodigo, opciones);
        }
    }

    public override void OnLeftRoom()
    {
        yaAparecioJugadorEnEstaSala = false;
        AparecerLocalmente(); 
    }

    public override void OnJoinedRoom()
    {
        if (textoEstadoConexion != null) textoEstadoConexion.text = "Conectado";
        if (textoCodigoSala != null) textoCodigoSala.text = "Código: " + PhotonNetwork.CurrentRoom.Name;

        if (!yaAparecioJugadorEnEstaSala)
        {
            AparecerEnRed();
            yaAparecioJugadorEnEstaSala = true;
        }
    }

    void AparecerEnRed()
    {
        Vector3 posicionActual = miPersonajeActual != null ? miPersonajeActual.transform.position : new Vector3(0, 1, 0);
        Quaternion rotacionActual = miPersonajeActual != null ? miPersonajeActual.transform.rotation : Quaternion.identity;

        if (miPersonajeActual != null)
        {
            Destroy(miPersonajeActual);
        }

        miPersonajeActual = PhotonNetwork.Instantiate(nombrePrefabJugador, posicionActual, rotacionActual);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Error: La sala no existe.");
        if (textoEstadoConexion != null) textoEstadoConexion.text = "Error: Sala no encontrada";
        string miCodigo = "SALA-" + Random.Range(1000, 9999).ToString();
        RoomOptions opciones = new RoomOptions { MaxPlayers = 4, IsVisible = false, IsOpen = true };
        PhotonNetwork.CreateRoom(miCodigo, opciones);

        Invoke("ResetearEstadoUI", 3f); 
    }

    void ResetearEstadoUI()
    {
        if (PhotonNetwork.InRoom && textoEstadoConexion != null) 
            textoEstadoConexion.text = "Conectado";
    }
}