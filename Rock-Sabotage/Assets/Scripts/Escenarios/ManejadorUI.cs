using UnityEngine;
using TMPro; // Asegurate de usar TextMeshPro para los textos
using Photon.Pun;
using Photon.Realtime;

public class ManejadorUI : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI textoJugadoresConectados;
    public TextMeshProUGUI textoPuntajes;

    void Update()
    {
        // Actualizamos la cantidad de jugadores adentro de la sala (Ej: "Jugadores: 2/4")
        if (PhotonNetwork.InRoom)
        {
            textoJugadoresConectados.text = $"Jugadores: {PhotonNetwork.CurrentRoom.PlayerCount} / {PhotonNetwork.CurrentRoom.MaxPlayers}";
            
            ActualizarTablaPuntos();
        }
    }

    void ActualizarTablaPuntos()
    {
        string tabla = "PUNTAJES:\n";

        // Recorremos la lista de jugadores actuales en la partida
        foreach (Player jugador in PhotonNetwork.PlayerList)
        {
            // Buscamos si el jugador ya tiene puntos guardados, si no, arranca en 0
            int puntos = 0;
            if (jugador.CustomProperties.ContainsKey("Puntos"))
            {
                puntos = (int)jugador.CustomProperties["Puntos"];
            }

            tabla += $"{jugador.NickName}: {puntos} pts\n";
        }

        textoPuntajes.text = tabla;
    }
}