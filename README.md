# Xadrez3D Mobile

Projeto de xadrez 3D mobile (Android/iOS), com foco em visual premium, IA com varios niveis e analise pos-partida em nivel profissional.

## Frentes do projeto
- `src-unity/`: cliente 3D em Unity
- `backend/`: API para Render (partidas, historico, analise)
- `docs/`: arquitetura, setup e deploy

## Backend (Render)
- Runtime Node.js
- Endpoints REST para partida e analise
- Integracao com Stockfish via UCI (com fallback seguro)
- Persistencia em PostgreSQL por `DATABASE_URL`

## Setup rapido do backend
```bash
cd backend
cp .env.example .env
npm install
npm run dev
```

API local: `http://localhost:3001`
Saude: `http://localhost:3001/api/health`

## Integracao Unity
`GameController` usa `BackendChessEngine` para buscar melhor lance em `/api/analysis`.
Cena jogavel inicial: `docs/UNITY_PLAYABLE_SCENE.md`.

## Proximos passos imediatos
1. Gerar FEN real no cliente Unity
2. Enviar historico de lances UCI no payload de analise
3. Integrar dashboard de analise por partida (acuracia, blunders, melhores lances)
