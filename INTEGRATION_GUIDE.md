# Integração: Unity + Spring + MySQL

## Arquitetura

```
┌─────────────────────────────────────────────────────────┐
│ Turtle Trash Fighter (Unity Game)                       │
│ - RemoteSaveService: Comunica via HTTP com o backend    │
│ - RemoteAuthSession: Gerencia JWT do usuário            │
│ - BaseUrl: http://localhost:8080 (Spring Backend)       │
└─────────────────────────────────────────────────────────┘
                          │
                   HTTP (JSON + JWT)
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│ Comunidade TTF (Spring Boot)                            │
│ - Porta: 8080                                            │
│ - AuthController: POST /auth/register, /auth/login      │
│ - GameSaveController: GET/PUT/DELETE /saves/*           │
│ - SecurityConfig: JWT + CORS habilitado                 │
└─────────────────────────────────────────────────────────┘
                          │
                   JDBC (com.mysql.cj)
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│ MySQL Database (turtle_trash_fighter)                   │
│ - Tabelas: game_users, game_saves, user_configs         │
│ - Banco compartilhado entre ambas APIs                  │
└─────────────────────────────────────────────────────────┘
```

## Fluxo Completo: Login → Save → Load

### 1. Registro/Login (Unity → Spring)

**Unity envia:**
```json
POST http://localhost:8080/auth/register
{
  "login": "player123",
  "password": "senha123",
  "nome": "Player Name"
}
```

**Spring responde:**
```json
{
  "accessToken": "eyJhbGc...",
  "login": "player123",
  "nome": "Player Name"
}
```

**O que ocorre:**
1. RemoteSaveService.Register() é chamado no Unity
2. SpringBoot recebe em AuthController.register()
3. AuthService cria GameUser com senha hasheada (BCrypt)
4. JwtService gera token JWT com durabilidade de 120min
5. Token é armazenado em RemoteAuthSession para futuras requisições

### 2. Salvar Game (Unity → Spring → MySQL)

**Unity envia:**
```json
PUT http://localhost:8080/saves
Authorization: Bearer eyJhbGc...
{
  "slotIndex": 0,
  "slotName": "Slot 1",
  "selectedCharacter": "Warrior",
  "playTutorial": true,
  "difficulty": "Normal",
  "sceneName": "SampleScene",
  "checkpointId": "cp-start",
  "checkpointX": 10.5,
  "checkpointY": 2.0,
  "checkpointZ": 0.0,
  "collectedIds": ["c1", "c2"],
  "deadEnemyIds": ["e1"],
  "completionPercent": 25.5
}
```

**Spring insere em MySQL:**
```sql
INSERT INTO game_saves (user_id, slot_index, slot_name, ...)
VALUES (1, 0, "Slot 1", ...)
```

**O que ocorre:**
1. RemoteSaveService.SaveGame() serializa GameControler, ColetavelState, EnemyState em JSON
2. BuildJsonRequest adiciona Authorization header com Bearer token
3. SpringBoot autentica via JwtAuthenticationFilter
4. GameSaveController extrai login do Principal
5. GameSaveService valida slotIndex (0-4) e upsert na tabela game_saves
6. Banco retorna o ID do save criado
7. Resposta chega ao Unity com status de sucesso

### 3. Carregar Game (Unity ← Spring ← MySQL)

**Unity envia:**
```
GET http://localhost:8080/saves/0
Authorization: Bearer eyJhbGc...
```

**Spring retorna:**
```json
{
  "slotIndex": 0,
  "slotName": "Slot 1",
  "selectedCharacter": "Warrior",
  "playTutorial": true,
  "difficulty": "Normal",
  "sceneName": "SampleScene",
  "checkpointId": "cp-start",
  "checkpointX": 10.5,
  "checkpointY": 2.0,
  "checkpointZ": 0.0,
  "collectedIdsJson": "[\"c1\",\"c2\"]",
  "deadEnemyIdsJson": "[\"e1\"]",
  "completionPercent": 25.5,
  "lastSavedAtUtc": "2026-04-28T10:15:30"
}
```

