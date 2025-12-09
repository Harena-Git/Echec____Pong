-- ==================== TABLES PRINCIPALES ====================

-- Terrain de jeu hybride (ping-pong + échecs)
CREATE TABLE terrain (
    id_terrain SERIAL PRIMARY KEY,
    nom_terrain VARCHAR(100) NOT NULL,
    -- Dimensions zone ping-pong
    largeur_pingpong FLOAT NOT NULL DEFAULT 2.74,
    longueur_pingpong FLOAT NOT NULL DEFAULT 1.525,
    -- Dimensions zone échecs (derrière chaque joueur)
    largeur_zone_echecs FLOAT NOT NULL DEFAULT 2.74,
    profondeur_zone_echecs FLOAT NOT NULL DEFAULT 1.0,
    -- Configuration des colonnes pour les pions
    nombre_colonnes_pions INTEGER NOT NULL DEFAULT 8 CHECK (nombre_colonnes_pions BETWEEN 1 AND 12),
    nombre_rangees_pions INTEGER NOT NULL DEFAULT 2 CHECK (nombre_rangees_pions BETWEEN 1 AND 4)
);

-- Joueur
CREATE TABLE joueur (
    id_joueur SERIAL PRIMARY KEY,
    pseudo VARCHAR(50) NOT NULL UNIQUE,
    position_x FLOAT DEFAULT 0.5,  -- Position sur la table de ping-pong (0-1)
    position_y FLOAT DEFAULT 0.5,  -- Côté de la table (0 = bas, 1 = haut)
    statut VARCHAR(20) DEFAULT 'actif' CHECK (statut IN ('actif', 'elimine', 'en_attente')),
    cote_terrain VARCHAR(10) CHECK (cote_terrain IN ('nord', 'sud', NULL))
);

-- Match hybride ping-pong/échecs
CREATE TABLE match_hybride (
    id_match SERIAL PRIMARY KEY,
    id_terrain INTEGER REFERENCES terrain(id_terrain),
    id_joueur_nord INTEGER REFERENCES joueur(id_joueur),
    id_joueur_sud INTEGER REFERENCES joueur(id_joueur),
    
    -- Scores ping-pong
    score_joueur_nord INTEGER DEFAULT 0,
    score_joueur_sud INTEGER DEFAULT 0,
    points_pour_gagner INTEGER DEFAULT 11,
    
    -- État des pièces d'échecs
    roi_nord_vivant BOOLEAN DEFAULT TRUE,
    roi_sud_vivant BOOLEAN DEFAULT TRUE,
    nombre_pions_nord INTEGER DEFAULT 16,
    nombre_pions_sud INTEGER DEFAULT 16,
    
    -- Informations match
    statut VARCHAR(20) DEFAULT 'en_attente' CHECK (statut IN ('en_attente', 'en_cours', 'termine', 'abandonne')),
    tour_actuel VARCHAR(10) DEFAULT 'pingpong' CHECK (tour_actuel IN ('pingpong', 'deplacement_pions')),
    joueur_au_service INTEGER REFERENCES joueur(id_joueur),
    date_debut TIMESTAMP,
    date_fin TIMESTAMP,
    
    -- Conditions de victoire
    vainqueur INTEGER REFERENCES joueur(id_joueur),
    raison_victoire VARCHAR(30) CHECK (raison_victoire IN (
        'roi_capture', 'score_pingpong', 'abandon', 'temps_ecoule', 'pions_elimines'
    ))
);

-- ==================== PIÈCES D'ÉCHECS ====================

-- Types de pièces d'échecs
CREATE TYPE type_piece AS ENUM ('roi', 'reine', 'tour', 'fou', 'cavalier', 'pion');

-- Pièces d'échecs de chaque joueur
CREATE TABLE piece_echecs (
    id_piece SERIAL PRIMARY KEY,
    id_joueur INTEGER REFERENCES joueur(id_joueur) ON DELETE CASCADE,
    id_match INTEGER REFERENCES match_hybride(id_match) ON DELETE CASCADE,
    type_piece type_piece NOT NULL,
    
    -- Position sur la zone échecs (relatif au terrain du joueur)
    colonne INTEGER NOT NULL CHECK (colonne >= 0 AND colonne <= 7),
    rangee INTEGER NOT NULL CHECK (rangee >= 0 AND rangee <= 1),
    
    -- Caractéristiques
    nombre_vies INTEGER NOT NULL DEFAULT 1 CHECK (nombre_vies BETWEEN 1 AND 5),
    vies_restantes INTEGER NOT NULL CHECK (vies_restantes >= 0 AND vies_restantes <= nombre_vies),
    valeur INTEGER NOT NULL,  -- Valeur de la pièce (1 pour pion, 9 pour reine, etc.)
    
    -- Statut
    statut VARCHAR(20) DEFAULT 'vivant' CHECK (statut IN ('vivant', 'blesse', 'mort', 'protege')),
    date_creation TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CHECK (vies_restantes <= nombre_vies)
);

