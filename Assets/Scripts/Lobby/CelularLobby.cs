using UnityEngine;
using TMPro;
using Photon.Pun;

public class CelularLobby : MonoBehaviour
{
    public static bool interfazAbierta = false; 

    [Header("UI del Celular (Canvas)")]
    public GameObject panelInterfazCelular; 
    public TMP_InputField campoCodigoAmigo; 

    [Header("UI de Alertas (Opcional)")]
    public TextMeshProUGUI textoAvisoPantalla; 

    private bool jugadorCerca = false;

    void Start()
    {
        if (panelInterfazCelular != null) panelInterfazCelular.SetActive(false);
        if (textoAvisoPantalla != null) textoAvisoPantalla.text = "";
        interfazAbierta = false;
    }

    void Update()
    {
        if (jugadorCerca && Input.GetKeyDown(KeyCode.E))
        {
            if (PhotonNetwork.OfflineMode)
            {
                Debug.LogWarning("El celular no tiene señal en Modo Local.");
                if (textoAvisoPantalla != null)
                {
                    StopAllCoroutines();
                    StartCoroutine(MostrarAvisoTemporal("Sin servicio (Requiere Internet)", 3f));
                }
                return; 
            }

            bool estaActivo = panelInterfazCelular.activeSelf;
            panelInterfazCelular.SetActive(!estaActivo);

            if (!estaActivo) 
            {
                interfazAbierta = true; 
                if (campoCodigoAmigo != null) campoCodigoAmigo.Select(); 
                Cursor.lockState = CursorLockMode.None; 
                Cursor.visible = true;
            }
            else 
            {
                CerrarMenuCelular();
            }
        }
    }

    public void PresionarUnirseAmigo()
    {
        if (campoCodigoAmigo == null) return;

        string codigo = campoCodigoAmigo.text.Trim().ToUpper();

        if (!string.IsNullOrWhiteSpace(codigo))
        {
            Debug.Log("Guardando destino y abandonando sala actual para mudar a: " + codigo);
            
            ExitGames.Client.Photon.Hashtable propiedadDestino = new ExitGames.Client.Photon.Hashtable();
            propiedadDestino["SalaDestino"] = codigo;
            PhotonNetwork.LocalPlayer.SetCustomProperties(propiedadDestino);
            CerrarMenuCelular(); 

            PhotonNetwork.LeaveRoom(); 
        }
    }

    private void CerrarMenuCelular()
    {
        interfazAbierta = false; 
        if (panelInterfazCelular != null) panelInterfazCelular.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;
    }

    private void OnTriggerEnter(Collider other)
    {
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
    System.Collections.IEnumerator MostrarAvisoTemporal(string mensaje, float tiempo)
    {
        textoAvisoPantalla.text = mensaje;
        yield return new WaitForSeconds(tiempo);
        textoAvisoPantalla.text = "";
    }
}