using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // Necesario para cambiar de escena sin Photon

public class ControladorMenuRed : MonoBehaviour
{
    [Header("Pantallas")]
    public CanvasGroup grupoPanelInicio; 
    public CanvasGroup grupoPanelNombre;  
    public TMP_InputField campoNombre;
    public GameObject botonJugar; 
    public TextMeshProUGUI textoPresionaBoton;

    [Header("Configuraciones")]
    public float velocidadTransicion = 2f; 
    public float velocidadPulso = 3f;

    private bool esperandoBoton = true;

    void Start()
    {
        grupoPanelInicio.alpha = 1f;
        grupoPanelNombre.alpha = 0f;
        grupoPanelNombre.interactable = false;
        grupoPanelNombre.blocksRaycasts = false;
        
        // Se eliminó el SetActive del botón para que el CanvasGroup maneje su visibilidad suavemente
    }

    void Update()
    {
        if (esperandoBoton)
        {
            float alphaPulso = (Mathf.Sin(Time.time * velocidadPulso) + 1f) / 2f;
            textoPresionaBoton.color = new Color(textoPresionaBoton.color.r, textoPresionaBoton.color.g, textoPresionaBoton.color.b, alphaPulso * 0.7f + 0.3f);

            if (Input.anyKeyDown)
            {
                esperandoBoton = false; 
                StartCoroutine(TransicionSuave());
            }
        }
    }

    IEnumerator TransicionSuave()
    {
        while (grupoPanelInicio.alpha > 0)
        {
            grupoPanelInicio.alpha -= Time.deltaTime * velocidadTransicion;
            yield return null;
        }

        grupoPanelInicio.blocksRaycasts = false;
        grupoPanelNombre.gameObject.SetActive(true); 

        while (grupoPanelNombre.alpha < 1)
        {
            grupoPanelNombre.alpha += Time.deltaTime * velocidadTransicion;
            yield return null;
        }

        grupoPanelNombre.interactable = true;
        grupoPanelNombre.blocksRaycasts = true;
        campoNombre.Select(); 
        
        // Se eliminó el SetActive de acá para evitar que el botón aparezca de golpe y rompa el fade
    }

    public void ConfirmarNombreYEntrar()
    {
        string nombreIngresado = campoNombre.text;
        if (string.IsNullOrWhiteSpace(nombreIngresado)) return;

        PlayerPrefs.SetString("NombreJugador", nombreIngresado);
        PlayerPrefs.Save();
        SceneManager.LoadScene("MenuLobby"); 
    }

    public void ProbarBoton()
    {
        Debug.Log("¡El script funciona!");
    }
}