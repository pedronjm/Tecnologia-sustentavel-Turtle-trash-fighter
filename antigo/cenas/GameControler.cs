using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameControler : MonoBehaviour
{
    // Textos na tela
    public Text garrafa;
    public Text engrenagem;
    public Text maca;
    public Text circuito;
    public Text walk;

    // Objetos
    public GameObject Gameover;
    public GameObject DICAS;
    public GameObject ganhou;
    public GameObject vida;
    public GameObject hudvida;
    public GameObject inicio;
    public GameObject notCollected;
    public GameObject coletavel;
    public GameObject jogador;

    // Coletáveis
    public float qttgarrafa;
    public float qttengrenagem;
    public float qttmaca;
    public float qttcircuito;
    public float rstgarrafa;
    public float rstengrenagem;
    public float rstmaca;
    public float rstcircuito;

    // Áudios
    public AudioSource audioSource;
    public AudioClip coletar;
    public AudioClip jogarfora;
    public AudioClip stepcomplete;
    public AudioClip tutorialcomplete;
    public AudioClip fasecomplete;
    public AudioClip heal;



    // Variáveis auxiliares
    public float fase;
    private int aux = 1;
    public float inaux;
    public float health = 5;

    private bool canPassStage = true; // Controla se é possível passar de fase
    public float passStageCooldown = 1f;

    private enum TutorialSteps { WalkRight, WalkLeft, Jump, DoubleJump, Dash }
    private TutorialSteps currentStep = TutorialSteps.WalkRight;
    private bool tutorialComplete = false;
    private bool stepInProgress = false;

    public static GameControler instance;

    void Awake()
    {
        // Singleton
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Configurações iniciais
        if (fase == 1)
        {
            inicio.SetActive(true); // Botão inicial visível
        }
        if(fase != 1){
            audioSource.PlayOneShot(fasecomplete);
        }
        Gameover.SetActive(false);
        health = vida.transform.childCount;
        hudvida.SetActive(false); //

    }

    void Update()
    {
        if (inaux != 0 || fase != 1)
        {
            hudvida.SetActive(true);
            imghearts();
        }

        if (fase != 1)
        {
            AtualizarColetas();
        }

        if (fase == 1 && !tutorialComplete)
        {
            AtualizarTutorialTextos(); // Atualiza o texto do tutorial em cada frame
            VerificarTutorial();      // Verifica se as condições para o próximo passo foram atendidas
        }

        if (health <= 0 && aux == 1)
            death();

        if (TodosItensColetados() && fase == 5)
            ShowGanhou();

        if (fase == 1)
        {
            coletavel.SetActive(false);
        }
    }

    public bool TodosItensColetados()
    {
        return rstgarrafa == 0 && rstengrenagem == 0 && rstmaca == 0 && rstcircuito == 0;
    }

    public void AtualizarColetas()
    {
        AtualizarTexto(garrafa, qttgarrafa, rstgarrafa);
        AtualizarTexto(engrenagem, qttengrenagem, rstengrenagem);
        AtualizarTexto(maca, qttmaca, rstmaca);
        AtualizarTexto(circuito, qttcircuito, rstcircuito);
    }

    private void AtualizarTexto(Text texto, float qtt, float rst)
    {
        texto.text = rst == 0 ? "já coletado" : $"{qtt}/{rst}";
        
    }

    public void ShowGameOver()
    {
        Gameover?.SetActive(true);
        Debug.Log("Game Over");
    }

    public void ShowGanhou()
    {
        ganhou?.SetActive(true);
        Debug.Log("Você venceu!");
    }

    public void death()
    {
        aux = 0;
        ShowGameOver();
        if (jogador != null)
            Destroy(jogador, 0.25f);
    }

    void imghearts()
    {
        int maxHealth = vida.transform.childCount;
        health = Mathf.Clamp(health, 0, maxHealth);

        for (int i = 0; i < maxHealth; i++)
            vida.transform.GetChild(i).gameObject.SetActive(i < health);
    }

    IEnumerator ensinar()
    {
        DICAS.SetActive(true);
        Debug.Log("Iniciando tutorial...");
        while (!tutorialComplete)
        {
            AtualizarTutorialTextos();
            yield return null;
        }

        StartCoroutine(showimage());
        yield return new WaitForSeconds(3f);
        DICAS.SetActive(false);
    }

    IEnumerator showimage()
    {
        Debug.Log("Tutorial concluído!");
        walk.text = "Tutorial concluído! Parabéns!";
        walk.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        walk.gameObject.SetActive(false);
    }

    private void AtualizarTutorialTextos()
    {
        if (Player.player == null)
        {
            Debug.LogWarning("Player instance is null!");
            return;
        }

        if (stepInProgress)
            return;

        switch (currentStep)
        {
            case TutorialSteps.WalkRight:
                walk.text = $"Ande para a direita (Pressione D): {Player.player.drt.ToString("F2")}/1";
                break;
            case TutorialSteps.WalkLeft:
                walk.text = $"Ande para a esquerda (Pressione A): {Player.player.esquerda.ToString("F2")}/1";
                break;
            case TutorialSteps.Jump:
                walk.text = $"Pressione Espaço para pular: {Player.player.pulo}/3";
                break;
            case TutorialSteps.DoubleJump:
                walk.text = $"Pressione Espaço duas vezes para pular mais alto: {Player.player.pulodlp}/3";
                break;
            case TutorialSteps.Dash:
                walk.text = $"Pressione Shift para dar dash: {Player.player.dsh}/3";
                break;
        }
    }

    private void VerificarTutorial()
    {
        if (Player.player == null || stepInProgress)
            return;

        switch (currentStep)
        {
            case TutorialSteps.WalkRight:
                if (Player.player.drt >= 1) StartCoroutine(TrocarEtapa());
                break;
            case TutorialSteps.WalkLeft:
                if (Player.player.esquerda >= 1) StartCoroutine(TrocarEtapa());
                break;
            case TutorialSteps.Jump:
                if (Player.player.pulo >= 3) StartCoroutine(TrocarEtapa());
                break;
            case TutorialSteps.DoubleJump:
                if (Player.player.pulodlp >= 3) StartCoroutine(TrocarEtapa());
                break;
            case TutorialSteps.Dash:
                if (Player.player.dsh >= 3) StartCoroutine(TrocarEtapa());
                break;
        }
    }

    private IEnumerator TrocarEtapa()
    {
        stepInProgress = true;
        yield return new WaitForSeconds(1f);

        if (currentStep == TutorialSteps.Dash)
        {
            tutorialComplete = true;
            audioSource.PlayOneShot(tutorialcomplete);
        }
        else
        {
            currentStep++;
            audioSource.PlayOneShot(stepcomplete);
            ResetContadores();
        }

        stepInProgress = false;
    }

    private void ResetContadores()
    {
        if (Player.player == null) return;

        Player.player.drt = 0;
        Player.player.esquerda = 0;
        Player.player.pulo = 0;
        Player.player.pulodlp = 0;
        Player.player.dsh = 0;
    }

    public void start()
    {
        Debug.Log("Botão Start clicado!");
        inicio.SetActive(false);
        DICAS.SetActive(true);
        imghearts();
        inaux++;
        if (fase == 1 && !tutorialComplete)
        {
            StartCoroutine(ensinar());
        }
    }

    public void RestartGame(string read)
    {
        SceneManager.LoadScene(read);
    }


    public void NotCollected()
    {
        if (notCollected != null)
        {
            notCollected.SetActive(true);
            StartCoroutine(DesativarNotCollected());
        }
    }

    IEnumerator DesativarNotCollected()
    {
        yield return new WaitForSeconds(2);
        notCollected?.SetActive(false);
    }

    public void PassarDeFase()
    {
        if (canPassStage)
        {
            canPassStage = false; // Bloqueia novas ativações até o cooldown terminar
            StartCoroutine(PassStageWithDelay());
        }
    }

    private IEnumerator PassStageWithDelay()
    {
        if (fase < 5)
        {
            fase++;
            SceneManager.LoadScene($"fase{fase}");
            audioSource.PlayOneShot(fasecomplete);
        }
        else
        {
            ShowGanhou();
        }

        // Aguarda o tempo de cooldown antes de permitir nova ativação
        yield return new WaitForSeconds(passStageCooldown);
        canPassStage = true; // Libera novamente para passar de fase
    }
}

