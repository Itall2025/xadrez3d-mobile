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

create index if not exists idx_games_updated_at on games(updated_at desc);
