# SaveApi - Turtle Trash Fighter

API HTTP para autenticação, configs e saves do jogo usando MySQL local (WAMP).

## Recursos

- Login e cadastro com usuario/login/nome
- JWT para autorizar operacoes protegidas
- Configs por usuario:
  - volume master
  - volume music
  - volume sfx
  - keybinds em JSON
- Saves por usuario e slot com:
  - personagem
  - tutorial ligado/desligado
  - dificuldade
  - coletaveis coletados
  - inimigos mortos
  - checkpoint atual
  - porcentagem de completude

## Rodar localmente

```bash
dotnet run
```

Antes de rodar, crie o banco e as tabelas com o arquivo `DatabaseSchema.sql`.

Por padrao, sobe em algo como `http://localhost:5000`.

## Endpoints

- `POST /auth/register`
- `POST /auth/login`
- `GET /config` (autenticado)
- `PUT /config` (autenticado)
- `GET /saves` (autenticado)
- `GET /saves/{slotIndex}` (autenticado)
- `PUT /saves` (autenticado)
- `DELETE /saves/{slotIndex}` (autenticado)

## Exemplo de corpo para config

```json
{
  "volumeMaster": 1,
  "volumeMusic": 0.8,
  "volumeSfx": 0.9,
  "keybinds": {
    "jump": "Space",
    "dash": "LeftShift"
  }
}
```

## Exemplo de corpo para save

```json
{
  "slotIndex": 0,
  "slotName": "Slot 1",
  "selectedCharacter": "Warrior",
  "playTutorial": true,
  "difficulty": "Normal",
  "sceneName": "fase1",
  "checkpointId": "cp-01",
  "checkpointX": 10,
  "checkpointY": 2,
  "checkpointZ": 0,
  "collectedIds": ["c1", "c2"],
  "deadEnemyIds": ["e1", "e2"],
  "completionPercent": 67.5
}
```

## Configuracao

- Ajuste a connection string em `appsettings.json`.
- O JWT tambem pode ser trocado em `appsettings.json`.

## Seguranca

- Troque a chave JWT em `appsettings.json` para producao.
- Em ambiente real, restrinja o acesso ao MySQL local e nao deixe senha vazia.
