using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

public class ControladorJugador : MonoBehaviourPun
{
    [Header("Movimiento y Empujón")]
    public float velocidad = 7f;
    public float fuerzaEmpujon = 15f;
    public float radioAtaque = 2f;
    
    [Header("Mecánica de Agarre")]
    public float radioAgarre = 2f;
    public float fuerzaLanzamiento = 25f;
    
    private Rigidbody rb;
    private Vector3 entradaMovimiento;
    
    // Variables para controlar el objeto agarrado
    private GameObject objetoAgarrado;
    private Rigidbody rbObjeto;
    private Transform puntoAgarre;
    // Guarda el ID del último que nos golpeó
    private int idUltimoAtacante = -1;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Creamos un punto invisible al frente del jugador donde flotará el objeto al cargarlo
        GameObject goPunto = new GameObject("PuntoAgarre");
        puntoAgarre = goPunto.transform;
        puntoAgarre.SetParent(this.transform);
        puntoAgarre.localPosition = new Vector3(0f, 0.5f, 1.2f); // Al frente y un poco arriba

        if (!photonView.IsMine)
        {
            rb.isKinematic = true;
            enabled = false; 
        }
    }

    void Update()
    {
        float moverX = Input.GetAxisRaw("Horizontal");
        float moverZ = Input.GetAxisRaw("Vertical");
        entradaMovimiento = new Vector3(moverX, 0f, moverZ).normalized;

        // ACCIÓN 1: INTENTAR AGARRAR O SOLTAR CON LA 'E'
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (objetoAgarrado == null)
                IntentarAgarrar();
            else
                SoltarObjeto();
        }

        // ACCIÓN 2: CLIC IZQUIERDO (Ataca o Lanza según el estado)
        if (Input.GetMouseButtonDown(0))
        {
            if (objetoAgarrado != null)
                LanzarObjeto();
            else
                IntentarEmpujar();
        }

        // Si llevamos un objeto, lo mantenemos pegado a nuestro punto de agarre
        if (objetoAgarrado != null)
        {
            objetoAgarrado.transform.position = puntoAgarre.position;
            objetoAgarrado.transform.rotation = puntoAgarre.rotation;
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + entradaMovimiento * velocidad * Time.fixedDeltaTime);
        
        if (entradaMovimiento != Vector3.zero)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(entradaMovimiento);
            rb.rotation = Quaternion.Slerp(rb.rotation, rotacionObjetivo, 0.15f);
        }
    }

    void IntentarAgarrar()
    {
        // Buscamos objetos alrededor del jugador
        Collider[] cercanos = Physics.OverlapSphere(transform.position + transform.forward * 0.5f, radioAgarre);
        
        foreach (Collider col in cercanos)
        {
            if (col.CompareTag("Objeto"))
            {
                PhotonView pvObjeto = col.GetComponent<PhotonView>();
                
                if (pvObjeto != null)
                {
                    // ¡REGLA DE RED!: Le pedimos a Photon el control de las físicas de este objeto
                    pvObjeto.RequestOwnership();
                    
                    objetoAgarrado = col.gameObject;
                    rbObjeto = objetoAgarrado.GetComponent<Rigidbody>();
                    
                    if (rbObjeto != null)
                    {
                        rbObjeto.isKinematic = true; // Desactivamos sus físicas mientras lo cargamos
                    }
                    break;
                }
            }
        }
    }

    void LanzarObjeto()
    {
        if (rbObjeto != null)
        {
            rbObjeto.isKinematic = false;
            // Lo empujamos con fuerza hacia adelante basándonos en hacia dónde mira el jugador
            Vector3 direccionLanzamiento = transform.forward + Vector3.up * 0.1f;
            rbObjeto.AddForce(direccionLanzamiento.normalized * fuerzaLanzamiento, ForceMode.Impulse);
        }

        objetoAgarrado = null;
        rbObjeto = null;
    }

    void SoltarObjeto()
    {
        if (rbObjeto != null)
        {
            rbObjeto.isKinematic = false;
        }
        objetoAgarrado = null;
        rbObjeto = null;
    }

    void IntentarEmpujar()
    {
        Vector3 puntoAtaque = transform.position + transform.forward * 1f;
        Collider[] objetosImpactados = Physics.OverlapSphere(puntoAtaque, radioAtaque);

        int miId = photonView.Owner.ActorNumber;

        foreach (Collider col in objetosImpactados)
        {
            // Si el objeto ya no existe en la memoria, saltamos
            if (col == null) continue; 
            if (col.gameObject == this.gameObject) continue;

            ControladorJugador rival = col.GetComponent<ControladorJugador>();
            
            // Verificamos que tenga PhotonView y que no esté destruyéndose
            if (rival != null && rival.photonView != null && !rival.photonView.IsMine)
            {
                Vector3 direccionEmpujon = (rival.transform.position - transform.position).normalized;
                direccionEmpujon.y = 0.2f;
                
                // Enviamos el golpe
                rival.photonView.RPC("RecibirEmpujonRPC", RpcTarget.AllViaServer, direccionEmpujon, fuerzaEmpujon, miId);
            }
        }
    }

    // ¡Atención! Agregamos el parámetro int idAtacante
    [PunRPC]
