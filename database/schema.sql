-- ============================================
-- BASE DE DONNEES COMPLETE - ECHEC-PONG HYBRIDE
-- Version parallele corrigee (sans accents)
-- ============================================

-- Supprimer les tables existantes (si besoin)
DROP TABLE IF EXISTS preferences_joueur CASCADE;
DROP TABLE IF EXISTS statistiques_defense CASCADE;
DROP TABLE IF EXISTS collision_precise CASCADE;
DROP TABLE IF EXISTS collision_balle_piece CASCADE;
DROP TABLE IF EXISTS coup_pingpong CASCADE;
DROP TABLE IF EXISTS piece_echecs CASCADE;
DROP TABLE IF EXISTS balle CASCADE;
DROP TABLE IF EXISTS match_hybride CASCADE;
DROP TABLE IF EXISTS joueurs CASCADE;
DROP TABLE IF EXISTS terrains CASCADE;
DROP TABLE IF EXISTS configuration_pieces CASCADE;

-- ==================== TABLES PRINCIPALES ====================

-- Table des terrains
CREATE TABLE terrains (
    id_terrain SERIAL PRIMARY KEY,
    nom_terrain VARCHAR(100) NOT NULL,
    largeur_pingpong FLOAT NOT NULL DEFAULT 2.74,
    longueur_pingpong FLOAT NOT NULL DEFAULT 1.525,
    largeur_zone_echecs FLOAT NOT NULL DEFAULT 2.74,
    profondeur_zone_echecs FLOAT NOT NULL DEFAULT 1.0,
    nombre_colonnes_pions INTEGER NOT NULL DEFAULT 8 CHECK (nombre_colonnes_pions BETWEEN 1 AND 12),
    nombre_rangees_pions INTEGER NOT NULL DEFAULT 2 CHECK (nombre_rangees_pions BETWEEN 1 AND 4),
    couleur_surface VARCHAR(20) DEFAULT 'vert',
    date_creation TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Table des joueurs
CREATE TABLE joueurs (
    id_joueur SERIAL PRIMARY KEY,
    pseudo VARCHAR(50) NOT NULL UNIQUE,
    date_inscription TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    classement INTEGER DEFAULT 1000,
    parties_jouees INTEGER DEFAULT 0,
    parties_gagnees INTEGER DEFAULT 0,
    total_points INTEGER DEFAULT 0,
    
    -- NOUVEAUX CHAMPS POUR CONCEPTION PARALLELE
    position_x_defaut FLOAT DEFAULT 0.5 CHECK (position_x_defaut BETWEEN 0 AND 1),
    vitesse_deplacement FLOAT DEFAULT 0.1 CHECK (vitesse_deplacement > 0),
    precision_moyenne FLOAT DEFAULT 0.0 CHECK (precision_moyenne BETWEEN 0 AND 1),
    
    -- Statistiques avancees
    tirs_tentes INTEGER DEFAULT 0,
    tirs_reussis INTEGER DEFAULT 0,
    defenses_tentees INTEGER DEFAULT 0,
    defenses_reussies INTEGER DEFAULT 0,
    rois_proteges INTEGER DEFAULT 0,
    rois_perdus INTEGER DEFAULT 0
);

-- Table des matchs
CREATE TABLE match_hybride (
    id_match SERIAL PRIMARY KEY,
    id_terrain INTEGER REFERENCES terrains(id_terrain),
    id_joueur_nord INTEGER REFERENCES joueurs(id_joueur),
    id_joueur_sud INTEGER REFERENCES joueurs(id_joueur),
    
    -- Scores ping-pong
    score_joueur_nord INTEGER DEFAULT 0 CHECK (score_joueur_nord >= 0),
    score_joueur_sud INTEGER DEFAULT 0 CHECK (score_joueur_sud >= 0),
    points_pour_gagner INTEGER DEFAULT 11 CHECK (points_pour_gagner > 0),
    
    -- Etat des pieces
    roi_nord_vivant BOOLEAN DEFAULT TRUE,
    roi_sud_vivant BOOLEAN DEFAULT TRUE,
    nombre_pions_nord INTEGER DEFAULT 16 CHECK (nombre_pions_nord BETWEEN 0 AND 16),
    nombre_pions_sud INTEGER DEFAULT 16 CHECK (nombre_pions_sud BETWEEN 0 AND 16),
    
    -- Informations match
    statut VARCHAR(20) DEFAULT 'en_attente' CHECK (statut IN ('en_attente', 'en_cours', 'termine', 'abandonne')),
    tour_actuel VARCHAR(20) DEFAULT 'pingpong' CHECK (tour_actuel IN ('pingpong', 'deplacement_pions')),
    joueur_au_service INTEGER REFERENCES joueurs(id_joueur),
    
    -- NOUVEAUX CHAMPS POUR CONCEPTION PARALLELE
    colonne_service INTEGER CHECK (colonne_service BETWEEN 0 AND 7),
    dernier_tir_colonne INTEGER CHECK (dernier_tir_colonne BETWEEN 0 AND 7),
    nombre_defenses INTEGER DEFAULT 0 CHECK (nombre_defenses >= 0),
    
    -- Conditions de victoire
    vainqueur INTEGER REFERENCES joueurs(id_joueur),
    raison_victoire VARCHAR(30) CHECK (raison_victoire IN (
        'roi_capture', 'score_pingpong', 'abandon', 'temps_ecoule', 'pions_elimines'
    )),
    
    -- Timestamps
    date_creation TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    date_debut TIMESTAMP,
    date_fin TIMESTAMP,
    duree_match INTERVAL,
    
    -- Contraintes supplementaires
    CHECK (id_joueur_nord != id_joueur_sud),
    CHECK (date_debut IS NULL OR date_debut >= date_creation),
    CHECK (date_fin IS NULL OR date_fin >= date_debut)
);

-- Table des pieces d'echecs
CREATE TABLE piece_echecs (
    id_piece SERIAL PRIMARY KEY,
    id_match INTEGER REFERENCES match_hybride(id_match) ON DELETE CASCADE,
    id_joueur INTEGER REFERENCES joueurs(id_joueur),
    
    -- Type de piece
    type_piece VARCHAR(20) NOT NULL CHECK (type_piece IN (
        'roi', 'reine', 'tour', 'fou', 'cavalier', 'pion'
    )),
    
    -- Position sur l'echiquier
    colonne INTEGER NOT NULL CHECK (colonne BETWEEN 0 AND 7),
    rangee INTEGER NOT NULL CHECK (rangee BETWEEN 0 AND 1),
    
    -- Caracteristiques
    nombre_vies INTEGER NOT NULL DEFAULT 1 CHECK (nombre_vies BETWEEN 1 AND 5),
    vies_restantes INTEGER NOT NULL CHECK (vies_restantes >= 0 AND vies_restantes <= nombre_vies),
    valeur INTEGER NOT NULL CHECK (valeur > 0),
    
    -- Statut
    statut VARCHAR(20) DEFAULT 'vivant' CHECK (statut IN ('vivant', 'blesse', 'mort', 'protege')),
    
    -- NOUVEAUX CHAMPS POUR CONCEPTION PARALLELE
    colonne_protegee BOOLEAN DEFAULT FALSE,
    derniere_colonne_touchee INTEGER CHECK (derniere_colonne_touchee BETWEEN 0 AND 7),
    precision_position FLOAT DEFAULT 1.0 CHECK (precision_position BETWEEN 0 AND 1),
    
    -- Timestamps
    date_creation TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    date_mort TIMESTAMP,
    
    -- Contraintes
    CHECK (vies_restantes <= nombre_vies),
    UNIQUE(id_match, id_joueur, colonne, rangee)
);

-- Table des balles
CREATE TABLE balle (
    id_balle SERIAL PRIMARY KEY,
    id_match INTEGER REFERENCES match_hybride(id_match) ON DELETE CASCADE,
    
    -- Position
    position_x FLOAT NOT NULL CHECK (position_x BETWEEN 0 AND 1),
    position_y FLOAT NOT NULL CHECK (position_y >= 0),
    position_z FLOAT DEFAULT 0,
    
    -- Vitesse
    vitesse_x FLOAT DEFAULT 0,
    vitesse_y FLOAT DEFAULT 0,
    vitesse_z FLOAT DEFAULT 0,
    
    -- Proprietes
    etat VARCHAR(20) DEFAULT 'en_jeu' CHECK (etat IN ('en_jeu', 'hors_jeu', 'collision', 'perdue', 'service')),
    dernier_touche_par INTEGER REFERENCES joueurs(id_joueur),
    force_impact FLOAT DEFAULT 1.0 CHECK (force_impact > 0),
    
    -- NOUVEAUX CHAMPS POUR CONCEPTION PARALLELE
    colonne_sortie_predite INTEGER CHECK (colonne_sortie_predite BETWEEN 0 AND 7),
    angle_tir FLOAT CHECK (angle_tir BETWEEN -90 AND 90),
    puissance_tir FLOAT DEFAULT 1.0 CHECK (puissance_tir BETWEEN 0.1 AND 5.0),
    
    -- Timestamps
    date_creation TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    date_dernier_rebond TIMESTAMP
);

-- ==================== TABLES D'EVENEMENTS ====================

-- Table des coups de ping-pong
CREATE TABLE coup_pingpong (
    id_coup SERIAL PRIMARY KEY,
    id_match INTEGER REFERENCES match_hybride(id_match) ON DELETE CASCADE,
    id_joueur INTEGER REFERENCES joueurs(id_joueur),
    id_balle INTEGER REFERENCES balle(id_balle),
    
    -- Type de coup
    type_coup VARCHAR(30) CHECK (type_coup IN (
        'service', 'drive', 'topspin', 'contre', 'lob', 'amorti', 'smash'
    )),
    
    -- Caracteristiques
    position_frappe_x FLOAT CHECK (position_frappe_x BETWEEN 0 AND 1),
    position_frappe_y FLOAT,
    puissance FLOAT DEFAULT 1.0 CHECK (puissance BETWEEN 0.1 AND 5.0),
    precision FLOAT DEFAULT 1.0 CHECK (precision BETWEEN 0 AND 1),
    angle FLOAT,
    
    -- Resultat
    reussi BOOLEAN DEFAULT TRUE,
    resultat VARCHAR(30) CHECK (resultat IN (
        'bon', 'faute', 'filet', 'hors_table', 'collision_piece', 'defendu'
    )),
    
    -- NOUVEAUX CHAMPS POUR CONCEPTION PARALLELE
    colonne_visee INTEGER CHECK (colonne_visee BETWEEN 0 AND 7),
    colonne_atteinte INTEGER CHECK (colonne_atteinte BETWEEN 0 AND 7),
    precision_tir FLOAT CHECK (precision_tir BETWEEN 0 AND 1),
    defense_reussie BOOLEAN DEFAULT FALSE,
    
    -- Timestamps
    date_coup TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    -- Contraintes
    CHECK (colonne_atteinte IS NULL OR colonne_visee IS NOT NULL)
);

-- Table des collisions balle-piece (ancienne version)
CREATE TABLE collision_balle_piece (
    id_collision SERIAL PRIMARY KEY,
    id_match INTEGER REFERENCES match_hybride(id_match) ON DELETE CASCADE,
    id_balle INTEGER REFERENCES balle(id_balle),
    id_piece INTEGER REFERENCES piece_echecs(id_piece),
    id_joueur_proprietaire INTEGER REFERENCES joueurs(id_joueur),
    
    -- Details collision
    point_impact_x FLOAT,
    point_impact_y FLOAT,
    force_impact FLOAT,
    degats_infliges INTEGER DEFAULT 1 CHECK (degats_infliges > 0),
    
    -- Etat apres collision
    vies_piece_avant INTEGER,
    vies_piece_apres INTEGER,
    piece_eliminee BOOLEAN DEFAULT FALSE,
    roi_touche BOOLEAN DEFAULT FALSE,
    
    -- Timestamps
    date_collision TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- NOUVELLE TABLE : Collisions precises (pour conception parallele)
CREATE TABLE collision_precise (
    id_collision SERIAL PRIMARY KEY,
    id_match INTEGER REFERENCES match_hybride(id_match) ON DELETE CASCADE,
    id_balle INTEGER REFERENCES balle(id_balle),
    id_piece INTEGER REFERENCES piece_echecs(id_piece),
    
    -- Position exacte
    colonne_touchee INTEGER NOT NULL CHECK (colonne_touchee BETWEEN 0 AND 7),
    rangee_touchee INTEGER NOT NULL CHECK (rangee_touchee BETWEEN 0 AND 1),
    
    -- Precision
    precision_tir FLOAT CHECK (precision_tir BETWEEN 0 AND 1),
    
    -- Defense
    defense_tentee BOOLEAN DEFAULT FALSE,
    defense_reussie BOOLEAN DEFAULT FALSE,
    
    -- Degats
    degats_infliges INTEGER DEFAULT 1 CHECK (degats_infliges > 0),
    piece_eliminee BOOLEAN DEFAULT FALSE,
    
    -- Timestamps
    moment_impact TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    -- Contraintes
    CHECK (NOT (defense_reussie AND piece_eliminee))
);

-- ==================== TABLES DE CONFIGURATION ====================

-- Configuration des pieces
CREATE TABLE configuration_pieces (
    id_config SERIAL PRIMARY KEY,
    nom_config VARCHAR(50) NOT NULL,
    description TEXT,
    
    -- Vies par type
    vies_roi INTEGER DEFAULT 3 CHECK (vies_roi > 0),
    vies_reine INTEGER DEFAULT 2 CHECK (vies_reine > 0),
    vies_tour INTEGER DEFAULT 2 CHECK (vies_tour > 0),
    vies_fou INTEGER DEFAULT 1 CHECK (vies_fou > 0),
    vies_cavalier INTEGER DEFAULT 1 CHECK (vies_cavalier > 0),
    vies_pion INTEGER DEFAULT 1 CHECK (vies_pion > 0),
    
    -- JSON pour configuration avancee
    regles_json JSONB,
    
    date_creation TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    actif BOOLEAN DEFAULT TRUE
);

-- NOUVELLE TABLE : Statistiques de defense
CREATE TABLE statistiques_defense (
    id_statistique SERIAL PRIMARY KEY,
    id_match INTEGER REFERENCES match_hybride(id_match) ON DELETE CASCADE,
    id_joueur INTEGER REFERENCES joueurs(id_joueur),
    
    -- Colonne specifique
    colonne_defendue INTEGER NOT NULL CHECK (colonne_defendue BETWEEN 0 AND 7),
    
    -- Statistiques
    nombre_defenses INTEGER DEFAULT 0 CHECK (nombre_defenses >= 0),
    nombre_tirs_subis INTEGER DEFAULT 0 CHECK (nombre_tirs_subis >= 0),
    taux_reussite FLOAT DEFAULT 0.0 CHECK (taux_reussite BETWEEN 0 AND 1),
    
    date_maj TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    -- Contraintes
    UNIQUE(id_match, id_joueur, colonne_defendue),
    CHECK (nombre_defenses <= nombre_tirs_subis)
);

-- NOUVELLE TABLE : Preferences joueur
CREATE TABLE preferences_joueur (
    id_preference SERIAL PRIMARY KEY,
    id_joueur INTEGER REFERENCES joueurs(id_joueur) ON DELETE CASCADE UNIQUE,
    
    -- Preferences de jeu
    colonne_preferee INTEGER DEFAULT 4 CHECK (colonne_preferee BETWEEN 0 AND 7),
    strategie_defense VARCHAR(20) DEFAULT 'mixte' CHECK (strategie_defense IN ('agressive', 'defensive', 'mixte')),
    sensibilite_controle FLOAT DEFAULT 1.0 CHECK (sensibilite_controle BETWEEN 0.1 AND 3.0),
    
    -- Preferences d'affichage
    afficher_predictions BOOLEAN DEFAULT TRUE,
    couleur_raquette VARCHAR(20) DEFAULT 'blanc',
    theme_interface VARCHAR(20) DEFAULT 'classique',
    
    -- Preferences audio
    volume_effets FLOAT DEFAULT 0.8 CHECK (volume_effets BETWEEN 0 AND 1),
    volume_musique FLOAT DEFAULT 0.5 CHECK (volume_musique BETWEEN 0 AND 1),
    
    date_creation TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    date_maj TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ==================== INDEX POUR PERFORMANCE ====================

-- Index pour les recherches frequentes
CREATE INDEX idx_match_statut ON match_hybride(statut);
CREATE INDEX idx_match_joueurs ON match_hybride(id_joueur_nord, id_joueur_sud);
CREATE INDEX idx_match_date ON match_hybride(date_creation, date_debut);

-- Index pour les pieces
CREATE INDEX idx_piece_match ON piece_echecs(id_match, id_joueur);
CREATE INDEX idx_piece_position ON piece_echecs(colonne, rangee);
CREATE INDEX idx_piece_statut ON piece_echecs(statut, vies_restantes);
CREATE INDEX idx_piece_colonne_protegee ON piece_echecs(colonne_protegee);
CREATE INDEX idx_piece_type ON piece_echecs(type_piece, statut);

-- Index pour les balles
CREATE INDEX idx_balle_match ON balle(id_match);
CREATE INDEX idx_balle_etat ON balle(etat);
CREATE INDEX idx_balle_colonne_predite ON balle(colonne_sortie_predite);

-- Index pour les coups
CREATE INDEX idx_coup_match ON coup_pingpong(id_match);
CREATE INDEX idx_coup_joueur ON coup_pingpong(id_joueur, date_coup);
CREATE INDEX idx_coup_colonne_visee ON coup_pingpong(colonne_visee, colonne_atteinte);

-- Index pour les collisions
CREATE INDEX idx_collision_match ON collision_precise(id_match);
CREATE INDEX idx_collision_colonne ON collision_precise(colonne_touchee, rangee_touchee);
CREATE INDEX idx_collision_piece ON collision_precise(id_piece);

-- Index pour les defenses
CREATE INDEX idx_defense_match_joueur ON statistiques_defense(id_match, id_joueur);
CREATE INDEX idx_defense_colonne ON statistiques_defense(colonne_defendue, taux_reussite);

-- Index pour les joueurs
CREATE INDEX idx_joueur_pseudo ON joueurs(pseudo);
CREATE INDEX idx_joueur_classement ON joueurs(classement);

-- ==================== DONNEES INITIALES ====================

-- Inserer un terrain par defaut
INSERT INTO terrains (
    nom_terrain, largeur_pingpong, longueur_pingpong,
    largeur_zone_echecs, profondeur_zone_echecs,
    nombre_colonnes_pions, nombre_rangees_pions, couleur_surface
) VALUES (
    'Arena Parallele', 2.74, 1.525, 2.74, 1.0, 8, 2, 'bleu'
);

-- Inserer la configuration par defaut des pieces
INSERT INTO configuration_pieces (
    nom_config, description, vies_roi, vies_reine,
    vies_tour, vies_fou, vies_cavalier, vies_pion,
    regles_json, actif
) VALUES (
    'Parallele Classique',
    'Configuration pour la conception parallele avec defense par colonne',
    3, 2, 2, 1, 1, 1,
    '{
        "regle_avantage": true,
        "points_par_set": 11,
        "sets_pour_gagner": 3,
        "defense_colonne": true,
        "prediction_trajectoire": true,
        "precision_requise": 0.6
    }'::jsonb,
    true
);

-- Inserer des joueurs de test
INSERT INTO joueurs (pseudo, classement, position_x_defaut, precision_moyenne) VALUES 
('AliceNord', 1200, 0.5, 0.85),
('BobSud', 1100, 0.5, 0.78),
('Charlie', 1050, 0.3, 0.72),
('Diana', 1150, 0.7, 0.81),
('Eve', 1000, 0.5, 0.65),
('TesteurParallele', 1000, 0.5, 0.5);

-- Inserer des preferences pour les joueurs de test
INSERT INTO preferences_joueur (id_joueur, colonne_preferee, strategie_defense) VALUES
(1, 4, 'agressive'),
(2, 4, 'defensive'),
(3, 2, 'mixte'),
(4, 6, 'agressive'),
(5, 3, 'defensive'),
(6, 4, 'mixte');

-- ==================== VUES UTILES ====================

-- Vue pour les matchs en cours avec informations paralleles
CREATE VIEW vue_matchs_paralleles AS
SELECT 
    m.id_match,
    jn.pseudo as joueur_nord,
    js.pseudo as joueur_sud,
    m.score_joueur_nord,
    m.score_joueur_sud,
    m.nombre_pions_nord,
    m.nombre_pions_sud,
    m.dernier_tir_colonne,
    m.nombre_defenses,
    m.date_debut,
    EXTRACT(EPOCH FROM (CURRENT_TIMESTAMP - m.date_debut)) as duree_secondes
FROM match_hybride m
JOIN joueurs jn ON m.id_joueur_nord = jn.id_joueur
JOIN joueurs js ON m.id_joueur_sud = js.id_joueur
WHERE m.statut = 'en_cours';

-- Vue pour les statistiques de precision des joueurs
CREATE VIEW vue_statistiques_precision AS
SELECT 
    j.id_joueur,
    j.pseudo,
    j.classement,
    j.precision_moyenne,
    COUNT(DISTINCT m.id_match) as matchs_joues,
    COUNT(DISTINCT CASE WHEN m.vainqueur = j.id_joueur THEN m.id_match END) as matchs_gagnes,
    COUNT(cp.id_coup) as tirs_tentes,
    COUNT(CASE WHEN cp.reussi THEN 1 END) as tirs_reussis,
    ROUND((AVG(cp.precision_tir)::NUMERIC * 100)::NUMERIC, 2) as precision_tir_moyenne,
    ROUND((AVG(CASE WHEN cp.colonne_visee = cp.colonne_atteinte THEN 1.0 ELSE 0.0 END)::NUMERIC * 100)::NUMERIC, 2) as precision_colonne
FROM joueurs j
LEFT JOIN match_hybride m ON j.id_joueur IN (m.id_joueur_nord, m.id_joueur_sud)
LEFT JOIN coup_pingpong cp ON j.id_joueur = cp.id_joueur AND m.id_match = cp.id_match
GROUP BY j.id_joueur, j.pseudo, j.classement, j.precision_moyenne
ORDER BY precision_tir_moyenne DESC NULLS LAST;

-- Vue pour les defenses par colonne
CREATE VIEW vue_defenses_colonnes AS
SELECT 
    j.pseudo,
    sd.colonne_defendue,
    sd.nombre_defenses,
    sd.nombre_tirs_subis,
    ROUND((sd.taux_reussite::NUMERIC * 100)::NUMERIC, 2) as taux_reussite_pourcent,
    CASE 
        WHEN sd.taux_reussite >= 0.8 THEN 'Excellent'
        WHEN sd.taux_reussite >= 0.6 THEN 'Bon'
        WHEN sd.taux_reussite >= 0.4 THEN 'Moyen'
        ELSE 'Faible'
    END as niveau_defense
FROM statistiques_defense sd
JOIN joueurs j ON sd.id_joueur = j.id_joueur
WHERE sd.nombre_tirs_subis >= 5
ORDER BY sd.taux_reussite DESC;

-- Vue pour les pieces vulnerables (colonne non protegee)
CREATE VIEW vue_pieces_vulnerables AS
SELECT 
    m.id_match,
    j.pseudo,
    p.type_piece,
    p.colonne,
    p.rangee,
    p.vies_restantes,
    p.colonne_protegee,
    CASE 
        WHEN p.colonne_protegee THEN 'PROTEGEE'
        WHEN p.rangee = 0 THEN 'AVANT - VULNERABLE'
        ELSE 'ARRIERE'
    END as niveau_vulnerabilite
FROM piece_echecs p
JOIN joueurs j ON p.id_joueur = j.id_joueur
JOIN match_hybride m ON p.id_match = m.id_match
WHERE p.statut = 'vivant' 
AND m.statut = 'en_cours'
ORDER BY p.vies_restantes, p.colonne_protegee;

-- ==================== FONCTIONS ====================

-- Fonction pour initialiser les pieces avec colonnes paralleles
CREATE OR REPLACE FUNCTION initialiser_pieces_paralleles(
    p_id_match INTEGER,
    p_id_joueur_nord INTEGER,
    p_id_joueur_sud INTEGER
) RETURNS VOID AS $$
DECLARE
    v_config_id INTEGER;
    v_vies_roi INTEGER;
    v_vies_reine INTEGER;
    v_vies_tour INTEGER;
    v_vies_fou INTEGER;
    v_vies_cavalier INTEGER;
    v_vies_pion INTEGER;
BEGIN
    -- Recuperer la configuration active
    SELECT id_config, vies_roi, vies_reine, vies_tour, vies_fou, vies_cavalier, vies_pion
    INTO v_config_id, v_vies_roi, v_vies_reine, v_vies_tour, v_vies_fou, v_vies_cavalier, v_vies_pion
    FROM configuration_pieces
    WHERE actif = TRUE
    ORDER BY id_config DESC
    LIMIT 1;
    
    -- Pieces pour le joueur Nord (rangee 1 = arriere, 0 = avant)
    INSERT INTO piece_echecs (id_match, id_joueur, type_piece, colonne, rangee, nombre_vies, vies_restantes, valeur)
    VALUES 
    -- Rangee arriere (rangee 1)
    (p_id_match, p_id_joueur_nord, 'tour', 0, 1, v_vies_tour, v_vies_tour, 5),
    (p_id_match, p_id_joueur_nord, 'cavalier', 1, 1, v_vies_cavalier, v_vies_cavalier, 3),
    (p_id_match, p_id_joueur_nord, 'fou', 2, 1, v_vies_fou, v_vies_fou, 3),
    (p_id_match, p_id_joueur_nord, 'reine', 3, 1, v_vies_reine, v_vies_reine, 9),
    (p_id_match, p_id_joueur_nord, 'roi', 4, 1, v_vies_roi, v_vies_roi, 100),
    (p_id_match, p_id_joueur_nord, 'fou', 5, 1, v_vies_fou, v_vies_fou, 3),
    (p_id_match, p_id_joueur_nord, 'cavalier', 6, 1, v_vies_cavalier, v_vies_cavalier, 3),
    (p_id_match, p_id_joueur_nord, 'tour', 7, 1, v_vies_tour, v_vies_tour, 5),
    -- Pions (rangee 0 - avant)
    (p_id_match, p_id_joueur_nord, 'pion', 0, 0, v_vies_pion, v_vies_pion, 1),
    (p_id_match, p_id_joueur_nord, 'pion', 1, 0, v_vies_pion, v_vies_pion, 1),
    (p_id_match, p_id_joueur_nord, 'pion', 2, 0, v_vies_pion, v_vies_pion, 1),
    (p_id_match, p_id_joueur_nord, 'pion', 3, 0, v_vies_pion, v_vies_pion, 1),
    (p_id_match, p_id_joueur_nord, 'pion', 4, 0, v_vies_pion, v_vies_pion, 1),
    (p_id_match, p_id_joueur_nord, 'pion', 5, 0, v_vies_pion, v_vies_pion, 1),
    (p_id_match, p_id_joueur_nord, 'pion', 6, 0, v_vies_pion, v_vies_pion, 1),
    (p_id_match, p_id_joueur_nord, 'pion', 7, 0, v_vies_pion, v_vies_pion, 1);
    
    -- Pieces pour le joueur Sud (rangee 0 = avant, 1 = arriere)
    INSERT INTO piece_echecs (id_match, id_joueur, type_piece, colonne, rangee, nombre_vies, vies_restantes, valeur)
    VALUES 
    -- Pions (rangee 0 - avant)
    (p_id_match, p_id_joueur_sud, 'pion', 0, 0, v_vies_pion, v_vies_pion, 1),
    (p_id_match, p_id_joueur_sud, 'pion', 1, 0, v_vies_pion, v_vies_pion, 1),
    (p_id_match, p_id_joueur_sud, 'pion', 2, 0, v_vies_pion, v_vies_pion, 1),
    (p_id_match, p_id_joueur_sud, 'pion', 3, 0, v_vies_pion, v_vies_pion, 1),
    (p_id_match, p_id_joueur_sud, 'pion', 4, 0, v_vies_pion, v_vies_pion, 1),
    (p_id_match, p_id_joueur_sud, 'pion', 5, 0, v_vies_pion, v_vies_pion, 1),
    (p_id_match, p_id_joueur_sud, 'pion', 6, 0, v_vies_pion, v_vies_pion, 1),
    (p_id_match, p_id_joueur_sud, 'pion', 7, 0, v_vies_pion, v_vies_pion, 1),
    -- Rangee arriere (rangee 1)
    (p_id_match, p_id_joueur_sud, 'tour', 0, 1, v_vies_tour, v_vies_tour, 5),
    (p_id_match, p_id_joueur_sud, 'cavalier', 1, 1, v_vies_cavalier, v_vies_cavalier, 3),
    (p_id_match, p_id_joueur_sud, 'fou', 2, 1, v_vies_fou, v_vies_fou, 3),
    (p_id_match, p_id_joueur_sud, 'reine', 3, 1, v_vies_reine, v_vies_reine, 9),
    (p_id_match, p_id_joueur_sud, 'roi', 4, 1, v_vies_roi, v_vies_roi, 100),
    (p_id_match, p_id_joueur_sud, 'fou', 5, 1, v_vies_fou, v_vies_fou, 3),
    (p_id_match, p_id_joueur_sud, 'cavalier', 6, 1, v_vies_cavalier, v_vies_cavalier, 3),
    (p_id_match, p_id_joueur_sud, 'tour', 7, 1, v_vies_tour, v_vies_tour, 5);
END;
$$ LANGUAGE plpgsql;

-- Fonction pour verifier si une colonne est defendue
CREATE OR REPLACE FUNCTION colonne_est_defendue(
    p_id_joueur INTEGER,
    p_id_match INTEGER,
    p_colonne INTEGER
) RETURNS BOOLEAN AS $$
DECLARE
    v_position_joueur FLOAT;
    v_colonne_joueur INTEGER;
BEGIN
    -- Recuperer la position du joueur dans le match
    -- Note: Cette fonction necessite une table temporaire ou une jointure
    -- Pour simplifier, on retourne FALSE par defaut
    RETURN FALSE;
END;
$$ LANGUAGE plpgsql;

-- Fonction pour enregistrer une collision precise
CREATE OR REPLACE FUNCTION enregistrer_collision_precise(
    p_id_match INTEGER,
    p_id_balle INTEGER,
    p_id_piece INTEGER,
    p_colonne INTEGER,
    p_rangee INTEGER,
    p_precision_tir FLOAT,
    p_defense_tentee BOOLEAN,
    p_defense_reussie BOOLEAN,
    p_degats INTEGER
) RETURNS INTEGER AS $$
DECLARE
    v_id_collision INTEGER;
    v_piece_eliminee BOOLEAN;
BEGIN
    -- Verifier si la piece est eliminee
    SELECT (vies_restantes - p_degats) <= 0 INTO v_piece_eliminee
    FROM piece_echecs
    WHERE id_piece = p_id_piece;
    
    -- Inserer la collision
    INSERT INTO collision_precise (
        id_match, id_balle, id_piece,
        colonne_touchee, rangee_touchee,
        precision_tir, defense_tentee, defense_reussie,
        degats_infliges, piece_eliminee
    ) VALUES (
        p_id_match, p_id_balle, p_id_piece,
        p_colonne, p_rangee,
        p_precision_tir, p_defense_tentee, p_defense_reussie,
        p_degats, v_piece_eliminee
    ) RETURNING id_collision INTO v_id_collision;
    
    -- Mettre a jour la piece
    UPDATE piece_echecs
    SET vies_restantes = GREATEST(0, vies_restantes - p_degats),
        derniere_colonne_touchee = p_colonne,
        statut = CASE 
            WHEN vies_restantes - p_degats <= 0 THEN 'mort'
            WHEN vies_restantes - p_degats = 1 THEN 'blesse'
            ELSE statut
        END,
        date_mort = CASE 
            WHEN vies_restantes - p_degats <= 0 THEN CURRENT_TIMESTAMP
            ELSE date_mort
        END
    WHERE id_piece = p_id_piece;
    
    -- Mettre a jour les statistiques de defense si defense tentee
    IF p_defense_tentee THEN
        INSERT INTO statistiques_defense (id_match, id_joueur, colonne_defendue, nombre_tirs_subis, nombre_defenses, taux_reussite)
        VALUES (p_id_match, 
                (SELECT id_joueur FROM piece_echecs WHERE id_piece = p_id_piece),
                p_colonne, 1, 
                CASE WHEN p_defense_reussie THEN 1 ELSE 0 END,
                CASE WHEN p_defense_reussie THEN 1.0 ELSE 0.0 END)
        ON CONFLICT (id_match, id_joueur, colonne_defendue) 
        DO UPDATE SET 
            nombre_tirs_subis = statistiques_defense.nombre_tirs_subis + 1,
            nombre_defenses = statistiques_defense.nombre_defenses + CASE WHEN p_defense_reussie THEN 1 ELSE 0 END,
            taux_reussite = (statistiques_defense.nombre_defenses + CASE WHEN p_defense_reussie THEN 1 ELSE 0 END)::FLOAT 
                          / (statistiques_defense.nombre_tirs_subis + 1)::FLOAT,
            date_maj = CURRENT_TIMESTAMP;
    END IF;
    
    RETURN v_id_collision;
END;
$$ LANGUAGE plpgsql;

-- ==================== TRIGGERS ====================

-- Trigger pour mettre a jour la duree du match
CREATE OR REPLACE FUNCTION update_match_duration()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.date_fin IS NOT NULL AND NEW.date_debut IS NOT NULL THEN
        NEW.duree_match = NEW.date_fin - NEW.date_debut;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_match_duration
BEFORE UPDATE ON match_hybride
FOR EACH ROW
EXECUTE FUNCTION update_match_duration();

-- Trigger pour mettre a jour le nombre de pions
CREATE OR REPLACE FUNCTION update_piece_count()
RETURNS TRIGGER AS $$
DECLARE
    v_joueur_id INTEGER;
    v_match_id INTEGER;
    v_is_nord BOOLEAN;
BEGIN
    IF NEW.statut = 'mort' AND OLD.statut != 'mort' THEN
        v_joueur_id := NEW.id_joueur;
        v_match_id := NEW.id_match;
        
        -- Determiner si c'est le joueur nord ou sud
        SELECT (id_joueur_nord = v_joueur_id) INTO v_is_nord
        FROM match_hybride
        WHERE id_match = v_match_id;
        
        -- Mettre a jour le compte de pions
        IF v_is_nord THEN
            UPDATE match_hybride 
            SET nombre_pions_nord = nombre_pions_nord - 1
            WHERE id_match = v_match_id;
            
            -- Verifier si c'est un roi
            IF NEW.type_piece = 'roi' THEN
                UPDATE match_hybride 
                SET roi_nord_vivant = FALSE
                WHERE id_match = v_match_id;
            END IF;
        ELSE
            UPDATE match_hybride 
            SET nombre_pions_sud = nombre_pions_sud - 1
            WHERE id_match = v_match_id;
            
            -- Verifier si c'est un roi
            IF NEW.type_piece = 'roi' THEN
                UPDATE match_hybride 
                SET roi_sud_vivant = FALSE
                WHERE id_match = v_match_id;
            END IF;
        END IF;
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_piece_count
AFTER UPDATE ON piece_echecs
FOR EACH ROW
EXECUTE FUNCTION update_piece_count();

-- Trigger pour mettre a jour les preferences
CREATE OR REPLACE FUNCTION update_preferences_timestamp()
RETURNS TRIGGER AS $$
BEGIN
    NEW.date_maj = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_preferences_timestamp
BEFORE UPDATE ON preferences_joueur
FOR EACH ROW
EXECUTE FUNCTION update_preferences_timestamp();

-- ==================== COMMENTAIRES (sans accents) ====================

COMMENT ON TABLE terrains IS 'Configurations des terrains de jeu paralleles';
COMMENT ON TABLE joueurs IS 'Informations des joueurs avec statistiques paralleles';
COMMENT ON TABLE match_hybride IS 'Matchs avec systeme de colonnes paralleles';
COMMENT ON TABLE piece_echecs IS 'Pieces d echecs avec protection par colonne';
COMMENT ON TABLE balle IS 'Balles avec prediction de sortie par colonne';
COMMENT ON TABLE coup_pingpong IS 'Coups avec precision de colonne';
COMMENT ON TABLE collision_precise IS 'Collisions precises par colonne et rangee';
COMMENT ON TABLE configuration_pieces IS 'Configurations des pieces';
COMMENT ON TABLE statistiques_defense IS 'Statistiques de defense par colonne';
COMMENT ON TABLE preferences_joueur IS 'Preferences des joueurs';

COMMENT ON COLUMN piece_echecs.colonne_protegee IS 'Indique si la piece est protegee par la raquette du joueur';
COMMENT ON COLUMN balle.colonne_sortie_predite IS 'Colonne par laquelle la balle devrait sortir';
COMMENT ON COLUMN coup_pingpong.colonne_visee IS 'Colonne que le joueur visait';
COMMENT ON COLUMN coup_pingpong.colonne_atteinte IS 'Colonne reellement atteinte';

-- ==================== FIN ====================

DO $$
BEGIN
    RAISE NOTICE 'Base de donnees "Echec-Pong Parallele" creee avec succes!';
    RAISE NOTICE 'Tables creees: 10 tables';
    RAISE NOTICE 'Index crees: 22 index';
    RAISE NOTICE 'Vues creees: 4 vues';
    RAISE NOTICE 'Fonctions creees: 4 fonctions';
    RAISE NOTICE 'Triggers crees: 3 triggers';
    RAISE NOTICE '';
    RAISE NOTICE 'Configuration parallele activee:';
    RAISE NOTICE '  - Defense par colonne';
    RAISE NOTICE '  - Prediction de trajectoire';
    RAISE NOTICE '  - Collisions precises';
    RAISE NOTICE '  - Statistiques de defense';
END $$;