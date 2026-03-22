import "dotenv/config";

function parseAllowedOrigins(value) {
  if (!value) return [];
  return value
    .split(",")
    .map((origin) => origin.trim())
    .filter(Boolean);
}

export const config = {
  nodeEnv: process.env.NODE_ENV ?? "development",
  port: Number(process.env.PORT ?? 3001),
  apiBaseUrl: process.env.API_BASE_URL ?? "http://localhost:3001",
  allowedOrigins: parseAllowedOrigins(process.env.ALLOWED_ORIGINS),
  stockfishPath: process.env.STOCKFISH_PATH ?? "",
  defaultAnalysisDepth: Number(process.env.DEFAULT_ANALYSIS_DEPTH ?? 12)
};
