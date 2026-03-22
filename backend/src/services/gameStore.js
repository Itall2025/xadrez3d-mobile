import crypto from "node:crypto";

const games = new Map();

export function createGame(payload) {
  const id = crypto.randomUUID();
  const now = new Date().toISOString();

  const game = {
    id,
    createdAt: now,
    updatedAt: now,
    whitePlayer: payload.whitePlayer,
    blackPlayer: payload.blackPlayer,
    difficulty: payload.difficulty,
    result: payload.result,
    pgn: payload.pgn,
    fen: payload.fen,
    moves: payload.moves,
    analysis: payload.analysis ?? null
  };

  games.set(id, game);
  return game;
}

export function listGames(limit = 20) {
  return Array.from(games.values())
    .sort((a, b) => b.updatedAt.localeCompare(a.updatedAt))
    .slice(0, limit);
}

export function getGameById(id) {
  return games.get(id) ?? null;
}

export function updateGameAnalysis(id, analysis) {
  const game = games.get(id);
  if (!game) return null;

  game.analysis = analysis;
  game.updatedAt = new Date().toISOString();
  games.set(id, game);
  return game;
}
