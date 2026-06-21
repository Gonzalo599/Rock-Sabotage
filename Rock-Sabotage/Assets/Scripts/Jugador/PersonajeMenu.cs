using UnityEngine;
using Photon.Pun;

public class PersonajeMenu : MonoBehaviourPun
{
    public float velocidad = 5f;
    public float velocidadGiro = 10f;
    
    private CharacterController controller;
    private Transform camaraTransform;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        // Si este personaje NO es el nuestro, lo ignoramos
        if (!photonView.IsMine) return;

        camaraTransform = Camera.main.transform;
    }

    void Update()
    {
        // Solo movemos si es nuestro personaje
        if (!photonView.IsMine) return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 direccion = new Vector3(horizontal, 0, vertical).normalized;

        if (direccion.magnitude >= 0.1f)
        {
            // Calculamos el ángulo hacia donde mira la cámara
            float anguloObjetivo = Mathf.Atan2(direccion.x, direccion.z) * Mathf.Rad2Deg + camaraTransform.eulerAngles.y;
            
            // Rotamos el personaje hacia ese ángulo
            transform.rotation = Quaternion.Euler(0f, anguloObjetivo, 0f);

            // Movemos el personaje
            Vector3 direccionMovimiento = Quaternion.Euler(0f, anguloObjetivo, 0f) * Vector3.forward;
            controller.Move(direccionMovimiento.normalized * velocidad * Time.deltaTime);
        }
    }
}