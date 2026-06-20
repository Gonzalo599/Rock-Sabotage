using UnityEngine;
using Photon.Pun;
using System.Collections;

public class SpawnerObjetos : MonoBehaviourPunCallbacks
{
    public string nombrePrefab = "Objeto_Prototipo";
    public float tiempoSpawn = 5f;
    public float radioSpawn = 9f;
    
    // Nueva variable para poner el tope de objetos en juego
    public int maxObjetos = 5; 

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(RutinaSpawn());
        }
    }

    IEnumerator RutinaSpawn()
    {
        while (true)
        {
            yield return new WaitForSeconds(tiempoSpawn);

            // Buscamos en el mapa todos los objetos que tengan la etiqueta "ObjetoTirable"
            GameObject[] objetosEnEscena = GameObject.FindGameObjectsWithTag("Objeto");

            // CONTROL: Si la cantidad actual es menor al máximo, recién ahí tiramos una nueva
            if (objetosEnEscena.Length < maxObjetos)
            {
                Vector3 posicionSpawn = new Vector3(Random.Range(-radioSpawn, radioSpawn), 13f, Random.Range(1f, 19f));
                PhotonNetwork.Instantiate(nombrePrefab, posicionSpawn, Quaternion.identity);
                
                Debug.Log($"Caja spawneada. Total en escena: {objetosEnEscena.Length + 1}/{maxObjetos}");
            }
            else
            {
                Debug.Log("Límite de cajas alcanzado. Esperando a que baje la cantidad...");
            }
        }
    }
}