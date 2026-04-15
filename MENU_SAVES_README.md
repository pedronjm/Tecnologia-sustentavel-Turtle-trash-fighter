# Menu de Saves - Guia de Integração

## Visão Geral

O sistema de saves permite até **5 slots de jogo** com persistência via PlayerPrefs. Cada slot armazena:
- ID e posição do checkpoint atual
- Percentual de progresso (baseado em coletáveis, inimigos derrotados e checkpoint)
- Timestamp do último save

---

## Estrutura de Arquivos

```
Assets/Nosso/codigos/ui/menu/
├── MenuUIController.cs           (Controlador principal do menu)
├── MenuVolumeController.cs       (Painel de volume)
├── MenuKeybindsController.cs     (Painel de atalhos)
├── MenuContactsController.cs     (Painel de contatos)
├── MenuBindingStore.cs           (Persistência de bindings)
├── SaveSlotManager.cs            (Gerenciador de save slots - NEW)
└── MenuSavesController.cs        (UI do menu de saves - NEW)
```

---

## Como Implementar no Unity Editor

### 1) Criar a Hierarquia de Canvas

No seu menu, crie a seguinte estrutura:

```
Canvas (Main Menu Canvas)
├── MainMenuPanel (GameObject com UI do menu principal)
├── SettingsPanel (GameObject com UI de configurações)
├── VolumePanel (GameObject)
├── KeybindsPanel (GameObject)
├── ContactsPanel (GameObject)
└── SavesPanel (NEW - GameObject)
    ├── TitleLabel (TMP_Text: "Saves")
    ├── SlotsContainer (VerticalLayoutGroup)
    │   └── SlotPrefab (Prefab de slot reutilizável)
    ├── SelectedSlotLabel (TMP_Text: mostra info do slot selecionado)
    ├── ButtonGroup (HorizontalLayoutGroup)
    │   ├── LoadButton (Button: "Carregar")
    │   ├── SaveButton (Button: "Salvar")
    │   └── DeleteButton (Button: "Deletar")
    ├── StatusLabel (TMP_Text: mensagens de feedback)
    └── BackButton (Button: "Voltar")
```

### 2) Criar o Prefab de Slot

1. Crie um GameObject vazio chamado `SlotPrefab` dentro de `SavesPanel/SlotsContainer`
2. Adicione um `Button` (Unity.UI.Button) ao prefab
3. Adicione um `Image` (opcional, para background)
4. Adicione um `TMP_Text` como filho para mostrar nome/progresso do slot
5. Deixe o componente `SaveSlotUI` *sem* script (será adicionado dinamicamente)

### 3) Configurar o MenuSavesController

1. Selecione o GameObject `SavesPanel`
2. Adicione o script `MenuSavesController` como componente
3. Preencha os campos:
   - **Slots Container**: arraste `SavesPanel/SlotsContainer`
   - **Slot Prefab**: arraste o prefab criado acima
   - **Selected Slot Label**: arraste `SavesPanel/SelectedSlotLabel`
   - **Load Button**: arraste `SavesPanel/ButtonGroup/LoadButton`
   - **Save Button**: arraste `SavesPanel/ButtonGroup/SaveButton`
   - **Delete Button**: arraste `SavesPanel/ButtonGroup/DeleteButton`
   - **Status Label**: arraste `SavesPanel/StatusLabel`

### 4) Integrar com MenuUIController

1. Selecione o Canvas (raiz)
2. Selecione o `MenuUIController` nele
3. No Inspector, em **Panels**, preencha o novo campo:
   - **Saves Panel**: arraste `SavesPanel`

### 5) Conectar Botões de Navegação

No **MainMenuPanel**, adicione um botão "Saves" que chame:
```
MenuUIController.ShowSaves()
```

No **SavesPanel/BackButton**, configure para chamar:
```
MenuUIController.ShowSettings() // ou ShowMainMenu()
```

---

## Funcionalidades

### Salvar Jogo

1. Clique em um slot vazio ou existente
2. Clique em "Salvar"
3. O checkpoint atual, progresso e histórico são salvos no slot

### Carregar Jogo

1. Clique em um slot com dados (será indicado visualmente)
2. Clique em "Carregar"
3. O jogo volta para a cena `SampleScene` no checkpoint salvo

### Deletar Save

1. Clique em um slot com dados
2. Clique em "Deletar"
3. O slot fica vazio e pronto para novo save

---

## Integração com Checkpoint

Quando você **carrega** um save:

1. `SaveSlotManager.LoadGameFromSlot(index)` é chamado
2. Restaura `CheckpointState` com o ID e posição salvos
3. A cena carrega normalmente
4. O player aparece na posição do checkpoint

**Nota:** O `CheckpointTrigger` na cena deve estar configurado com um ID único para cada checkpoint (ou será gerado automaticamente).

---

## Persistência

- **Local:** PlayerPrefs (funciona offline)
- **Remoto:** Usar `RemoteSaveService` se integrado (sincroniza com backend)

---

## Troubleshooting

| Problema | Solução |
|----------|---------|
| Slots não aparecem | Verifique se `SlotsContainer` está ligado em `MenuSavesController` |
| Botões não funcionam | Confira se todos os campos do controller estão preenchidos |
| Save não persiste | Verifique se `CheckpointState.instance` está ativo na cena |
| Carregar não funciona | Confirme que a cena `SampleScene` existe e está no Build Settings |

---

## Exemplo de Estrutura Recomendada

```yaml
Canvas (RenderMode: ScreenSpaceOverlay)
├── SavesPanel (CanvasGroup para fade in/out)
│   ├── Background (Image, fill screen)
│   ├── Container
│   │   ├── Header
│   │   │   └── Title (TMP_Text: "Seus Saves")
│   │   ├── SlotsContainer (GridLayoutGroup, N=2)
│   │   │   └── [SlotPrefab x5 (preenchido dinamicamente)]
│   │   ├── InfoPanel
│   │   │   └── SelectedSlotLabel (TMP_Text)
│   │   ├── ActionPanel
│   │   │   ├── LoadButton
│   │   │   ├── SaveButton
│   │   │   ├── DeleteButton
│   │   │   └── BackButton
│   │   └── FeedbackPanel
│   │       └── StatusLabel (TMP_Text)
```
