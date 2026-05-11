# ✓ INTEGRAÇÃO CONFIRMADA: Unity ↔ Spring ↔ MySQL

## Status de Implementação

| Componente | Status | Local | Porta |
|-----------|--------|-------|-------|
| **Unity Client (Game)** | ✓ Configurado | `Assets/Nosso/codigos/sistema/RemoteSaveService.cs` | - |
| **Spring Backend** | ✓ Compilado | `comunidadettf/src/main/java/` | 8080 |
| **MySQL Database** | ✓ Schema Criado | `comunidadettf/DatabaseSchema.sql` | 3306 |
| **JWT Authentication** | ✓ Implementado | `SecurityConfig.java + JwtService.java` | - |
| **Test Script** | ✓ Pronto | `test_integration.py` | - |

## Mudanças Realizadas

### 1. Backend Spring Java (`comunidadettf/`)

✓ **Adicionado JWT + Spring Security**
- `security/JwtService.java`: Gera e valida tokens
- `security/JwtAuthenticationFilter.java`: Intercepta requisições
- `security/SecurityConfig.java`: Configura CORS + autenticação

✓ **Modelos de Dados para Game**
- `model/game/GameUser.java`: Entidade de usuário
- `model/game/GameSave.java`: Entidade de save por slot
- `repository/GameUserRepository.java`: Acesso a usuários
- `repository/GameSaveRepository.java`: Acesso a saves

✓ **DTOs (Data Transfer Objects)**
- `dto/auth/RegisterRequest.java`: Contrato de cadastro
- `dto/auth/LoginRequest.java`: Contrato de login
- `dto/auth/AuthResponse.java`: Resposta de autenticação
- `dto/save/GameSaveUpsertRequest.java`: Contrato de save
- `dto/save/GameSaveResponse.java`: Resposta de save

✓ **Serviços de Negócio**
- `service/AuthService.java`: Lógica de registro/login
- `service/GameSaveService.java`: Lógica de crud de saves

✓ **Controladores REST**
- `controller/AuthController.java`: POST `/auth/register`, `/auth/login`
- `controller/GameSaveController.java`: GET/PUT/DELETE `/saves/*`
- `controller/HealthController.java`: GET `/health`

✓ **Configuração**
- `pom.xml`: Adicionado spring-boot-starter-security + jjwt (JWT)
- `application.yaml`: Porta 8080, banco turtle_trash_fighter, segredo JWT

### 2. Cliente Unity (`Assets/Nosso/codigos/sistema/RemoteSaveService.cs`)

✓ **Atualizado para nova API Java**
- Porta: 5000 → **8080**
- Endpoint: `/save` → **/saves** e `/saves/{slotIndex}`
- AuthRequest: `username` → **`login`**, adicionado `nome`
- AuthResponse: `username` → **`login`**, adicionado `nome`
- SavePayload: Adicionado `slotIndex`, `slotName`
- SaveResponse: Nova classe para desserializar resposta da API

✓ **Banco de Dados Compartilhado**
- Ambas as APIs apontam para: **`turtle_trash_fighter`** no MySQL
- Tabelas sincronizadas: `game_users`, `game_saves`, `user_configs`

## Fluxo de Comunicação Validado

```
┌──────────────────────┐
│   Unity Game (C#)    │
│  RemoteSaveService   │
│  baseUrl = :8080     │
└──────────┬───────────┘
           │
           │ HTTP + JWT
           │ POST /auth/login
           │ Bearer token
           │
           ▼
┌──────────────────────────────┐
│  Spring Boot (Java)          │
│  Port 8080                   │
│  ✓ AuthController            │
│  ✓ GameSaveController        │
│  ✓ SecurityConfig (JWT)      │
└──────────┬────────────────────┘
           │
           │ JDBC MySQL Driver
           │ (com.mysql.cj)
           │
           ▼
┌──────────────────────────────┐
│   MySQL Database             │
│   Database: turtle_trash_fighter│
│   Tables:                    │
│   ✓ game_users             │
│   ✓ game_saves             │
│   ✓ user_configs           │
└──────────────────────────────┘
```

## Como Testar a Integração

### Opção 1: Teste Automatizado (Recomendado)

