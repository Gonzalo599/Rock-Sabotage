using System.Collections;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class ControladorMenuRed : MonoBehaviourPunCallbacks
{
    [Header("Pantalla de Inicio (Título y Botón)")]
    public CanvasGroup grupoPanelInicio; 
    public TextMeshProUGUI textoPresionaBoton; // Para el efecto de parpadeo suave

    [Header("Panel Ingreso de Nombre")]
    public CanvasGroup grupoPanelNombre;  
    public TMP_InputField campoNombre;
    public GameObject botonJugar; 

    [Header("Configuraciones Visuales")]
    public float velocidadTransicion = 2f; 
    public float velocidadPulso = 3f; // Qué tan rápido respira el texto

    private bool esperandoBoton = true;

    void Start()
    {
        // 1. Estado inicial de la pantalla
        grupoPanelInicio.alpha = 1f;
        grupoPanelInicio.interactable = true;
        grupoPanelInicio.blocksRaycasts = true;

        grupoPanelNombre.alpha = 0f;
        grupoPanelNombre.interactable = false;
        grupoPanelNombre.blocksRaycasts = false;
        
        if (botonJugar != null) botonJugar.SetActive(false);

        // 2. Conexión silenciosa a Photon
        Debug.Log("Conectando a los servidores de Photon...");
        PhotonNetwork.ConnectUsingSettings(); 
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Update()
    {
        if (esperandoBoton)
        {
            // EFECTO DE JUGO: Animación de "Respiración" usando la onda Senoidal (Mathf.Sin)
            // Esto hace que la transparencia del texto suba y baje suavemente entre 0.3 y 1
            float alphaPulso = (Mathf.Sin(Time.time * velocidadPulso) + 1f) / 2f;
            textoPresionaBoton.color = new Color(textoPresionaBoton.color.r, textoPresionaBoton.color.g, textoPresionaBoton.color.b, alphaPulso * 0.7f + 0.3f);

            // Input.anyKeyDown detecta CUALQUIER tecla del teclado o clic del mouse
            if (Input.anyKeyDown)
            {
                esperandoBoton = false; 
                StartCoroutine(TransicionSuave());
            }
        }
    }

    IEnumerator TransicionSuave()
    {
        // 1. Desvanece el título y el texto de inicio
        while (grupoPanelInicio.alpha > 0)
        {
            grupoPanelInicio.alpha -= Time.deltaTime * velocidadTransicion;
            yield return null;
        }

        grupoPanelInicio.interactable = false;
        grupoPanelInicio.blocksRaycasts = false;

        // ---- LA SOLUCIÓN: PRENDEMOS EL OBJETO ----
        grupoPanelNombre.gameObject.SetActive(true); 
        // ------------------------------------------

        // 2. Aparece el panel del nombre
        while (grupoPanelNombre.alpha < 1)
        {
            grupoPanelNombre.alpha += Time.deltaTime * velocidadTransicion;
            yield return null;
        }

        grupoPanelNombre.interactable = true;
        grupoPanelNombre.blocksRaycasts = true;
        campoNombre.Select(); 
    }

    // --- LÓGICA DE PHOTON ---

    public override void OnConnectedToMaster()
    {
        Debug.Log("¡Conectado al servidor de Photon!");
        if (botonJugar != null) botonJugar.SetActive(true); 
    }

    public void ConfirmarNombreYJugar()
    {
        string nombreIngresado = campoNombre.text;

        if (string.IsNullOrWhiteSpace(nombreIngresado))
        {
            Debug.LogWarning("¡El nombre no puede estar vacío!");
            return; 
        }

        PlayerPrefs.SetString("NombreJugador", nombreIngresado);
        PlayerPrefs.Save();
        PhotonNetwork.NickName = nombreIngresado;

        if (botonJugar != null) botonJugar.SetActive(false); 
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        RoomOptions opcionesSala = new RoomOptions { MaxPlayers = 4 };
        PhotonNetwork.CreateRoom("Sala" + Random.Range(100, 999), opcionesSala);
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("MenuLobby"); 
        }
    }
}