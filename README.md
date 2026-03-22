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

## Setup rapido do backend
```bash
cd backend
cp .env.example .env
npm install
npm run dev
```

API local: `http://localhost:3001`
Saude: `http://localhost:3001/api/health`

## Proximos passos imediatos
1. Conectar Unity ao endpoint `/api/analysis`
2. Persistencia em banco (PostgreSQL no Render)
3. Integrar Stockfish no servidor com binario dedicado
4. Dashboard de analise por partida (acuracia, blunders, melhores lances)
