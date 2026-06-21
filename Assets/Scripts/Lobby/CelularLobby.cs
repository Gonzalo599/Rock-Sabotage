using UnityEngine;
using TMPro;
using Photon.Pun;

public class CelularLobby : MonoBehaviour
{
    [Header("UI del Celular (Canvas)")]
    public GameObject panelInterfazCelular; // El panel flotante de la pantalla del celular
    public TMP_InputField campoCodigoAmigo;  // Input para escribir el código

    [Header("UI de Alertas (Opcional)")]
    public TextMeshProUGUI textoAvisoPantalla; // Un texto flotante para poner "Sin Servicio"

    private bool jugadorCerca = false;

    void Start()
    {
        // Aseguramos que la interfaz del celular arranque apagada
        if (panelInterfazCelular != null) panelInterfazCelular.SetActive(false);
        if (textoAvisoPantalla != null) textoAvisoPantalla.text = "";
    }

    void Update()
    {
        // Si el jugador está en el Trigger y presiona la tecla E
        if (jugadorCerca && Input.GetKeyDown(KeyCode.E))
        {
            // CERRADURA: Si el juego está en Modo Offline, el celular no tiene servicio
            if (PhotonNetwork.OfflineMode)
            {
                Debug.LogWarning("El celular no tiene señal en Modo Local.");
                if (textoAvisoPantalla != null)
                {
                    StopAllCoroutines();
                    StartCoroutine(MostrarAvisoTemporal("Sin servicio (Requiere Internet)", 3f));
                }
                return; // Cortamos acá, no se abre el panel
            }

            // Si hay internet, abre o cierra de forma normal
            bool estaActivo = panelInterfazCelular.activeSelf;
            panelInterfazCelular.SetActive(!estaActivo);

            if (!estaActivo) // Se acaba de abrir
            {
                if (campoCodigoAmigo != null) campoCodigoAmigo.Select(); // Hace foco para escribir directo
                Cursor.lockState = CursorLockMode.None; // Libera el mouse
                Cursor.visible = true;
            }
            else // Se acaba de cerrar manualmente
            {
                CerrarMenuCelular();
            }
        }
    }

    // Esta función tenés que asignarla en el botón "CONECTAR" del Canvas de tu celular
    public void PresionarUnirseAmigo()
    {
        if (campoCodigoAmigo == null) return;

        // Tomamos el código, borramos espacios fantasmas y lo pasamos a mayúsculas
        string codigo = campoCodigoAmigo.text.Trim().ToUpper();

        if (!string.IsNullOrWhiteSpace(codigo))
        {
            Debug.Log("Guardando destino y abandonando sala actual para mudar a: " + codigo);
            
            // Guardamos temporalmente el código del amigo en las propiedades de red de nuestro jugador
            ExitGames.Client.Photon.Hashtable propiedadDestino = new ExitGames.Client.Photon.Hashtable();
            propiedadDestino["SalaDestino"] = codigo;
            PhotonNetwork.LocalPlayer.SetCustomProperties(propiedadDestino);

            // Ocultamos el mouse antes del viaje
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Abandonamos nuestra sala actual. Al salir, el GestorLobby va a detectar automáticamente 
            // el código guardado y nos va a conectar con nuestro amigo.
            PhotonNetwork.LeaveRoom(); 
        }
    }

    private void CerrarMenuCelular()
    {
        if (panelInterfazCelular != null) panelInterfazCelular.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked; // Bloquea el mouse para que sigas jugando
        Cursor.visible = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Recordá ponerle el Tag "Player" a tu prefab de jugador
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;
            Debug.Log("Cerca del teléfono. Presioná 'E' para interactuar.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            CerrarMenuCelular();
        }
    }

    // Corrutina para hacer desaparecer el cartel de "Sin Servicio" después de unos segundos
    System.Collections.IEnumerator MostrarAvisoTemporal(string mensaje, float tiempo)
    {
        textoAvisoPantalla.text = mensaje;
        yield return new WaitForSeconds(tiempo);
        textoAvisoPantalla.text = "";
    }
}