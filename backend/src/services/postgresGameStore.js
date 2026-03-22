import { Pool } from "pg";

function mapRow(row) {
  return {
    id: row.id,
    createdAt: row.created_at,
    updatedAt: row.updated_at,
    whitePlayer: row.white_player,
    blackPlayer: row.black_player,
    difficulty: row.difficulty,
    result: row.result,
    pgn: row.pgn,
    fen: row.fen,
    moves: row.moves ?? [],
    analysis: row.analysis ?? null
  };
}

export function createPostgresGameStore(databaseUrl) {
  const pool = new Pool({ connectionString: databaseUrl });

  async function init() {
    await pool.query(`
      create table if not exists games (
        id text primary key,
        created_at timestamptz not null,
        updated_at timestamptz not null,
        white_player text not null,
        black_player text not null,
        difficulty text not null,
        result text not null,
        pgn text not null,
        fen text not null,
        moves jsonb not null default '[]'::jsonb,
        analysis jsonb
      );
    `);

    await pool.query("create index if not exists idx_games_updated_at on games(updated_at desc);");
  }

  async function createGame(game) {
    const row = await pool.query(
      `
        insert into games (
          id, created_at, updated_at, white_player, black_player,
          difficulty, result, pgn, fen, moves, analysis
        )
        values ($1,$2,$3,$4,$5,$6,$7,$8,$9,$10::jsonb,$11::jsonb)
        returning *;
      `,
      [
        game.id,
        game.createdAt,
        game.updatedAt,
        game.whitePlayer,
        game.blackPlayer,
        game.difficulty,
        game.result,
        game.pgn,
        game.fen,
        JSON.stringify(game.moves ?? []),
        JSON.stringify(game.analysis ?? null)
      ]
    );

    return mapRow(row.rows[0]);
  }

  async function listGames(limit) {
    const rows = await pool.query("select * from games order by updated_at desc limit $1;", [limit]);
    return rows.rows.map(mapRow);
  }

  async function getGameById(id) {
    const row = await pool.query("select * from games where id = $1 limit 1;", [id]);
    if (row.rowCount === 0) return null;
    return mapRow(row.rows[0]);
  }

  async function updateGameAnalysis(id, analysis) {
    const now = new Date().toISOString();
    const row = await pool.query(
      `
        update games
        set analysis = $2::jsonb,
            updated_at = $3
        where id = $1
        returning *;
      `,
      [id, JSON.stringify(analysis), now]
    );

    if (row.rowCount === 0) return null;
    return mapRow(row.rows[0]);
  }

  async function close() {
    await pool.end();
  }

  return {
    mode: "postgres",
    init,
    createGame,
    listGames,
    getGameById,
    updateGameAnalysis,
    close
  };
}
