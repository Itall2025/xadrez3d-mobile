import { Router } from "express";

const router = Router();

router.get("/health", (_req, res) => {
  res.json({
    ok: true,
    service: "xadrez3d-mobile-backend",
    timestamp: new Date().toISOString()
  });
});

export default router;
