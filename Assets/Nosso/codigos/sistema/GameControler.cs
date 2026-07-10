using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class GameControler : MonoBehaviour
{
    // // Textos na tela
    // public Text garrafa;
    // public Text engrenagem;
    // public Text maca;
    // public Text circuito;
    // public Text walk;

    // // Objetos
    public GameObject Gameover;

    // public GameObject DICAS;
    // public GameObject ganhou;

    public float maxHealth;

    // public GameObject hudvida;
    // public GameObject inicio;
    // public GameObject notCollected;
    // public GameObject coletavel;
    public GameObject jogador;

    // Coletáveis (usados pelos scripts garrafa, circuito, engrenagem, Maçã Pode)
    [HideInInspector]
    public float qttgarrafa;

    [HideInInspector]
    public float qttengrenagem;

    [HideInInspector]
    public float qttmaca;

    [HideInInspector]
    public float qttcircuito;

    [HideInInspector]
    public float qttPapel;

    [HideInInspector]
    public float qttPlastico;

    // [HideInInspector] public float rstgarrafa;
    // [HideInInspector] public float rstengrenagem;
    // [HideInInspector] public float rstmaca;
    // [HideInInspector] public float rstcircuito;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip collectClip;
    public AudioClip playerAttackClip;
    public AudioClip playerHitClip;
    public AudioClip playerDeathClip;
    public AudioClip enemyHitClip;
    public AudioClip enemyDeathClip;

    // Variáveis auxiliares
    // public float fase;
    // private int aux = 1;
    // public float inaux;
    [HideInInspector]
    public float health = 10;

    /*
        private bool canPassStage = true; // Controla se é possível passar de fase
        public float passStageCooldown = 1f;
    
        private enum TutorialSteps { WalkRight, WalkLeft, Jump, DoubleJump, Dash }
        private TutorialSteps currentStep = TutorialSteps.WalkRight;
        private bool tutorialComplete = false;
        private bool stepInProgress = false;
    */
    public static GameControler instance;

    // Contagem de coletáveis na cena (total e quantos restam)
    private int totalColetaveis;
    private int coletados;
    private bool gameOverTriggered;

    // Respawn / checkpoint
    [HideInInspector]
    public Vector3 lastCheckpoint;

    [HideInInspector]
    public bool hasCheckpoint = false;
    public float respawnDelay = 1f;

    [Header("Menu")]
    public string mainMenuSceneName = "menu";

    // referência à corrotina de respawn ativa
    private Coroutine activeRespawnCoroutine;

    [Header("Respawn")]
    public bool autoRespawn = false; // quando true, respawna automaticamente após death()
    public bool gameOverFullScreen = true; // quando true, o painel Gameover preencherá a tela

    /// <summary>Total de coletáveis que existiam na cena ao iniciar.</summary>
    public int TotalColetaveis => totalColetaveis;

    /// <summary>Quantos já foram coletados.</summary>
    public int Coletados => coletados;

    /// <summary>Quantos ainda restam para coletar.</summary>
    public int Restantes => totalColetaveis - coletados;

    [Header("UI - Coletáveis restantes (opcional)")]
    [Tooltip(
        "Se atribuir um Text aqui, ele mostrará quantos coletáveis restam (ex: \"Restam: 5\")."
    )]
    public Text textoRestantes;

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

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

    void Start()
    {
        ContarColetaveisNaCena();
        AtualizarTextoRestantes();
        Gameover.SetActive(false);

        // registra posição inicial como checkpoint padrão (spawn inicial)
        if (CheckpointState.instance != null
            && CheckpointState.instance.TryGetCheckpointPosition(out Vector3 checkpointPos))
        {
            lastCheckpoint = checkpointPos;
            hasCheckpoint = true;
        }
        else if (jogador != null)
        {
            lastCheckpoint = jogador.transform.position;
            hasCheckpoint = true;
        }
    }

    void Update()
    {
        Damageable playerDamageable =
            Player.player != null ? Player.player.GetComponent<Damageable>() : null;
        if (playerDamageable != null && playerDamageable.currentHealth <= 0)
        {
            death();
        }
    }

    /// <summary>Conta quantos objetos Coletavel existem na cena (garrafa, circuito, engrenagem, maçã, etc.).</summary>
    void ContarColetaveisNaCena()
    {
        Coletavel[] todos = FindObjectsByType<Coletavel>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );
        totalColetaveis = todos != null ? todos.Length : 0;
        coletados = 0;
    }

    /// <summary>Chamado por cada Coletavel quando é coletado. Atualiza a contagem e o texto na tela.</summary>
    public void RegistrarColetavelColetado()
    {
        coletados++;
        AtualizarTextoRestantes();
    }

    public void PlayCollectSound()
    {
        PlayClip(collectClip);
    }

    public void PlayPlayerAttackSound()
    {
        PlayClip(playerAttackClip);
    }

    public void PlayPlayerHitSound()
    {
        PlayClip(playerHitClip);
    }

    public void PlayPlayerDeathSound()
    {
        PlayClip(playerDeathClip);
    }

    public void PlayEnemyHitSound()
    {
        PlayClip(enemyHitClip);
    }

    public void PlayEnemyDeathSound()
    {
        PlayClip(enemyDeathClip);
    }

    void PlayClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    void AtualizarTextoRestantes()
    {
        if (textoRestantes != null)
            textoRestantes.text = $"Restam: {Restantes}";
    }

    /*
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
     */
    /*  public bool TodosItensColetados()
     {
         return rstgarrafa == 0 && rstengrenagem == 0 && rstmaca == 0 && rstcircuito == 0;
     }
 
     public void AtualizarColetas()
     {
         AtualizarTexto(garrafa, qttgarrafa, rstgarrafa);
         AtualizarTexto(engrenagem, qttengrenagem, rstengrenagem);
         AtualizarTexto(maca, qttmaca, rstmaca);
         AtualizarTexto(circuito, qttcircuito, rstcircuito);
     } */

    /*  private void AtualizarTexto(Text texto, float qtt, float rst)
     {
         texto.text = rst == 0 ? "já coletado" : $"{qtt}/{rst}";
         
     }
     public void ShowGanhou()
     {
         ganhou?.SetActive(true);
         Debug.Log("Você venceu!");
     }


    */
    public void ShowGameOver()
    {
        if (Gameover != null)
        {
            // Reparent to root Canvas (if needed) and reset RectTransform so it appears centered
            Canvas rootCanvas = Gameover.GetComponentInParent<Canvas>();
            if (rootCanvas == null)
                rootCanvas = FindObjectOfType<Canvas>();

            if (rootCanvas != null)
                Gameover.transform.SetParent(rootCanvas.transform, false);

            // garante que o painel GameOver fique por cima de outros elementos do mesmo Canvas
            Gameover.transform.SetAsLastSibling();

            // se o próprio Gameover tiver um Canvas (canvas separado), force override sorting para garantir topo
            Canvas goCanvas = Gameover.GetComponent<Canvas>();
            if (goCanvas != null)
            {
                goCanvas.overrideSorting = true;
                goCanvas.sortingOrder = 100; // valor alto para garantir topo
            }

            RectTransform rt = Gameover.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.localScale = Vector3.one;
                if (gameOverFullScreen)
                {
                    // preencher toda a tela
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchorMin = new Vector2(0f, 0f);
                    rt.anchorMax = new Vector2(1f, 1f);
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                    rt.anchoredPosition = Vector2.zero;
                    rt.sizeDelta = Vector2.zero;
                }
                else
                {
                    // centralizar sem esticar
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = Vector2.zero;
                }
            }

            Gameover.SetActive(true);
        }
    }

    public void death()
    {
        if (gameOverTriggered)
            return;

        gameOverTriggered = true;
        PlayPlayerDeathSound();
        ShowGameOver();
        if (jogador != null)
        {
            // desativa jogador ao invés de destruir para permitir respawn
            jogador.SetActive(false);
        }

        // Se o painel de GameOver não estiver configurado (null), evita travamento recarregando a cena
        if (Gameover == null)
        {
            activeRespawnCoroutine = StartCoroutine(ResetSceneAfterDelay(respawnDelay));
            return;
        }

        // se autoRespawn estiver habilitado, inicia respawn automático; senão espera ação do jogador via botão
        if (autoRespawn)
        {
            if (HasAnyCheckpoint())
            {
                activeRespawnCoroutine = StartCoroutine(RespawnAfterDelay(respawnDelay));
            }
            else
            {
                // nenhum checkpoint alcançado: recarrega a cena após pequeno delay
                activeRespawnCoroutine = StartCoroutine(ResetSceneAfterDelay(respawnDelay));
            }
        }
    }

    /// <summary>
    /// True se há algum ponto de respawn válido: o checkpoint remoto
    /// (CheckpointState, com posição já registrada pela cena atual) tem
    /// prioridade; o checkpoint local (lastCheckpoint) serve de fallback
    /// para o spawn inicial da fase.
    /// </summary>
    private bool HasAnyCheckpoint()
    {
        return hasCheckpoint
            || (CheckpointState.instance != null && CheckpointState.instance.HasCheckpoint());
    }

    /// <summary>
    /// Resolve a posição de respawn: prioriza o checkpoint salvo em
    /// CheckpointState (apenas se a posição estiver de fato registrada -
    /// ver TryGetCheckpointPosition); cai para lastCheckpoint como fallback.
    /// </summary>
    private Vector3 ResolveSpawnPosition()
    {
        if (CheckpointState.instance != null
            && CheckpointState.instance.TryGetCheckpointPosition(out Vector3 checkpointPos))
        {
            return checkpointPos;
        }

        return lastCheckpoint;
    }

    IEnumerator RespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (jogador != null)
        {
            jogador.SetActive(true);
            jogador.transform.position = ResolveSpawnPosition();

            Damageable dmg = jogador.GetComponent<Damageable>();
            if (dmg != null)
            {
                dmg.currentHealth = dmg.maxHealth;
                dmg.OnHealthChanged?.Invoke(1f);
            }
        }

        gameOverTriggered = false;
        if (Gameover != null)
            Gameover.SetActive(false);

        activeRespawnCoroutine = null;
    }

    IEnumerator ResetSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        // Recarrega a cena atual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>Chamada por botão: reaparece imediatamente (ou recarrega cena se não houver checkpoint).</summary>
    public void RespawnNow()
    {
        // cancela corrotina pendente
        if (activeRespawnCoroutine != null)
        {
            StopCoroutine(activeRespawnCoroutine);
            activeRespawnCoroutine = null;
        }

        // Se houver checkpoint, faz respawn; senão recarrega cena
        if (HasAnyCheckpoint())
        {
            if (jogador != null)
            {
                jogador.SetActive(true);
                jogador.transform.position = ResolveSpawnPosition();

                Damageable dmg = jogador.GetComponent<Damageable>();
                if (dmg != null)
                {
                    dmg.currentHealth = dmg.maxHealth;
                    dmg.OnHealthChanged?.Invoke(1f);
                }
            }

            gameOverTriggered = false;
            if (Gameover != null)
                Gameover.SetActive(false);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    /// <summary>Chamada por botão: vai para o menu principal.</summary>
    public void GoToMainMenu()
    {
        string sceneName = string.IsNullOrEmpty(mainMenuSceneName) ? "menu" : mainMenuSceneName;
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>Define o checkpoint atual para respawn. Se fornecer um checkpointId, também atualiza o CheckpointState global (save).</summary>
    public void SetCheckpoint(Vector3 position, string checkpointId = null)
    {
        lastCheckpoint = position;
        hasCheckpoint = true;

        if (!string.IsNullOrEmpty(checkpointId) && CheckpointState.instance != null)
        {
            CheckpointState.instance.SetCheckpoint(checkpointId);
        }
    }

    /// <summary>
    /// Chamado pelo RemoteSaveService logo depois de aplicar um save
    /// carregado (RemoteSaveService.AplicarSave). É aqui que o checkpoint
    /// realmente vira "carregar o jogo": move o jogador para a posição
    /// salva e restaura a vida no componente Damageable de verdade
    /// (antes disso só existiam os campos health/maxHealth do GameControler,
    /// que não têm nenhuma ligação com o Damageable do player).
    /// </summary>
    /// <param name="position">Posição do checkpoint a aplicar.</param>
    /// <param name="posicaoValida">
    /// true se a posição veio de CheckpointState.TryGetCheckpointPosition
    /// (ou seja, o checkpoint já está registrado nesta cena). Se false, o
    /// jogador não é movido (evita jogar o player para Vector3.zero).
    /// </param>
    public void AplicarCheckpointCarregado(
        Vector3 position,
        bool posicaoValida,
        int currentHealth,
        int maxHealthValue
    )
    {
        Debug.Log(
            $"[GameControler] AplicarCheckpointCarregado -> posicaoValida={posicaoValida} pos={position} vida={currentHealth}/{maxHealthValue}"
        );

        if (jogador == null)
        {
            Debug.LogWarning(
                "[GameControler] Nao foi possivel aplicar o checkpoint carregado: referencia 'jogador' esta vazia no Inspector."
            );
            return;
        }

        if (posicaoValida)
        {
            lastCheckpoint = position;
            hasCheckpoint = true;

            jogador.SetActive(true);
            jogador.transform.position = position;
            Debug.Log($"[GameControler] Jogador reposicionado no checkpoint: {position}");
        }
        else
        {
            Debug.LogWarning(
                "[GameControler] Checkpoint carregado ainda nao tem posicao registrada nesta cena (nenhum CheckpointTrigger com esse ID rodou o Awake ainda). Jogador permanece na posicao padrao da cena."
            );
        }

        Damageable dmg = jogador.GetComponent<Damageable>();
        if (dmg != null && maxHealthValue > 0)
        {
            dmg.maxHealth = maxHealthValue;
            dmg.currentHealth = Mathf.Clamp(currentHealth, 0, maxHealthValue);
            dmg.OnHealthChanged?.Invoke((float)dmg.currentHealth / dmg.maxHealth);
            Debug.Log(
                $"[GameControler] Vida do jogador restaurada: {dmg.currentHealth}/{dmg.maxHealth}"
            );
        }
        else
        {
            Debug.LogWarning(
                "[GameControler] Nao foi possivel restaurar a vida: Damageable nao encontrado no jogador ou maxHealth invalido."
            );
        }

        // mantém os campos legados sincronizados, caso algo mais no projeto
        // ainda leia GameControler.health/maxHealth diretamente.
        health = currentHealth;
        maxHealth = maxHealthValue;
    }
    /*
       
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
        } */
}