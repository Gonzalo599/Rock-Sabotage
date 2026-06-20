using UnityEngine;
using Photon.Pun;

public class ImpactoObjeto : MonoBehaviourPun
{
    // Qué tan fuerte será el empujón basado en la velocidad de la caja
    public float multiplicadorFuerza = 1.5f;

    private void OnCollisionEnter(Collision collision)
    {
        // Verificamos que realmente tengamos un PhotonView
    if (photonView == null) return;

    // Solo pedimos propiedad si no somos dueños y si el jugador realmente existe
    if (!photonView.IsMine && PhotonNetwork.LocalPlayer != null)
    {
        photonView.RequestOwnership();
    }

    ControladorJugador rival = collision.gameObject.GetComponent<ControladorJugador>();
        
        if (rival != null && photonView.IsMine) // Solo ejecuto el golpe si soy el dueño real
        {
            float velocidadImpacto = collision.relativeVelocity.magnitude;
            if (velocidadImpacto > 0.5f) 
            {
                int idLanzador = photonView.Owner.ActorNumber;
                Vector3 direccionGolpe = (rival.transform.position - transform.position).normalized;
                direccionGolpe.y = 0.3f;
                
                float fuerzaFinal = velocidadImpacto * 2f; // Ajustá tu multiplicador aquí

                rival.photonView.RPC("RecibirEmpujonRPC", RpcTarget.AllViaServer, direccionGolpe, fuerzaFinal, idLanzador);
            }
        }
    }
}