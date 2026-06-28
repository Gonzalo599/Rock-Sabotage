using UnityEngine;
using Photon.Pun;

public class ColorPersonaje : MonoBehaviourPun
{
    [Header("Visuales del Chibi")]
    public Renderer renderizadoPersonaje; 

    [Header("Colores por Jugador")]
    public Color colorJugador1 = Color.blue;
    public Color colorJugador2 = Color.red;
    public Color colorJugador3 = Color.yellow;
    public Color colorJugador4 = Color.green;

    void Start()
    {
        if (photonView == null || !photonView.isActiveAndEnabled || !PhotonNetwork.InRoom)
        {
            PintarPersonaje(colorJugador1);
        }
        else
        {
            AsignarColorOnline();
        }
    }

    void AsignarColorOnline()
    {
        int numeroJugador = photonView.Owner.ActorNumber;
        Color colorAsignado = Color.gray; 

        switch (numeroJugador)
        {
            case 1: colorAsignado = colorJugador1; break;
            case 2: colorAsignado = colorJugador2; break;
            case 3: colorAsignado = colorJugador3; break;
            case 4: colorAsignado = colorJugador4; break;
        }

        PintarPersonaje(colorAsignado);
    }

    void PintarPersonaje(Color colorElegido)
    {
        if (renderizadoPersonaje != null)
        {
            renderizadoPersonaje.material.color = colorElegido;
        }
    }
}