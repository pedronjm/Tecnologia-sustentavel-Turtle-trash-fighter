# SaveApi - Turtle Trash Fighter

API HTTP para autenticação e save remoto do jogo.

## Recursos

- Login e cadastro com usuario/senha
- JWT para autorizar operacoes de save
- Save por usuario com:
  - coletaveis coletados
  - inimigos mortos
  - checkpoint atual
  - porcentagem de completude

## Rodar localmente

```bash
dotnet run
```

Por padrao, sobe em algo como `http://localhost:5000`.

## Endpoints

- `POST /auth/register`
- `POST /auth/login`
- `GET /save` (autenticado)
- `PUT /save` (autenticado)

## Exemplo de corpo para save

```json
{
  "sceneName": "fase1",
  "collectedIds": ["c1", "c2"],
  "deadEnemyIds": ["e1", "e2"],
  "checkpointId": "cp-01",
  "checkpointPosition": { "x": 10, "y": 2, "z": 0 },
  "completionPercent": 67.5
}
```

## Seguranca

- Troque a chave JWT em `Program.cs` para producao.
- Em ambiente real, mova usuarios/saves para banco de dados.
