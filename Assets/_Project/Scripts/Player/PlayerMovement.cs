using UnityEngine;

// Os dois-pontos (:) equivalem ao 'extends' do Java
// [RequireComponent] é uma Annotation (Attribute em C#) que impede alguém de remover o Rigidbody por engano na Unity
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    // [Header] e [SerializeField] expõem variáveis 'private' no painel Inspector da Unity!
    // Assim você mantém o encapsulamento (private), mas consegue ajustar a velocidade na tela sem recompilar.
    [Header("Configuração de movimento")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rotationSpeed = 15f;

    private Rigidbody _rb;
    private InputSystem_Actions _inputActions;
    private Vector2 _moveInput;// Guarda o X (lados) e Y (frente/trás) do teclado/controle

    private void Awake()
    {
        // GetComponent busca o componente Rigidbody que está na mesma "caixa" (GameObject) que este script
        _rb = GetComponent<Rigidbody>();

        // Instancia a classe gerada pelo Input System
        _inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        // Liga a escuta do teclado/controle quando o Player está ativo na cena
        _inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        // Desliga a escuta quando o objeto é destruído ou desativado (evita Memory Leak)
        _inputActions.Player.Disable();
    }

    private void Update()
    {
        // 1. LEITURA DE INPUT (Sempre no Update)
        // Lê o Vector2 (-1 a 1) da ação "Move" configurada no mapa Player
        _moveInput = _inputActions.Player.Move.ReadValue<Vector2>();

        // Vai printar (0.0, 0.0) parado, ou (1.0, 0.0) andando, etc.
        Debug.Log("Input atual: " + _moveInput);
    }

    private void FixedUpdate()
    {
        MovePlayer();
        RotatePlayer();
    }

    private void MovePlayer()
    {
        // Converte o Vector2 do teclado (X, Y) para o mundo 3D da Unity (X = lados, Y = altura, Z = profundidade)
        // Mantemos a velocidade Y atual do Rigidbody (_rb.linearVelocity.y) para não anular a gravidade!
        Vector3 targetVelocity = new Vector3(_moveInput.x * moveSpeed, _rb.linearVelocity.y, _moveInput.y * moveSpeed);

        // Na Unity 6, a propriedade 'velocity' foi renomeada para 'linearVelocity' para maior clareza física
        _rb.linearVelocity = targetVelocity;
    }

    private void RotatePlayer()
    {
        // Direção horizontal pura (ignorando o eixo Y de altura)
        Vector3 lookDirection = new Vector3(_moveInput.x, 0f, _moveInput.y);

        // sqrMagnitude > 0.01f verifica se o jogador realmente está apertando alguma tecla.
        // Se soltar as teclas, não queremos que o boneco volte a olhar para o ângulo zero (norte).
        if (lookDirection.sqrMagnitude > 0.01f)
        {
            // Quaternion é a estrutura matemática que representa rotações 3D sem sofrer de "Gimbal Lock"
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);

            // Slerp (Spherical Linear Interpolation) gira suavemente da rotação atual até a rotação alvo.
            // Time.fixedDeltaTime garante que a velocidade do giro seja consistente com o relógio da física.
            Quaternion smoothRotation = Quaternion.Slerp(_rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

            _rb.MoveRotation(smoothRotation);
        }
    }

}
