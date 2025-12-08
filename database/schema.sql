-- Script SQL pour créer les tables PostgreSQL
-- Base de données : echec_pong

-- Table des parties
CREATE TABLE IF NOT EXISTS games (
    id SERIAL PRIMARY KEY,
    created_at TIMESTAMP DEFAULT NOW(),
    status VARCHAR(50) DEFAULT 'waiting',  -- 'waiting', 'playing', 'finished'
    winner_id INTEGER,
    game_data JSONB      -- État du jeu en JSON
);

-- Table des joueurs
CREATE TABLE IF NOT EXISTS players (
    id SERIAL PRIMARY KEY,
    username VARCHAR(100) NOT NULL,
    connected_at TIMESTAMP DEFAULT NOW(),
    game_id INTEGER REFERENCES games(id) ON DELETE SET NULL
);

-- Table des mouvements
CREATE TABLE IF NOT EXISTS moves (
    id SERIAL PRIMARY KEY,
    game_id INTEGER REFERENCES games(id) ON DELETE CASCADE,
    player_id INTEGER REFERENCES players(id) ON DELETE CASCADE,
    move_data JSONB,
    timestamp TIMESTAMP DEFAULT NOW()
);

-- Index pour améliorer les performances
CREATE INDEX IF NOT EXISTS idx_games_status ON games(status);
CREATE INDEX IF NOT EXISTS idx_players_game_id ON players(game_id);
CREATE INDEX IF NOT EXISTS idx_moves_game_id ON moves(game_id);
CREATE INDEX IF NOT EXISTS idx_moves_timestamp ON moves(timestamp);

