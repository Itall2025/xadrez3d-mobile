# Deploy no Render

## Estrategia
- Unity app no mobile
- Backend API no Render
- Analise de partidas via endpoint `/api/analysis`
- Persistencia em PostgreSQL via `DATABASE_URL`

## Passos
1. Subir este repositorio no GitHub
2. No Render, criar Web Service via `render.yaml`
3. Criar Postgres no Render e copiar `DATABASE_URL`
4. Configurar variaveis de ambiente:
   - `ALLOWED_ORIGINS` com dominio do app/site
   - `DATABASE_URL` do Postgres Render
   - `STOCKFISH_PATH` (quando binario estiver disponivel)
5. Validar saude em `/api/health`

## Endpoints principais
- `GET /api/health`
- `POST /api/games`
- `GET /api/games`
- `GET /api/games/:id`
- `POST /api/analysis`
- `POST /api/games/:id/analysis`

## Banco
Schema SQL inicial em `backend/sql/001_init_games.sql`.

## Observacao importante
Se `DATABASE_URL` falhar, backend entra em modo `memory` para nao derrubar a API.
Se `STOCKFISH_PATH` nao estiver configurado, a API responde com analise fallback para manter UX.
