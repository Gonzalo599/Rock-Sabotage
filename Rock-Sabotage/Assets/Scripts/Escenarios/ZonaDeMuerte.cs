using UnityEngine;
using Photon.Pun;
using System.Collections;


public class ZonaDeMuerte : MonoBehaviourPunCallbacks
{
    public float alturaRespawn = 10f; 
    public float tiempoEspera = 2f;
    private void OnTriggerEnter(Collider other)
    {
        // 1. Verificamos que el objeto sea un jugador (o lo que sea que quieras matar)
        ControladorJugador jugador = other.GetComponent<ControladorJugador>();

        // ¡La parte clave! Si 'jugador' es nulo, el resto no se ejecuta
        if (jugador != null)
        {
            // Llamamos a la muerte retro
            jugador.IniciarMuerteRetro();
        }
        
        // 2. Si es una caja, la destruimos con cuidado
        // SIEMPRE que quieras borrar algo, hacelo mediante el Master
        if (other.CompareTag("Objeto")) 
        {
            PhotonView pvCaja = other.GetComponent<PhotonView>();
            if (pvCaja != null)
            {
                // En lugar de intentar borrarlo vos, le avisas al Master
                // El Master NO tiene errores al borrar, solo los clientes.
                photonView.RPC("RPC_SolicitarDestruccion", RpcTarget.MasterClient, pvCaja.ViewID);
            }
        }
    }


    private IEnumerator RespawnearJugador(GameObject jugadorGO)
    {
        Rigidbody rb = jugadorGO.GetComponent<Rigidbody>();
        // Buscamos el componente que dibuja al jugador en pantalla
        MeshRenderer renderer = jugadorGO.GetComponentInChildren<MeshRenderer>();

        // 1. Frenamos el personaje y lo hacemos invisible
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true; 
        }
        
        if (renderer != null)
        {
            renderer.enabled = false; // ¡Desaparece!
        }

        // 2. Esperamos el tiempo de castigo
        yield return new WaitForSeconds(tiempoEspera);

        // 3. Lo movemos arriba
        Vector3 nuevaPosicion = new Vector3(Random.Range(-9f, 9f), alturaRespawn, Random.Range(1f, 19f));
        jugadorGO.transform.position = nuevaPosicion;

        // 4. Le devolvemos las físicas y lo hacemos visible de nuevo
        if (rb != null)
        {
            rb.isKinematic = false;
        }
        
        if (renderer != null)
        {
            renderer.enabled = true; // ¡Reaparece!
        }

        Debug.Log("¡Jugador respawneado con éxito!");
    }
}