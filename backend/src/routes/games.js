import { Router } from "express";
import { z } from "zod";
import { createGame, getGameById, listGames, updateGameAnalysis } from "../services/gameStore.js";
import { analyzeFallback, analyzeWithStockfish } from "../stockfish/engine.js";
import { config } from "../config.js";
import { HttpError } from "../utils/httpError.js";

const router = Router();

const createGameSchema = z.object({
  whitePlayer: z.string().min(1),
  blackPlayer: z.string().min(1),
  difficulty: z.enum(["iniciante", "casual", "intermediario", "avancado", "pro"]),
  result: z.enum(["1-0", "0-1", "1/2-1/2", "*"]).default("*"),
  pgn: z.string().min(1),
  fen: z.string().min(1),
  moves: z.array(z.string().min(4)).default([])
});

const analyzeSchema = z.object({
  fen: z.string().min(1),
  moves: z.array(z.string().min(4)).default([]),
  depth: z.number().int().min(6).max(22).optional()
});

router.get("/games", (req, res) => {
  const limit = Math.max(1, Math.min(100, Number(req.query.limit ?? 20)));
  res.json({ items: listGames(limit) });
});

router.get("/games/:id", (req, res, next) => {
  const game = getGameById(req.params.id);
  if (!game) {
    return next(new HttpError(404, "Game not found."));
  }

  return res.json(game);
});

router.post("/games", (req, res, next) => {
  const parsed = createGameSchema.safeParse(req.body);
  if (!parsed.success) {
    return next(new HttpError(400, "Invalid payload.", parsed.error.flatten()));
  }

  const game = createGame(parsed.data);
  return res.status(201).json(game);
});

router.post("/analysis", async (req, res, next) => {
  const parsed = analyzeSchema.safeParse(req.body);
  if (!parsed.success) {
    return next(new HttpError(400, "Invalid analysis payload.", parsed.error.flatten()));
  }

  const payload = parsed.data;
  const depth = payload.depth ?? config.defaultAnalysisDepth;

  try {
    const result = await analyzeWithStockfish({
      stockfishPath: config.stockfishPath,
      fen: payload.fen,
      depth
    });

    return res.json({
      ...result,
      depth,
      analyzedAt: new Date().toISOString()
    });
  } catch (_err) {
    const fallback = analyzeFallback({ moves: payload.moves });
    return res.json({
      ...fallback,
      depth,
      analyzedAt: new Date().toISOString()
    });
  }
});

router.post("/games/:id/analysis", async (req, res, next) => {
  const game = getGameById(req.params.id);
  if (!game) {
    return next(new HttpError(404, "Game not found."));
  }

  const payload = {
    fen: game.fen,
    moves: game.moves,
    depth: config.defaultAnalysisDepth
  };

  try {
    const analysis = await analyzeWithStockfish({
      stockfishPath: config.stockfishPath,
      fen: payload.fen,
      depth: payload.depth
    });

    const updated = updateGameAnalysis(game.id, {
      ...analysis,
      depth: payload.depth,
      analyzedAt: new Date().toISOString()
    });

    return res.json(updated);
  } catch (_err) {
    const analysis = {
      ...analyzeFallback({ moves: payload.moves }),
      depth: payload.depth,
      analyzedAt: new Date().toISOString()
    };

    const updated = updateGameAnalysis(game.id, analysis);
    return res.json(updated);
  }
});

export default router;
