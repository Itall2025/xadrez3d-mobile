# Deploy no Render

## Estrategia
- Unity app no mobile
- Backend API no Render
- Analise de partidas via endpoint `/api/analysis`

## Passos
1. Subir este repositorio no GitHub
2. No Render, criar Web Service via `render.yaml`
3. Configurar variaveis de ambiente:
   - `ALLOWED_ORIGINS` com dominio do app/site
   - `STOCKFISH_PATH` (quando binario estiver disponivel)
4. Validar saude em `/api/health`

## Endpoints principais
- `GET /api/health`
- `POST /api/games`
- `GET /api/games`
- `GET /api/games/:id`
- `POST /api/analysis`
- `POST /api/games/:id/analysis`

## Observacao importante
Se `STOCKFISH_PATH` nao estiver configurado, a API responde com analise fallback para manter UX. Depois ativamos analise real no backend com Stockfish.