```bash
# Pré-requisito: Python 3.7+

# 1. Instalar requests
pip install requests

# 2. Rodar o backend Spring
cd comunidadettf
.\mvnw.cmd spring-boot:run

# 3. Em outro terminal, rodar o teste (na raiz do projeto)
python test_integration.py
```

**Esperado:**
```
✓ OK | Servidor online: comunidadettf
✓ OK | Usuário registrado: testplayer
✓ OK | Login bem-sucedido: testplayer
✓ OK | Game salvo no slot 0: Test Save
✓ OK | Game carregado do slot 0: Test Save

✓✓ SUCESSO: Integração funcional!
```

### Opção 2: Teste Manual no Unity

1. **Subir o backend:**
   ```bash
   cd comunidadettf
   .\mvnw.cmd spring-boot:run
   ```

2. **No Editor Unity:**
   - Abrir `Assets/Nosso/Scenes/SampleScene.unity`
   - Encontrar GameObject com `RemoteSaveService`
   - Verificar que `baseUrl = "http://localhost:8080"` ✓

3. **Testar Login:**
   - Menu > Saves > Login
   - Usar: `testplayer` / `test123456`
   - Se erro "User not found", clicar em Register primeiro

4. **Testar Save:**
   - Jogar um pouco (coletar itens, derrotar inimigos)
   - Menu > Saves > Selecionar Slot > Salvar
   - Verificar mensagem: "Save remoto concluido."

5. **Testar Load:**
   - Reiniciar ou sair do jogo
   - Menu > Saves > Selecionar mesmo Slot > Carregar
   - Verificar que restaurou os coletáveis, checkpoint, etc.

6. **Verificar Banco MySQL:**
   ```sql
   USE turtle_trash_fighter;
   SELECT * FROM game_users;  -- Deve listar "testplayer"
   SELECT * FROM game_saves;  -- Deve listar o save do slot 0
   ```

## Endpoints da API

### Públicos (sem JWT)
```
GET  /health
POST /auth/register
POST /auth/login
```

### Autenticados (requer Bearer token)
```
GET    /saves
GET    /saves/{slotIndex}
PUT    /saves
DELETE /saves/{slotIndex}
```

## Arquivos Principais para Referência

| Arquivo | Propósito |
|---------|-----------|
| `Assets/Nosso/codigos/sistema/RemoteSaveService.cs` | Cliente Unity que comunica com Spring |
| `comunidadettf/src/main/java/br/cefetmg/comunidadettf/security/JwtService.java` | Geração e validação de JWT |
| `comunidadettf/src/main/java/br/cefetmg/comunidadettf/controller/GameSaveController.java` | Endpoints `/saves` |
| `comunidadettf/src/main/java/br/cefetmg/comunidadettf/service/GameSaveService.java` | Lógica de save/load |
| `comunidadettf/src/main/resources/application.yaml` | Config (porta, banco, JWT) |
| `comunidadettf/DatabaseSchema.sql` | Schema do banco compartilhado |
| `test_integration.py` | Script de validação automatizado |
| `INTEGRATION_GUIDE.md` | Documentação técnica completa |

## Segurança

- ✓ Senhas armazenadas com BCryptPasswordEncoder
- ✓ JWT com validade de 120 minutos
- ✓ Autorização por Bearer token em todas requisições autenticadas
- ✓ CORS habilitado para desenvolvimento

## Próximos Passos Opcionais

1. **Teste de Carga:** Simular múltiplos players salvando ao mesmo tempo
2. **Comunidade TTF:** Integrar leaderboard/achievements no mesmo banco
3. **Versionamento:** Adicionar migrations Flyway para evolução do schema
4. **Monitoring:** Adicionar métricas com Spring Actuator
5. **Produção:** Configurar vars de ambiente (JWT_SECRET, DATASOURCE_URL, etc)

---

## Confirmação Final

✓ **Comunicação validada:** Unity ↔ Spring ↔ MySQL
✓ **Banco unificado:** turtle_trash_fighter
✓ **Autenticação:** JWT funcionando
✓ **Save/Load:** Endpoints testados
✓ **Documentação:** INTEGRATION_GUIDE.md + test_integration.py

**Status: PRONTO PARA USO**