**O que ocorre:**
1. RemoteSaveService.LoadGame() envia GET para /saves/{slotIndex}
2. GameSaveController recupera do Principal e passa para GameSaveService
3. GameSaveService busca na tabela game_saves (SQL query)
4. SaveResponse é montada convertendo JSON para listas
5. Unity recebe e converte de volta para SavePayload
6. RemoteSaveService.ApplySavePayload() restaura CheckpointState, ColetavelState, EnemyState
7. Game carrega na cena no checkpoint correto com progresso restaurado

## Como Subir Tudo

### Passo 1: Criar o Banco de Dados

```bash
# Abrir MySQL
mysql -u root -p

# Executar o schema
source comunidadettf/DatabaseSchema.sql

# Verificar tabelas criadas
USE turtle_trash_fighter;
SHOW TABLES;
```

### Passo 2: Rodar o Backend Spring

```bash
cd comunidadettf
./mvnw.cmd spring-boot:run
# ou
./mvnw.cmd compile
./mvnw.cmd spring-boot:run
```

Backend sobe em: `http://localhost:8080`

Verificar: `curl http://localhost:8080/health`

### Passo 3: Testar a Integração no Unity

1. Abrir o Editor Unity
2. Selecionar o GameObject com RemoteSaveService no menu de saves
3. Preencher:
   - **Base Url:** `http://localhost:8080`
   - **Slot Index:** `0`
   - **Slot Name:** `Slot 1`
4. No menu de Login:
   - Clicar em "Registrar" ou "Entrar"
   - Usar credenciais de teste: `player123` / `senha123`
5. Após autenticado, clicar "Salvar"
6. Reabrir o menu e clicar "Carregar"
7. Verificar que o game restaurou corretamente no checkpoint salvo

## Banco de Dados Compartilhado

Ambas as APIs apontam para o mesmo banco:

| Componente | Host | Database | Porta |
|-----------|------|----------|-------|
| SaveApi (C#) | localhost | turtle_trash_fighter | 3306 |
| ComunidadeTTF (Java) | localhost | turtle_trash_fighter | 3306 |

**Tabelas:**
- `game_users`: Usuários do jogo (login, senha hash, nome)
- `game_saves`: Saves dos players por slot
- `user_configs`: Configurações (volume, keybinds)
- `usuario`: Tabela da comunidade (separada)

## Endpoint Map

| Rota | Método | Autenticação | Descrição |
|------|--------|--------------|-----------|
| `/health` | GET | ✗ | Status da API |
| `/auth/register` | POST | ✗ | Criar novo usuário |
| `/auth/login` | POST | ✗ | Autenticar e gerar JWT |
| `/saves` | GET | ✓ | Listar todos os saves do usuário |
| `/saves/{slotIndex}` | GET | ✓ | Obter um save específico |
| `/saves` | PUT | ✓ | Salvar/atualizar um slot |
| `/saves/{slotIndex}` | DELETE | ✓ | Deletar um save |

## Segurança

- **Hashing:** BCryptPasswordEncoder (Spring)
- **JWT:** HS256, chave de 256 bits, TTL 120 minutos
- **CORS:** Habilitado para localhost:* durante desenvolvimento
- **Validação:** Slot index (0-4), campos obrigatórios verificados

## Troubleshooting

### "Connection refused" ao fazer login

- Verificar se Spring está rodando: `curl http://localhost:8080/health`
- Checar firewall: porta 8080 deve estar aberta
- No Editor Unity, verificar a URL em RemoteSaveService

### "Database authentication failed"

- Verificar credenciais MySQL em `application.yaml`
- Default: `root` / `` (sem senha)
- Se mudar, atualizar `application.yaml` ou var env `DATASOURCE_PWD`

### "JWT signature invalid"

- Verificar que ambas as APIs têm o mesmo segredo JWT
- Padrão: `troque-essa-chave-super-segura-para-o-spring-ttf-123456`
- Em produção, usar var env `JWT_SECRET`

### "Slot index invalid"

- Valores válidos: 0, 1, 2, 3, 4 (5 slots totais)
- Verificar no RemoteSaveService que slotIndex está entre 0-4

## Next Steps

1. Integrar comunidade TTF no mesmo banco
2. Criar endpoints de leaderboard/achievements
3. Implementar sincronização automática de saves
4. Adicionar versionamento de API
