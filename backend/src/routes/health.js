import { Router } from "express";

export function createHealthRoutes({ gameStore }) {
  const router = Router();

  router.get("/health", (_req, res) => {
    res.json({
      ok: true,
      service: "xadrez3d-mobile-backend",
      storageMode: gameStore.mode,
      timestamp: new Date().toISOString()
    });
  });

  return router;
}
