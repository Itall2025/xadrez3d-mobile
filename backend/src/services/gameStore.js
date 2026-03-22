import crypto from "node:crypto";
import { createPostgresGameStore } from "./postgresGameStore.js";

function createMemoryGameStore() {
  const games = new Map();

  return {
    mode: "memory",
    async init() {},
    async close() {},
    async createGame(payload) {
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
    },
    async listGames(limit = 20) {
      return Array.from(games.values())
        .sort((a, b) => b.updatedAt.localeCompare(a.updatedAt))
        .slice(0, limit);
    },
    async getGameById(id) {
      return games.get(id) ?? null;
    },
    async updateGameAnalysis(id, analysis) {
      const game = games.get(id);
      if (!game) return null;

      game.analysis = analysis;
      game.updatedAt = new Date().toISOString();
      games.set(id, game);
      return game;
    }
  };
}

export async function buildGameStore(config) {
  if (!config.databaseUrl) {
    const memoryStore = createMemoryGameStore();
    await memoryStore.init();
    return memoryStore;
  }

  const pgStore = createPostgresGameStore(config.databaseUrl);

  try {
    await pgStore.init();
    return pgStore;
  } catch (err) {
    console.error("Postgres unavailable, fallback to memory store.", err);
    await pgStore.close();

    const memoryStore = createMemoryGameStore();
    await memoryStore.init();
    return memoryStore;
  }
}
