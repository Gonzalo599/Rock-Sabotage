using UnityEngine;
using Photon.Pun;
using Photon.Realtime; 

public class SelectorInstrumento : MonoBehaviourPunCallbacks 
{
    public static event System.Action AlCambiarInstrumentoLocal; 
    public static string instrumentoLocalEquipado = ""; 

    [Header("Configuración del Instrumento")]
    public string nombreClase; 
    
    [Header("Referencias del Objeto")]
    public GameObject modeloVisual; 
    public Collider zonaInteraccion; 

    private bool jugadorCerca = false;

    void Start()
    {
        VerificarDisponibilidad();
        AlCambiarInstrumentoLocal += VerificarDisponibilidad; 
    }

    void OnDestroy()
    {
        AlCambiarInstrumentoLocal -= VerificarDisponibilidad; 
    }

    void Update()
    {
        // Si el panel del celular está abierto, no podemos agarrar cosas
        if (CelularLobby.interfazAbierta) return;

        if (jugadorCerca && Input.GetKeyDown(KeyCode.E))
        {
            EquiparInstrumento();
        }
    }

    void EquiparInstrumento()
    {
        instrumentoLocalEquipado = nombreClase;

        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
        {
            ExitGames.Client.Photon.Hashtable propiedadInstrumento = new ExitGames.Client.Photon.Hashtable();
            propiedadInstrumento["ClaseBanda"] = nombreClase;
            PhotonNetwork.LocalPlayer.SetCustomProperties(propiedadInstrumento);
        }

        AlCambiarInstrumentoLocal?.Invoke();
        Debug.Log("¡Te has equipado: " + nombreClase + "!");
    }
    
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("ClaseBanda")) VerificarDisponibilidad();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        VerificarDisponibilidad();
    }

    void VerificarDisponibilidad()
    {
        bool estaOcupado = false;

        if (instrumentoLocalEquipado == nombreClase)
        {
            estaOcupado = true;
        }
        else if (PhotonNetwork.InRoom)
        {
            foreach (Player jugador in PhotonNetwork.PlayerList)
            {
                if (jugador.IsLocal) continue; 

                if (jugador.CustomProperties.TryGetValue("ClaseBanda", out object claseGuardada))
                {
                    if ((string)claseGuardada == nombreClase)
                    {
                        estaOcupado = true;
                        break; 
                    }
                }
            }
        }

        if (modeloVisual != null) 
        {
            Renderer[] renderizadores = modeloVisual.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer render in renderizadores)
            {
                render.enabled = !estaOcupado;
            }
        }
        
        if (zonaInteraccion != null) zonaInteraccion.enabled = !estaOcupado;
        if (estaOcupado) jugadorCerca = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView pv = other.GetComponent<PhotonView>();
            // Permite agarrarlo si es nuestro clon online, o si estamos sin internet (pv apagado)
            if (pv == null || !pv.enabled || pv.ViewID == 0 || pv.IsMine)
            {
                jugadorCerca = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView pv = other.GetComponent<PhotonView>();
            if (pv == null || !pv.enabled || pv.ViewID == 0 || pv.IsMine)
            {
                jugadorCerca = false;
            }
        }
    }
}