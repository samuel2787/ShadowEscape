using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Configuración de Mecánica de Alma")]
    [SerializeField] private GameObject objetoAlma; // Arrastra aquí a AxelCane_Alma
    [SerializeField] private bool esElAlma = false;   // Marca esta casilla SÓLO en el inspector del Alma

    [Header("Configuración de Cámara")]
    // Arrastra aquí el objeto "CinemachineCamera" de tu jerarquía
    [SerializeField] private Unity.Cinemachine.CinemachineCamera camaraVirtual;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;

    private static PlayerMovement cuerpoPrincipal;
    private static PlayerMovement almaClon;
    private static bool esAstral = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;

        if (!esElAlma)
        {
            cuerpoPrincipal = this;
            if (objetoAlma != null)
            {
                almaClon = objetoAlma.GetComponent<PlayerMovement>();
                objetoAlma.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (!esAstral && !esElAlma)
        {
            UpdateAnimations();
        }
        else if (esAstral && esElAlma)
        {
            UpdateAnimations();
        }
    }

    void FixedUpdate()
    {
        if (!esAstral && !esElAlma)
        {
            rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
        }
        else if (esAstral && esElAlma)
        {
            rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void ActivarMecanicaB(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        if (!esAstral)
        {
            esAstral = true;

            if (almaClon != null && cuerpoPrincipal != null)
            {
                cuerpoPrincipal.moveInput = Vector2.zero;
                cuerpoPrincipal.rb.linearVelocity = Vector2.zero;
                if (cuerpoPrincipal.animator != null) cuerpoPrincipal.animator.SetBool("isWalking", false);

                almaClon.gameObject.transform.position = cuerpoPrincipal.transform.position;
                almaClon.gameObject.SetActive(true);

                // CAMBIO AQUÍ: La cámara ahora sigue al alma
                if (cuerpoPrincipal.camaraVirtual != null)
                {
                    cuerpoPrincipal.camaraVirtual.Follow = almaClon.transform;
                }
            }
        }
        else
        {
            esAstral = false;

            if (almaClon != null && cuerpoPrincipal != null)
            {
                almaClon.moveInput = Vector2.zero;
                almaClon.rb.linearVelocity = Vector2.zero;
                almaClon.gameObject.SetActive(false);

                // CAMBIO AQUÍ: La cámara vuelve a seguir al cuerpo físico
                if (cuerpoPrincipal.camaraVirtual != null)
                {
                    cuerpoPrincipal.camaraVirtual.Follow = cuerpoPrincipal.transform;
                }
            }
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        if (moveInput != Vector2.zero)
        {
            animator.SetBool("isWalking", true);
            animator.SetFloat("InputX", moveInput.x);
            animator.SetFloat("InputY", moveInput.y);
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }
}