-- Configuration initiale des pièces (template)
CREATE TABLE configuration_pieces (
    id_config SERIAL PRIMARY KEY,
    nom_config VARCHAR(50) NOT NULL,
    description TEXT,
    
    -- Nombre de vies par type de pièce
    vies_roi INTEGER DEFAULT 3,
    vies_reine INTEGER DEFAULT 2,
    vies_tour INTEGER DEFAULT 2,
    vies_fou INTEGER DEFAULT 1,
    vies_cavalier INTEGER DEFAULT 1,
    vies_pion INTEGER DEFAULT 1,
    
    -- Position initiale
    position_initiale JSONB  -- Stocke la configuration initiale au format JSON
);

-- ==================== BALLE DE PING-PONG ====================

CREATE TABLE balle (
    id_balle SERIAL PRIMARY KEY,
    id_match INTEGER REFERENCES match_hybride(id_match) ON DELETE CASCADE,
    
    -- Position actuelle
    position_x FLOAT NOT NULL,  -- 0-1 (0 = côté sud, 1 = côté nord)
    position_y FLOAT NOT NULL,  -- 0-1 (largeur)
    position_z FLOAT DEFAULT 0, -- hauteur
    
    -- Vecteur mouvement
    vitesse_x FLOAT DEFAULT 0,
    vitesse_y FLOAT DEFAULT 0,
    vitesse_z FLOAT DEFAULT 0,
    
    -- Propriétés
    etat VARCHAR(20) DEFAULT 'en_jeu' CHECK (etat IN ('en_jeu', 'hors_jeu', 'collision', 'perdue')),
    dernier_touche_par INTEGER REFERENCES joueur(id_joueur),
    force_impact FLOAT DEFAULT 1.0,
    
    -- Timing
    moment_creation TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    moment_dernier_rebond TIMESTAMP
);

-- ==================== ÉVÉNEMENTS DE JEU ====================

