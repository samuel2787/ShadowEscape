using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.Video; // ¡NUEVO! Necesario para controlar el video

public class MenuController : MonoBehaviour
{
    private UIDocument document;
    private Button newGame;
    private Button quitGame;
    private Button continueGame;
    private Button optionsGame;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;

    [Header("Cinemática de Introducción")]
    [SerializeField] private VideoPlayer introduccionVideo; // Arrastra el Video Player aquí
    [SerializeField] private string nombrePrimerNivel = "Level1";

    private bool videoReproduciendose = false;

    void Start()
    {
        document = GetComponent<UIDocument>();

        // --- BOTÓN NEW GAME ---
        newGame = document.rootVisualElement.Q<Button>("NewGame");
        newGame.clicked += HandleNewGame;
        newGame.RegisterCallback<MouseEnterEvent>(HandleHoverSound);

        // --- BOTÓN QUIT ---
        quitGame = document.rootVisualElement.Q<Button>("Quit");
        quitGame.clicked += HandleQuitGame;
        quitGame.RegisterCallback<MouseEnterEvent>(HandleHoverSound);

        // --- BOTÓN CONTINUE ---
        continueGame = document.rootVisualElement.Q<Button>("Continue");
        if (continueGame != null)
        {
            continueGame.clicked += HandleContinueGame;
            continueGame.RegisterCallback<MouseEnterEvent>(HandleHoverSound);
        }

        // --- BOTÓN OPTIONS ---
        optionsGame = document.rootVisualElement.Q<Button>("Options");
        if (optionsGame != null)
        {
            optionsGame.clicked += HandleOptionsGame;
            optionsGame.RegisterCallback<MouseEnterEvent>(HandleHoverSound);
        }

        // Configurar el evento de finalización del video si está asignado
        if (introduccionVideo != null)
        {
            introduccionVideo.loopPointReached += AlTerminarVideo;
        }
    }

    void Update()
    {
        // Forma correcta usando el Input System moderno para detectar Espacio o Enter
        if (videoReproduciendose && UnityEngine.InputSystem.Keyboard.current != null)
        {
            if (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame ||
                UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame)
            {
                TerminarCinematicaYCargarNivel();
            }
        }
    }

    private void OnDisable()
    {
        newGame.clicked -= HandleNewGame;
        newGame.UnregisterCallback<MouseEnterEvent>(HandleHoverSound);
        quitGame.clicked -= HandleQuitGame;
        quitGame.UnregisterCallback<MouseEnterEvent>(HandleHoverSound);

        if (continueGame != null)
        {
            continueGame.clicked -= HandleContinueGame;
            continueGame.UnregisterCallback<MouseEnterEvent>(HandleHoverSound);
        }

        if (optionsGame != null)
        {
            optionsGame.clicked -= HandleOptionsGame;
            optionsGame.UnregisterCallback<MouseEnterEvent>(HandleHoverSound);
        }

        if (introduccionVideo != null)
        {
            introduccionVideo.loopPointReached -= AlTerminarVideo;
        }
    }

    private void HandleNewGame()
    {
        PlaySound(clickSound);

        if (introduccionVideo != null)
        {
            // Ocultamos la interfaz del menú para que no estorbe visualmente al reproducir el video
            document.rootVisualElement.style.display = DisplayStyle.None;

            // Iniciamos la reproducción del video
            videoReproduciendose = true;
            introduccionVideo.Play();
        }
        else
        {
            // Si olvidaste poner el video en el inspector, carga directo el nivel para no romper el juego
            SceneManager.LoadScene(nombrePrimerNivel);
        }
    }

    // Este método se ejecuta automáticamente gracias al evento 'loopPointReached' cuando el video llega al final
    private void AlTerminarVideo(VideoPlayer vp)
    {
        TerminarCinematicaYCargarNivel();
    }

    private void TerminarCinematicaYCargarNivel()
    {
        videoReproduciendose = false;
        if (introduccionVideo != null)
        {
            introduccionVideo.Stop();
        }
        SceneManager.LoadScene(nombrePrimerNivel);
    }

    private void HandleQuitGame()
    {
        PlaySound(clickSound);
        Application.Quit();
    }

    private void HandleContinueGame()
    {
        PlaySound(clickSound);
    }

    private void HandleOptionsGame()
    {
        PlaySound(clickSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void HandleHoverSound(MouseEnterEvent evt)
    {
        PlaySound(hoverSound);
    }
}