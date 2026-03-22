import express from "express";
import cors from "cors";
import helmet from "helmet";
import morgan from "morgan";
import healthRoutes from "./routes/health.js";
import gameRoutes from "./routes/games.js";
import { config } from "./config.js";
import { HttpError } from "./utils/httpError.js";

const app = express();

app.use(helmet());
app.use(express.json({ limit: "1mb" }));
app.use(morgan(config.nodeEnv === "production" ? "combined" : "dev"));

app.use(
  cors({
    origin: (origin, callback) => {
      if (!origin || config.allowedOrigins.length === 0 || config.allowedOrigins.includes(origin)) {
        callback(null, true);
      } else {
        callback(new Error("Origin not allowed by CORS."));
      }
    }
  })
);

app.use("/api", healthRoutes);
app.use("/api", gameRoutes);

app.get("/", (_req, res) => {
  res.json({
    name: "Xadrez3D Mobile Backend",
    docs: "/api/health",
    uptimeSeconds: Math.round(process.uptime())
  });
});

app.use((req, _res, next) => {
  next(new HttpError(404, `Route not found: ${req.method} ${req.originalUrl}`));
});

app.use((err, _req, res, _next) => {
  const status = err instanceof HttpError ? err.status : 500;
  const payload = {
    error: {
      message: err.message ?? "Internal Server Error",
      details: err.details ?? null
    }
  };

  if (status >= 500) {
    console.error(err);
  }

  res.status(status).json(payload);
});

app.listen(config.port, () => {
  console.log(`Xadrez3D backend listening on port ${config.port}`);
});