-- Service/Renvoi de balle
CREATE TABLE coup_pingpong (
    id_coup SERIAL PRIMARY KEY,
    id_match INTEGER REFERENCES match_hybride(id_match) ON DELETE CASCADE,
    id_joueur INTEGER REFERENCES joueur(id_joueur),
    id_balle INTEGER REFERENCES balle(id_balle),
    
    type_coup VARCHAR(30) CHECK (type_coup IN (
        'service', 'drive', 'topspin', 'contre', 'lob', 'amorti', 'smash'
    )),
    
    -- Caractéristiques du coup
    position_frappe_x FLOAT,
    position_frappe_y FLOAT,
    puissance FLOAT DEFAULT 1.0 CHECK (puissance BETWEEN 0.1 AND 5.0),
    precision FLOAT DEFAULT 1.0 CHECK (precision BETWEEN 0 AND 1),
    angle FLOAT,  -- angle de frappe
    
    -- Résultat
    reussi BOOLEAN DEFAULT TRUE,
    resultat VARCHAR(30) CHECK (resultat IN (
        'bon', 'faute', 'filet', 'hors_table', 'collision_piece'
    )),
    
    moment TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Collision balle-pièce
CREATE TABLE collision_balle_piece (
    id_collision SERIAL PRIMARY KEY,
    id_match INTEGER REFERENCES match_hybride(id_match) ON DELETE CASCADE,
    id_balle INTEGER REFERENCES balle(id_balle),
    id_piece INTEGER REFERENCES piece_echecs(id_piece),
    id_joueur_proprietaire INTEGER REFERENCES joueur(id_joueur),
    
    -- Détails collision
    point_impact_x FLOAT,
    point_impact_y FLOAT,
    force_impact FLOAT,
    degats_infliges INTEGER DEFAULT 1,
    
    -- État après collision
    vies_piece_avant INTEGER,
    vies_piece_apres INTEGER,
    piece_eliminee BOOLEAN DEFAULT FALSE,
    
    -- Si le roi est touché
    roi_touche BOOLEAN DEFAULT FALSE,
    
    moment TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Déplacement des pièces d'échecs (optionnel - si les joueurs peuvent déplacer leurs pièces)
CREATE TABLE deplacement_piece (
    id_deplacement SERIAL PRIMARY KEY,
    id_match INTEGER REFERENCES match_hybride(id_match) ON DELETE CASCADE,
    id_joueur INTEGER REFERENCES joueur(id_joueur),
    id_piece INTEGER REFERENCES piece_echecs(id_piece),
    
    -- Position avant/après
    colonne_depart INTEGER,
    rangee_depart INTEGER,
    colonne_arrivee INTEGER,
    rangee_arrivee INTEGER,
    
    type_deplacement VARCHAR(20) CHECK (type_deplacement IN (
        'normal', 'capture', 'roque', 'promotion'
    )),
    
    moment TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ==================== STATISTIQUES ====================

CREATE TABLE statistiques_joueur_match (
    id_statistique SERIAL PRIMARY KEY,
    id_match INTEGER REFERENCES match_hybride(id_match) ON DELETE CASCADE,
    id_joueur INTEGER REFERENCES joueur(id_joueur),
    
    -- Ping-pong
    coups_reussis INTEGER DEFAULT 0,
    coups_rates INTEGER DEFAULT 0,
    services INTEGER DEFAULT 0,
    aces INTEGER DEFAULT 0,
    fautes INTEGER DEFAULT 0,
    
    -- Échecs
    pieces_protegees INTEGER DEFAULT 0,
    pieces_perdues INTEGER DEFAULT 0,
    degats_infliges INTEGER DEFAULT 0,
    degats_subis INTEGER DEFAULT 0,
    
    -- Hybrides
    pieces_sauvees INTEGER DEFAULT 0,  -- Interceptions de balle protégeant une pièce
    collisions_evitees INTEGER DEFAULT 0,
    
    date_maj TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(id_match, id_joueur)
);

-- ==================== FONCTIONS ET DÉCLENCHEURS ====================

-- Fonction pour initialiser les pièces d'un joueur
CREATE OR REPLACE FUNCTION initialiser_pieces_echecs(
    p_id_joueur INTEGER,
    p_id_match INTEGER,
    p_cote VARCHAR(10)
) RETURNS VOID AS $$
DECLARE
    v_rangee_base INTEGER;
    v_rangee_pions INTEGER;
BEGIN
    -- Déterminer les rangées selon le côté
    IF p_cote = 'nord' THEN
        v_rangee_base := 0;
        v_rangee_pions := 1;
    ELSE
        v_rangee_base := 1;
        v_rangee_pions := 0;
    END IF;
    
    -- Insérer les pièces selon la configuration classique
    -- Tours
    INSERT INTO piece_echecs (id_joueur, id_match, type_piece, colonne, rangee, nombre_vies, vies_restantes, valeur)
    VALUES 
    (p_id_joueur, p_id_match, 'tour', 0, v_rangee_base, 2, 2, 5),
    (p_id_joueur, p_id_match, 'tour', 7, v_rangee_base, 2, 2, 5);
    
    -- Cavaliers
    INSERT INTO piece_echecs (id_joueur, id_match, type_piece, colonne, rangee, nombre_vies, vies_restantes, valeur)
    VALUES 
    (p_id_joueur, p_id_match, 'cavalier', 1, v_rangee_base, 1, 1, 3),
    (p_id_joueur, p_id_match, 'cavalier', 6, v_rangee_base, 1, 1, 3);
    
    -- Fous
    INSERT INTO piece_echecs (id_joueur, id_match, type_piece, colonne, rangee, nombre_vies, vies_restantes, valeur)
    VALUES 
    (p_id_joueur, p_id_match, 'fou', 2, v_rangee_base, 1, 1, 3),
    (p_id_joueur, p_id_match, 'fou', 5, v_rangee_base, 1, 1, 3);
    
    -- Reine et Roi
    INSERT INTO piece_echecs (id_joueur, id_match, type_piece, colonne, rangee, nombre_vies, vies_restantes, valeur)
    VALUES 
    (p_id_joueur, p_id_match, 'reine', 3, v_rangee_base, 2, 2, 9),
    (p_id_joueur, p_id_match, 'roi', 4, v_rangee_base, 3, 3, 100);  -- Roi avec plus de vies
    
    -- Pions
    FOR i IN 0..7 LOOP
        INSERT INTO piece_echecs (id_joueur, id_match, type_piece, colonne, rangee, nombre_vies, vies_restantes, valeur)
        VALUES (p_id_joueur, p_id_match, 'pion', i, v_rangee_pions, 1, 1, 1);
    END LOOP;
END;
$$ LANGUAGE plpgsql;

-- Déclencheur pour gérer une collision balle-pièce
CREATE OR REPLACE FUNCTION gerer_collision_balle_piece()
RETURNS TRIGGER AS $$
DECLARE
    v_nouvelles_vies INTEGER;
    v_piece_morte BOOLEAN;
    v_type_piece type_piece;
    v_id_joueur_proprietaire INTEGER;
    v_roi_touche BOOLEAN := FALSE;
BEGIN
    -- Récupérer les infos de la pièce
    SELECT p.vies_restantes, p.type_piece, p.id_joueur
    INTO v_nouvelles_vies, v_type_piece, v_id_joueur_proprietaire
    FROM piece_echecs p
    WHERE p.id_piece = NEW.id_piece;
    
    -- Calculer les nouvelles vies
    v_nouvelles_vies := v_nouvelles_vies - NEW.degats_infliges;
    
    -- Vérifier si la pièce est morte
    v_piece_morte := v_nouvelles_vies <= 0;
    
    -- Mettre à jour la pièce
    IF v_piece_morte THEN
        UPDATE piece_echecs 
        SET statut = 'mort', vies_restantes = 0
        WHERE id_piece = NEW.id_piece;
        
        -- Vérifier si c'est un roi
        IF v_type_piece = 'roi' THEN
            v_roi_touche := TRUE;
            -- Mettre à jour le match
            UPDATE match_hybride 
            SET 
                vainqueur = (SELECT id_joueur_nord FROM match_hybride WHERE id_match = NEW.id_match 
                             AND id_joueur_nord != v_id_joueur_proprietaire
                             UNION ALL
                             SELECT id_joueur_sud FROM match_hybride WHERE id_match = NEW.id_match 
                             AND id_joueur_sud != v_id_joueur_proprietaire
                             LIMIT 1),
                raison_victoire = 'roi_capture',
                statut = 'termine',
                date_fin = CURRENT_TIMESTAMP
            WHERE id_match = NEW.id_match;
        END IF;
        
        -- Mettre à jour le compteur de pièces
        IF v_id_joueur_proprietaire = (SELECT id_joueur_nord FROM match_hybride WHERE id_match = NEW.id_match) THEN
            UPDATE match_hybride 
            SET nombre_pions_nord = nombre_pions_nord - 1
            WHERE id_match = NEW.id_match;
        ELSE
            UPDATE match_hybride 
            SET nombre_pions_sud = nombre_pions_sud - 1
            WHERE id_match = NEW.id_match;
        END IF;
    ELSE
        -- Mettre à jour les vies restantes
        UPDATE piece_echecs 
        SET vies_restantes = v_nouvelles_vies,
            statut = CASE 
                WHEN v_nouvelles_vies = 1 THEN 'blesse'
                ELSE 'vivant'
            END
        WHERE id_piece = NEW.id_piece;
    END IF;
    
    -- Mettre à jour la collision avec les valeurs finales
    NEW.vies_piece_avant := v_nouvelles_vies + NEW.degats_infliges;
    NEW.vies_piece_apres := GREATEST(v_nouvelles_vies, 0);
    NEW.piece_eliminee := v_piece_morte;
    NEW.roi_touche := v_roi_touche;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_gerer_collision_balle_piece
BEFORE INSERT ON collision_balle_piece
FOR EACH ROW
EXECUTE FUNCTION gerer_collision_balle_piece();

-- ==================== INDEX ====================

CREATE INDEX idx_piece_match ON piece_echecs(id_match, id_joueur);
CREATE INDEX idx_piece_position ON piece_echecs(colonne, rangee, id_match);
CREATE INDEX idx_balle_match ON balle(id_match);
CREATE INDEX idx_coup_match ON coup_pingpong(id_match, id_joueur);
CREATE INDEX idx_collision_match ON collision_balle_piece(id_match);
CREATE INDEX idx_match_statut ON match_hybride(statut);
CREATE INDEX idx_joueur_statut ON joueur(statut);
CREATE INDEX idx_piece_type ON piece_echecs(type_piece, statut);

-- ==================== VUES UTILES ====================

-- Vue pour l'état actuel du match
CREATE VIEW vue_etat_match AS
SELECT 
    m.id_match,
    jn.pseudo as joueur_nord,
    js.pseudo as joueur_sud,
    m.score_joueur_nord || '-' || m.score_joueur_sud as score_pingpong,
    m.nombre_pions_nord as pions_nord_restants,
    m.nombre_pions_sud as pions_sud_restants,
    m.roi_nord_vivant and m.roi_sud_vivant as rois_vivants,
    m.statut,
    m.tour_actuel
FROM match_hybride m
JOIN joueur jn ON m.id_joueur_nord = jn.id_joueur
JOIN joueur js ON m.id_joueur_sud = js.id_joueur;

-- Vue pour les pièces vulnérables (peu de vies restantes)
CREATE VIEW vue_pieces_vulnerables AS
SELECT 
    p.id_piece,
    j.pseudo as proprietaire,
    p.type_piece,
    p.vies_restantes,
    p.colonne,
    p.rangee,
    m.id_match
FROM piece_echecs p
JOIN joueur j ON p.id_joueur = j.id_joueur
JOIN match_hybride m ON p.id_match = m.id_match
WHERE p.statut = 'vivant' AND p.vies_restantes <= 1
ORDER BY p.vies_restantes ASC;