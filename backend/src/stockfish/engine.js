import { spawn } from "node:child_process";

function parseCpFromInfoLine(line) {
  const cpMatch = line.match(/score cp (-?\d+)/);
  if (cpMatch) return Number(cpMatch[1]);

  const mateMatch = line.match(/score mate (-?\d+)/);
  if (mateMatch) {
    const mateIn = Number(mateMatch[1]);
    return mateIn > 0 ? 10000 - mateIn * 100 : -10000 - mateIn * 100;
  }

  return null;
}

function classifyDelta(deltaCp) {
  const abs = Math.abs(deltaCp);
  if (abs <= 25) return "excellent";
  if (abs <= 75) return "good";
  if (abs <= 150) return "inaccuracy";
  if (abs <= 300) return "mistake";
  return "blunder";
}

function writePosition(sf, fen) {
  if (!fen || fen === "startpos") {
    sf.stdin.write("position startpos\n");
    return;
  }

  sf.stdin.write(`position fen ${fen}\n`);
}

export async function analyzeWithStockfish({ stockfishPath, fen, depth }) {
  if (!stockfishPath) {
    throw new Error("Stockfish path not configured.");
  }

  return new Promise((resolve, reject) => {
    const sf = spawn(stockfishPath, [], { stdio: ["pipe", "pipe", "pipe"] });

    let bestMove = null;
    let evalCp = 0;
    let principalVariation = "";

    const timeout = setTimeout(() => {
      sf.kill();
      reject(new Error("Stockfish timeout."));
    }, 8000);

    sf.stdout.on("data", (chunk) => {
      const text = chunk.toString();
      const lines = text.split(/\r?\n/);

      for (const line of lines) {
        if (!line) continue;

        if (line.startsWith("info ")) {
          const cp = parseCpFromInfoLine(line);
          if (cp !== null) evalCp = cp;

          const pvMatch = line.match(/ pv (.+)$/);
          if (pvMatch) {
            principalVariation = pvMatch[1].trim();
          }
        }

        if (line.startsWith("bestmove ")) {
          bestMove = line.split(" ")[1] ?? null;
          clearTimeout(timeout);
          sf.kill();

          resolve({
            bestMove,
            evalCp,
            principalVariation,
            moveQuality: classifyDelta(0),
            deltaCp: 0,
            source: "stockfish"
          });
        }
      }
    });

    sf.stderr.on("data", (chunk) => {
      const msg = chunk.toString().trim();
      if (msg) {
        clearTimeout(timeout);
        sf.kill();
        reject(new Error(`Stockfish error: ${msg}`));
      }
    });

    sf.on("error", (err) => {
      clearTimeout(timeout);
      reject(err);
    });

    sf.stdin.write("uci\n");
    sf.stdin.write("isready\n");
    writePosition(sf, fen);
    sf.stdin.write(`go depth ${depth}\n`);
  });
}

export function analyzeFallback({ moves }) {
  const accuracy = Math.max(52, 92 - Math.min(moves.length, 60) * 0.3);

  return {
    bestMove: moves.at(-1) ?? null,
    evalCp: 0,
    principalVariation: "",
    moveQuality: "good",
    deltaCp: 0,
    source: "fallback",
    summary: {
      whiteAccuracy: Math.round(accuracy),
      blackAccuracy: Math.round(accuracy + 2),
      blunders: 0,
      mistakes: 0,
      inaccuracies: Math.max(0, Math.floor(moves.length / 12))
    }
  };
}
