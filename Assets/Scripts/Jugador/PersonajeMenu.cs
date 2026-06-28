using UnityEngine;
using Photon.Pun;

public class PersonajeMenu : MonoBehaviourPunCallbacks 
{
    public float velocidad = 5f;
    public float velocidadGiro = 15f; 
    public float gravedad = 20f; 

    [Header("Instrumentos Visuales en el Cuerpo")]
    public GameObject visualGuitarra;
    public GameObject visualBajo;
    public GameObject visualMicrofono;
    
    [Header("Palillos del Baterista")]
    public GameObject visualBateriaDerecha; // <--- Palillo derecho
    public GameObject visualBateriaIzquierda; // <--- Palillo izquierdo

    [Header("Configuración de Combate (Lobby)")]
    public float radioAtaque = 2f; 
    public float fuerzaEmpuje = 15f; 
    public Transform puntoAtaque;
    
    public float tiempoPausaAtaque = 1f; 
    private bool estaAtacando = false;

    private CharacterController controller;
    private Animator anim;
    private Vector3 velocidadVertical;
    private string instrumentoActual = "";
    private Vector3 impactoFuerza = Vector3.zero; 

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
        SelectorInstrumento.AlCambiarInstrumentoLocal += ActualizarInstrumentoVisual;

        ActualizarInstrumentoVisual();
    }

    void OnDestroy()
    {
        SelectorInstrumento.AlCambiarInstrumentoLocal -= ActualizarInstrumentoVisual;
    }

    void Update()
    {
        if (photonView != null && photonView.isActiveAndEnabled && !photonView.IsMine) 
        {
            return;
        }

        if (CelularLobby.interfazAbierta)
        {
            if (anim != null) anim.SetBool("Correr", false);
            return; 
        }

        // --- PROCESAR EMPUJE RECIBIDO (Físicas del golpe) ---
        if (impactoFuerza.magnitude > 0.2f)
        {
            controller.Move(impactoFuerza * Time.deltaTime);
            impactoFuerza = Vector3.Lerp(impactoFuerza, Vector3.zero, 5f * Time.deltaTime);
        }

        if (controller.isGrounded && velocidadVertical.y < 0)
        {
            velocidadVertical.y = -2f; 
        }
        if (!estaAtacando)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 direccion = new Vector3(horizontal, 0f, vertical).normalized;
            bool seMueve = direccion.magnitude >= 0.1f;
            
            if (anim != null) anim.SetBool("Correr", seMueve);

            if (seMueve)
            {
                float anguloObjetivo = Mathf.Atan2(direccion.x, direccion.z) * Mathf.Rad2Deg;
                float anguloSuave = Mathf.LerpAngle(transform.eulerAngles.y, anguloObjetivo, velocidadGiro * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0f, anguloSuave, 0f);

                controller.Move(direccion * velocidad * Time.deltaTime);
            }
        }
        else 
        {
            // Si estamos atacando, nos aseguramos de apagar la animación de correr
            if (anim != null) anim.SetBool("Correr", false);
        }

        // La gravedad sigue funcionando siempre, para que no te quedes flotando si atacás en el aire
        velocidadVertical.y -= gravedad * Time.deltaTime;
        controller.Move(velocidadVertical * Time.deltaTime);
        
        // --- ENTRADA DE ATAQUE ---
        // Evitamos que el jugador haga "spam" de clics mientras ya está pegando
        if (Input.GetMouseButtonDown(0) && !estaAtacando && !string.IsNullOrEmpty(instrumentoActual))
        {
            StartCoroutine(RutinaAtaqueEnLobby());
        }
    }

    System.Collections.IEnumerator RutinaAtaqueEnLobby()
    {
        estaAtacando = true;
        if (anim != null) anim.SetTrigger("Atacar");
        Vector3 posicionAtaque = puntoAtaque != null ? puntoAtaque.position : transform.position + transform.forward;
        Collider[] enemigosGolpeados = Physics.OverlapSphere(posicionAtaque, radioAtaque);

        foreach (Collider hit in enemigosGolpeados)
        {
            if (hit.CompareTag("Player") && hit.gameObject != this.gameObject)
            {
                PhotonView pvEnemigo = hit.GetComponent<PhotonView>();
                if (pvEnemigo != null && pvEnemigo.enabled)
                {
                    Vector3 direccionEmpuje = (hit.transform.position - transform.position).normalized;
                    direccionEmpuje.y = 0.1f; 
                    pvEnemigo.RPC("RPC_RecibirGolpe", pvEnemigo.Owner, direccionEmpuje * fuerzaEmpuje);
                }
            }
        }

        // 4. ¡LA PAUSA! Esperamos el tiempo indicado antes de devolver el control
        yield return new WaitForSeconds(tiempoPausaAtaque);

        // 5. Liberamos el movimiento
        estaAtacando = false;
    }

    [PunRPC]
    void RPC_RecibirGolpe(Vector3 fuerza)
    {
        impactoFuerza = fuerza;
        if (anim != null) anim.SetTrigger("Golpeado"); 
        Debug.Log("¡Ay! Me pegaron.");
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (photonView != null && photonView.Owner != null && targetPlayer == photonView.Owner)
        {
            if (changedProps.ContainsKey("ClaseBanda"))
            {
                ActualizarInstrumentoVisual();
            }
        }
    }

    void ActualizarInstrumentoVisual()
    {
        string claseElegida = "";
        
        if (photonView == null || !photonView.enabled || photonView.IsMine || !PhotonNetwork.IsConnected)
        {
            claseElegida = SelectorInstrumento.instrumentoLocalEquipado;
        }
        else if (PhotonNetwork.InRoom && photonView.Owner != null)
        {
            if (photonView.Owner.CustomProperties.TryGetValue("ClaseBanda", out object claseGuardada))
            {
                claseElegida = (string)claseGuardada;
            }
        }

        instrumentoActual = claseElegida;
        
        if (visualGuitarra != null) visualGuitarra.SetActive(false);
        if (visualBajo != null) visualBajo.SetActive(false);
        if (visualMicrofono != null) visualMicrofono.SetActive(false);
        if (visualBateriaDerecha != null) visualBateriaDerecha.SetActive(false);
        if (visualBateriaIzquierda != null) visualBateriaIzquierda.SetActive(false);
        
        // ¡LA MAGIA NUEVA ESTÁ ACÁ ADENTRO!
        switch (instrumentoActual)
        {
            case "Guitarrista": 
                if (visualGuitarra != null) visualGuitarra.SetActive(true); 
                if (anim != null) anim.SetInteger("IDInstrumento", 1); // Avisamos al Animator
                break;
            case "Bajista": 
                if (visualBajo != null) visualBajo.SetActive(true); 
                if (anim != null) anim.SetInteger("IDInstrumento", 2);
                break;
            case "Baterista": 
                if (visualBateriaDerecha != null) visualBateriaDerecha.SetActive(true); 
                if (visualBateriaIzquierda != null) visualBateriaIzquierda.SetActive(true);
                if (anim != null) anim.SetInteger("IDInstrumento", 3);
                break;
            case "Cantante": 
                if (visualMicrofono != null) visualMicrofono.SetActive(true); 
                if (anim != null) anim.SetInteger("IDInstrumento", 4);
                break;
            default:
                if (anim != null) anim.SetInteger("IDInstrumento", 0); // Desarmado
                break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 posicionAtaque = puntoAtaque != null ? puntoAtaque.position : transform.position + transform.forward;
        Gizmos.DrawWireSphere(posicionAtaque, radioAtaque);
    }
}