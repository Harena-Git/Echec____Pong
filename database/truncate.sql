-- Désactiver temporairement les contraintes FK
SET session_replication_role = replica;

TRUNCATE TABLE
    preferences_joueur,
    statistiques_defense,
    collision_precise,
    collision_balle_piece,
    coup_pingpong,
    balle,
    piece_echecs,
    match_hybride,
    joueurs,
    terrains,
    configuration_pieces
RESTART IDENTITY CASCADE;

-- Réactiver les contraintes FK
SET session_replication_role = DEFAULT;
