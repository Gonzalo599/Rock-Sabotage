using UnityEngine;
using Photon.Pun;

public class PersonajeMenu : MonoBehaviourPun
{
    public float velocidad = 5f;
    public float velocidadGiro = 15f; 
    public float gravedad = 20f; 

    private CharacterController controller;
    private Animator anim;
    private Vector3 velocidadVertical;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
        
        if (!photonView.IsMine) return;
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        if (controller.isGrounded && velocidadVertical.y < 0)
        {
            velocidadVertical.y = -2f; 
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direccion = new Vector3(horizontal, 0f, vertical).normalized;
        bool seMueve = direccion.magnitude >= 0.1f;
        if (anim != null)
        {
            anim.SetBool("Correr", seMueve);
        }
        // -----------------------------

        if (seMueve)
        {
            float anguloObjetivo = Mathf.Atan2(direccion.x, direccion.z) * Mathf.Rad2Deg;
            float anguloSuave = Mathf.LerpAngle(transform.eulerAngles.y, anguloObjetivo, velocidadGiro * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, anguloSuave, 0f);

            controller.Move(direccion * velocidad * Time.deltaTime);
        }

        velocidadVertical.y -= gravedad * Time.deltaTime;
        controller.Move(velocidadVertical * Time.deltaTime);
    }
}