public void RecibirEmpujonRPC(Vector3 direccion, float fuerza, int idAtacante) 
{
    idUltimoAtacante = idAtacante;
    
    // Fuerzaamos la dirección solo en el plano horizontal (x, z)
    direccion.y = 0; 
    direccion = direccion.normalized;

    if (rb.isKinematic) rb.isKinematic = false;

    // Usamos Velocity en lugar de AddForce para que sea un deslizamiento lineal
    // Esto hace que el personaje "patine" hacia atrás en lugar de saltar
    rb.linearVelocity = direccion * (fuerza * 0.5f); 

    // Si querés que se detenga rápido, llamamos a esto
    Invoke("DetenerMovimiento", 0.3f);
}

void DetenerMovimiento()
{
    rb.linearVelocity = Vector3.zero;
}

    void VolverAEstadosDeRed()
    {
        if (!photonView.IsMine) rb.isKinematic = true;
    }

    [Header("Efecto de Muerte Retro")]
    public float alturaRespawn = 10f;

    // Función pública que llama la Zona de Muerte
   public void IniciarMuerteRetro()
    {
        // 1. REPARTIR EL PUNTO: Si alguien me pegó antes de caer, le doy un punto
        if (idUltimoAtacante != -1)
        {
            // Buscamos al jugador por su ID
            Photon.Realtime.Player atacante = PhotonNetwork.CurrentRoom.GetPlayer(idUltimoAtacante);
            
            if (atacante != null)
            {
                int puntosActuales = 0;
                // Leemos cuántos puntos tiene ahora
                if (atacante.CustomProperties.ContainsKey("Puntos"))
                {
                    puntosActuales = (int)atacante.CustomProperties["Puntos"];
                }

                // Le sumamos 1 usando la tabla especial de Photon (Hashtable)
                ExitGames.Client.Photon.Hashtable propiedades = new ExitGames.Client.Photon.Hashtable();
                propiedades.Add("Puntos", puntosActuales + 1);
                atacante.SetCustomProperties(propiedades);
            }
            
            // Reseteamos la memoria para la próxima vida
            idUltimoAtacante = -1; 
        }

        // 2. Iniciamos el parpadeo
        photonView.RPC("SecuenciaMuerteRPC", RpcTarget.All);
    }
    // 1. EL RPC que recibe el mensaje de todos
    [PunRPC]
    public void SecuenciaMuerteRPC()
    {
        // Iniciamos la corrutina localmente en cada PC
        StartCoroutine(EjecutarSecuenciaMuerte());
    }

    // 2. LA CORRUTINA que maneja los tiempos (no es RPC)
    private IEnumerator EjecutarSecuenciaMuerte()
    {
        MeshRenderer renderer = GetComponentInChildren<MeshRenderer>();

        if (photonView.IsMine)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            this.enabled = false;
        }

        transform.rotation = Quaternion.Euler(90f, transform.rotation.eulerAngles.y, 0f);
        yield return new WaitForSeconds(1f);

        int cantidadParpadeos = 6;
        for (int i = 0; i < cantidadParpadeos; i++)
        {
            if (renderer != null) renderer.enabled = !renderer.enabled;
            yield return new WaitForSeconds(0.15f);
        }

        if (renderer != null) renderer.enabled = false;
        yield return new WaitForSeconds(0.2f);

        if (photonView.IsMine)
        {
            Vector3 nuevaPosicion = new Vector3(Random.Range(-9f, 9f), alturaRespawn, Random.Range(1f, 19f));
            transform.position = nuevaPosicion;
            photonView.RPC("RevivirRPC", RpcTarget.All);
        }
    }
    [PunRPC]
    private void RevivirRPC()
    {
        // Reseteamos la rotación para estar de pie y volvemos a prender el personaje
        transform.rotation = Quaternion.identity;
        MeshRenderer renderer = GetComponentInChildren<MeshRenderer>();
        if (renderer != null) renderer.enabled = true;

        // Si soy el dueño, recupero el control de las físicas y el teclado
        if (photonView.IsMine)
        {
            rb.isKinematic = false;
            this.enabled = true;
        }
        else
        {
            rb.isKinematic = true; // Los rivales se mantienen kinematic en mi pantalla
        }
    }
}