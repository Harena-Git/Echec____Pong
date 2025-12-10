-- ============================================
-- BASE DE DONNÉES : PING-PONG ÉCHECS HYBRIDE
-- ============================================

-- Supprimer les tables existantes (si besoin)
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
    total_points INTEGER DEFAULT 0
);

-- Table des matchs
CREATE TABLE match_hybride (
    id_match SERIAL PRIMARY KEY,
    id_terrain INTEGER REFERENCES terrains(id_terrain),
    id_joueur_nord INTEGER REFERENCES joueurs(id_joueur),
    id_joueur_sud INTEGER REFERENCES joueurs(id_joueur),
    
    -- Scores ping-pong
    score_joueur_nord INTEGER DEFAULT 0,
    score_joueur_sud INTEGER DEFAULT 0,
    points_pour_gagner INTEGER DEFAULT 11,
    
    -- État des pièces
    roi_nord_vivant BOOLEAN DEFAULT TRUE,
    roi_sud_vivant BOOLEAN DEFAULT TRUE,
    nombre_pions_nord INTEGER DEFAULT 16,
    nombre_pions_sud INTEGER DEFAULT 16,
    
    -- Informations match
    statut VARCHAR(20) DEFAULT 'en_attente' CHECK (statut IN ('en_attente', 'en_cours', 'termine', 'abandonne')),
    tour_actuel VARCHAR(20) DEFAULT 'pingpong' CHECK (tour_actuel IN ('pingpong', 'deplacement_pions')),
    joueur_au_service INTEGER REFERENCES joueurs(id_joueur),
    
    -- Conditions de victoire
    vainqueur INTEGER REFERENCES joueurs(id_joueur),
    raison_victoire VARCHAR(30) CHECK (raison_victoire IN (
        'roi_capture', 'score_pingpong', 'abandon', 'temps_ecoule', 'pions_elimines'
    )),
    
    -- Timestamps
    date_creation TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    date_debut TIMESTAMP,
    date_fin TIMESTAMP,
    duree_match INTERVAL
);

-- Table des pièces d'échecs
CREATE TABLE piece_echecs (
    id_piece SERIAL PRIMARY KEY,
    id_match INTEGER REFERENCES match_hybride(id_match) ON DELETE CASCADE,
    id_joueur INTEGER REFERENCES joueurs(id_joueur),
    
    -- Type de pièce
    type_piece VARCHAR(20) NOT NULL CHECK (type_piece IN (
        'roi', 'reine', 'tour', 'fou', 'cavalier', 'pion'
    )),
    
    -- Position sur l'échiquier
    colonne INTEGER NOT NULL CHECK (colonne >= 0 AND colonne <= 7),
    rangee INTEGER NOT NULL CHECK (rangee >= 0 AND rangee <= 1),
    
    -- Caractéristiques
    nombre_vies INTEGER NOT NULL DEFAULT 1 CHECK (nombre_vies BETWEEN 1 AND 5),
    vies_restantes INTEGER NOT NULL CHECK (vies_restantes >= 0 AND vies_restantes <= nombre_vies),
    valeur INTEGER NOT NULL,
    
    -- Statut
    statut VARCHAR(20) DEFAULT 'vivant' CHECK (statut IN ('vivant', 'blesse', 'mort', 'protege')),
    
    -- Timestamps
    date_creation TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    date_mort TIMESTAMP,
    
    CHECK (vies_restantes <= nombre_vies)
);

-- Table des balles
CREATE TABLE balle (
    id_balle SERIAL PRIMARY KEY,
    id_match INTEGER REFERENCES match_hybride(id_match) ON DELETE CASCADE,
    
    -- Position
    position_x FLOAT NOT NULL,
    position_y FLOAT NOT NULL,
    position_z FLOAT DEFAULT 0,
    
    -- Vitesse
    vitesse_x FLOAT DEFAULT 0,
    vitesse_y FLOAT DEFAULT 0,
    vitesse_z FLOAT DEFAULT 0,
    
    -- Propriétés
    etat VARCHAR(20) DEFAULT 'en_jeu' CHECK (etat IN ('en_jeu', 'hors_jeu', 'collision', 'perdue')),
    dernier_touche_par INTEGER REFERENCES joueurs(id_joueur),
    force_impact FLOAT DEFAULT 1.0,
    
    -- Timestamps
    date_creation TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    date_dernier_rebond TIMESTAMP
);

-- ==================== TABLES D'ÉVÉNEMENTS ====================

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
    
    -- Caractéristiques
    position_frappe_x FLOAT,
    position_frappe_y FLOAT,
    puissance FLOAT DEFAULT 1.0 CHECK (puissance BETWEEN 0.1 AND 5.0),
    precision FLOAT DEFAULT 1.0 CHECK (precision BETWEEN 0 AND 1),
    angle FLOAT,
    
    -- Résultat
    reussi BOOLEAN DEFAULT TRUE,
    resultat VARCHAR(30) CHECK (resultat IN (
        'bon', 'faute', 'filet', 'hors_table', 'collision_piece'
    )),
    
    -- Timestamps
    date_coup TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Table des collisions balle-pièce
CREATE TABLE collision_balle_piece (
    id_collision SERIAL PRIMARY KEY,
    id_match INTEGER REFERENCES match_hybride(id_match) ON DELETE CASCADE,
    id_balle INTEGER REFERENCES balle(id_balle),
    id_piece INTEGER REFERENCES piece_echecs(id_piece),
    id_joueur_proprietaire INTEGER REFERENCES joueurs(id_joueur),
    
    -- Détails collision
    point_impact_x FLOAT,
    point_impact_y FLOAT,
    force_impact FLOAT,
    degats_infliges INTEGER DEFAULT 1,
    
    -- État après collision
    vies_piece_avant INTEGER,
    vies_piece_apres INTEGER,
    piece_eliminee BOOLEAN DEFAULT FALSE,
    roi_touche BOOLEAN DEFAULT FALSE,
    
    -- Timestamps
    date_collision TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ==================== TABLES DE CONFIGURATION ====================

-- Configuration des pièces
CREATE TABLE configuration_pieces (
    id_config SERIAL PRIMARY KEY,
    nom_config VARCHAR(50) NOT NULL,
    description TEXT,
    
    -- Vies par type
    vies_roi INTEGER DEFAULT 3,
    vies_reine INTEGER DEFAULT 2,
    vies_tour INTEGER DEFAULT 2,
    vies_fou INTEGER DEFAULT 1,
    vies_cavalier INTEGER DEFAULT 1,
    vies_pion INTEGER DEFAULT 1,
    
    -- JSON pour configuration avancée
    regles_json JSONB,
    
    date_creation TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    actif BOOLEAN DEFAULT TRUE
);

-- ==================== INDEX POUR PERFORMANCE ====================

-- Index pour les recherches fréquentes
CREATE INDEX idx_match_statut ON match_hybride(statut);
CREATE INDEX idx_match_joueurs ON match_hybride(id_joueur_nord, id_joueur_sud);
CREATE INDEX idx_piece_match ON piece_echecs(id_match, id_joueur);
CREATE INDEX idx_piece_position ON piece_echecs(colonne, rangee);
CREATE INDEX idx_balle_match ON balle(id_match);
CREATE INDEX idx_coup_match ON coup_pingpong(id_match);
CREATE INDEX idx_collision_match ON collision_balle_piece(id_match);
CREATE INDEX idx_joueur_pseudo ON joueurs(pseudo);
CREATE INDEX idx_match_date ON match_hybride(date_creation);

-- Index pour les recherches spatiales
CREATE INDEX idx_balle_position ON balle(position_x, position_y);
CREATE INDEX idx_piece_vies ON piece_echecs(vies_restantes) WHERE statut = 'vivant';

-- ==================== DONNÉES INITIALES ====================

-- Insérer un terrain par défaut
INSERT INTO terrains (nom_terrain, largeur_pingpong, longueur_pingpong, 
                      largeur_zone_echecs, profondeur_zone_echecs,
                      nombre_colonnes_pions, nombre_rangees_pions)
VALUES ('Terrain Principal', 2.74, 1.525, 2.74, 1.0, 8, 2);

-- Insérer la configuration par défaut des pièces
INSERT INTO configuration_pieces (nom_config, description, vies_roi, vies_reine, 
                                  vies_tour, vies_fou, vies_cavalier, vies_pion)
VALUES ('Classique', 'Configuration standard avec roi renforcé', 3, 2, 2, 1, 1, 1);

-- Insérer un joueur test (optionnel)
INSERT INTO joueurs (pseudo, classement)
VALUES ('JoueurTest', 1000);

-- ==================== VUES UTILES ====================

-- Vue pour les matchs en cours
CREATE VIEW vue_matchs_en_cours AS
SELECT 
    m.id_match,
    jn.pseudo as joueur_nord,
    js.pseudo as joueur_sud,
    m.score_joueur_nord,
    m.score_joueur_sud,
    m.nombre_pions_nord,
    m.nombre_pions_sud,
    m.date_debut,
    EXTRACT(EPOCH FROM (CURRENT_TIMESTAMP - m.date_debut)) as duree_secondes
FROM match_hybride m
JOIN joueurs jn ON m.id_joueur_nord = jn.id_joueur
JOIN joueurs js ON m.id_joueur_sud = js.id_joueur
WHERE m.statut = 'en_cours';

-- Vue pour les statistiques des joueurs
CREATE VIEW vue_statistiques_joueurs AS
SELECT 
    j.id_joueur,
    j.pseudo,
    j.classement,
    j.parties_jouees,
    j.parties_gagnees,
    CASE 
        WHEN j.parties_jouees > 0 THEN ROUND((j.parties_gagnees::FLOAT / j.parties_jouees) * 100, 2)
        ELSE 0 
    END as pourcentage_victoires,
    j.total_points,
    COUNT(DISTINCT m.id_match) as matchs_joues,
    COUNT(DISTINCT CASE WHEN m.vainqueur = j.id_joueur THEN m.id_match END) as matchs_gagnes
FROM joueurs j
LEFT JOIN match_hybride m ON j.id_joueur IN (m.id_joueur_nord, m.id_joueur_sud)
GROUP BY j.id_joueur, j.pseudo, j.classement, j.parties_jouees, j.parties_gagnees, j.total_points;

-- Vue pour les pièces vulnérables (peu de vies)
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
JOIN joueurs j ON p.id_joueur = j.id_joueur
JOIN match_hybride m ON p.id_match = m.id_match
WHERE p.statut = 'vivant' AND p.vies_restantes <= 1
ORDER BY p.vies_restantes ASC;

-- ==================== FONCTIONS ====================

-- Fonction pour initialiser les pièces d'un match
CREATE OR REPLACE FUNCTION initialiser_pieces_match(
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
    -- Récupérer la configuration active
    SELECT id_config, vies_roi, vies_reine, vies_tour, vies_fou, vies_cavalier, vies_pion
    INTO v_config_id, v_vies_roi, v_vies_reine, v_vies_tour, v_vies_fou, v_vies_cavalier, v_vies_pion
    FROM configuration_pieces
    WHERE actif = TRUE
    ORDER BY id_config DESC
    LIMIT 1;
    
    -- Pièces pour le joueur Nord (rangée 0-1)
    INSERT INTO piece_echecs (id_match, id_joueur, type_piece, colonne, rangee, nombre_vies, vies_restantes, valeur)
    VALUES 
    -- Rangée arrière
    (p_id_match, p_id_joueur_nord, 'tour', 0, 0, v_vies_tour, v_vies_tour, 5),
    (p_id_match, p_id_joueur_nord, 'cavalier', 1, 0, v_vies_cavalier, v_vies_cavalier, 3),
    (p_id_match, p_id_joueur_nord, 'fou', 2, 0, v_vies_fou, v_vies_fou, 3),
    (p_id_match, p_id_joueur_nord, 'reine', 3, 0, v_vies_reine, v_vies_reine, 9),
    (p_id_match, p_id_joueur_nord, 'roi', 4, 0, v_vies_roi, v_vies_roi, 100),
    (p_id_match, p_id_joueur_nord, 'fou', 5, 0, v_vies_fou, v_vies_fou, 3),
    (p_id_match, p_id_joueur_nord, 'cavalier', 6, 0, v_vies_cavalier, v_vies_cavalier, 3),
    (p_id_match, p_id_joueur_nord, 'tour', 7, 0, v_vies_tour, v_vies_tour, 5),
    -- Pions
    (p_id_match, p_id_joueur_nord, 'pion', 0, 1, v_vies_pion, v_vies_pion, 1),
    (p_id_match, p_id_joueur_nord, 'pion', 1, 1, v_vies_pion, v_vies_pion, 1),
    (p_id_match, p_id_joueur_nord, 'pion', 2, 1, v_vies_pion, v_vies_pion, 1),
    (p_id_match, p_id_joueur_nord, 'pion', 3, 1, v_vies_pion, v_vies_pion, 1),
    (p_id_match, p_id_joueur_nord, 'pion', 4, 1, v_vies_pion, v_vies_pion, 1),
    (p_id_match, p_id_joueur_nord, 'pion', 5, 1, v_vies_pion, v_vies_pion, 1),
    (p_id_match, p_id_joueur_nord, 'pion', 6, 1, v_vies_pion, v_vies_pion, 1),
    (p_id_match, p_id_joueur_nord, 'pion', 7, 1, v_vies_pion, v_vies_pion, 1);
    
    -- Pièces pour le joueur Sud (rangée 1-0 inversé)
    INSERT INTO piece_echecs (id_match, id_joueur, type_piece, colonne, rangee, nombre_vies, vies_restantes, valeur)
    VALUES 
    -- Rangée arrière
    (p_id_match, p_id_joueur_sud, 'tour', 0, 1, v_vies_tour, v_vies_tour, 5),
    (p_id_match, p_id_joueur_sud, 'cavalier', 1, 1, v_vies_cavalier, v_vies_cavalier, 3),
    (p_id_match, p_id_joueur_sud, 'fou', 2, 1, v_vies_fou, v_vies_fou, 3),
    (p_id_match, p_id_joueur_sud, 'reine', 3, 1, v_vies_reine, v_vies_reine, 9),
    (p_id_match, p_id_joueur_sud, 'roi', 4, 1, v_vies_roi, v_vies_roi, 100),
    (p_id_match, p_id_joueur_sud, 'fou', 5, 1, v_vies_fou, v_vies_fou, 3),
    (p_id_match, p_id_joueur_sud, 'cavalier', 6, 1, v_vies_cavalier, v_vies_cavalier, 3),
    (p_id_match, p_id_joueur_sud, 'tour', 7, 1, v_vies_tour, v_vies_tour, 5),
    -- Pions
    (p_id_match, p_id_joueur_sud, 'pion', 0, 0, v_vies_pion, v_vies_pion, 1),
    (p_id_match, p_id_joueur_sud, 'pion', 1, 0, v_vies_pion, v_vies_pion, 1),
    (p_id_match, p_id_joueur_sud, 'pion', 2, 0, v_vies_pion, v_vies_pion, 1),
    (p_id_match, p_id_joueur_sud, 'pion', 3, 0, v_vies_pion, v_vies_pion, 1),
    (p_id_match, p_id_joueur_sud, 'pion', 4, 0, v_vies_pion, v_vies_pion, 1),
    (p_id_match, p_id_joueur_sud, 'pion', 5, 0, v_vies_pion, v_vies_pion, 1),
    (p_id_match, p_id_joueur_sud, 'pion', 6, 0, v_vies_pion, v_vies_pion, 1),
    (p_id_match, p_id_joueur_sud, 'pion', 7, 0, v_vies_pion, v_vies_pion, 1);
END;
$$ LANGUAGE plpgsql;

-- Fonction pour vérifier la victoire
CREATE OR REPLACE FUNCTION verifier_victoire_match(p_id_match INTEGER)
RETURNS TABLE (
    victoire BOOLEAN,
    id_vainqueur INTEGER,
    raison VARCHAR(30)
) AS $$
DECLARE
    v_match RECORD;
    v_roi_nord_vivant BOOLEAN;
    v_roi_sud_vivant BOOLEAN;
    v_score_nord INTEGER;
    v_score_sud INTEGER;
    v_points_pour_gagner INTEGER;
BEGIN
    -- Récupérer les infos du match
    SELECT roi_nord_vivant, roi_sud_vivant, score_joueur_nord, score_joueur_sud, points_pour_gagner
    INTO v_roi_nord_vivant, v_roi_sud_vivant, v_score_nord, v_score_sud, v_points_pour_gagner
    FROM match_hybride
    WHERE id_match = p_id_match;
    
    -- Vérifier si un roi est mort
    IF NOT v_roi_nord_vivant THEN
        RETURN QUERY SELECT TRUE, 
            (SELECT id_joueur_sud FROM match_hybride WHERE id_match = p_id_match),
            'roi_capture';
        RETURN;
    END IF;
    
    IF NOT v_roi_sud_vivant THEN
        RETURN QUERY SELECT TRUE,
            (SELECT id_joueur_nord FROM match_hybride WHERE id_match = p_id_match),
            'roi_capture';
        RETURN;
    END IF;
    
    -- Vérifier la victoire au ping-pong
    IF v_score_nord >= v_points_pour_gagner AND v_score_nord - v_score_sud >= 2 THEN
        RETURN QUERY SELECT TRUE,
            (SELECT id_joueur_nord FROM match_hybride WHERE id_match = p_id_match),
            'score_pingpong';
        RETURN;
    END IF;
    
    IF v_score_sud >= v_points_pour_gagner AND v_score_sud - v_score_nord >= 2 THEN
        RETURN QUERY SELECT TRUE,
            (SELECT id_joueur_sud FROM match_hybride WHERE id_match = p_id_match),
            'score_pingpong';
        RETURN;
    END IF;
    
    -- Pas de victoire
    RETURN QUERY SELECT FALSE, NULL::INTEGER, NULL::VARCHAR;
END;
$$ LANGUAGE plpgsql;

-- Fonction pour mettre à jour les statistiques après un match
CREATE OR REPLACE FUNCTION mettre_a_jour_statistiques(p_id_match INTEGER)
RETURNS VOID AS $$
DECLARE
    v_vainqueur INTEGER;
    v_joueur_nord INTEGER;
    v_joueur_sud INTEGER;
BEGIN
    -- Récupérer les informations
    SELECT vainqueur, id_joueur_nord, id_joueur_sud
    INTO v_vainqueur, v_joueur_nord, v_joueur_sud
    FROM match_hybride
    WHERE id_match = p_id_match;
    
    -- Mettre à jour les statistiques des joueurs
    UPDATE joueurs
    SET parties_jouees = parties_jouees + 1,
        parties_gagnees = parties_gagnees + CASE WHEN id_joueur = v_vainqueur THEN 1 ELSE 0 END,
        total_points = total_points + CASE 
            WHEN id_joueur = v_joueur_nord THEN (SELECT score_joueur_nord FROM match_hybride WHERE id_match = p_id_match)
            WHEN id_joueur = v_joueur_sud THEN (SELECT score_joueur_sud FROM match_hybride WHERE id_match = p_id_match)
            ELSE 0
        END
    WHERE id_joueur IN (v_joueur_nord, v_joueur_sud);
END;
$$ LANGUAGE plpgsql;

-- ==================== TRIGGERS ====================

-- Trigger pour mettre à jour automatiquement la durée du match
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

-- Trigger pour mettre à jour le nombre de pions
CREATE OR REPLACE FUNCTION update_piece_count()
RETURNS TRIGGER AS $$
DECLARE
    v_joueur_id INTEGER;
    v_match_id INTEGER;
BEGIN
    IF NEW.statut = 'mort' AND OLD.statut != 'mort' THEN
        -- Récupérer les IDs
        v_joueur_id := NEW.id_joueur;
        v_match_id := NEW.id_match;
        
        -- Mettre à jour le compte de pions
        IF v_joueur_id = (SELECT id_joueur_nord FROM match_hybride WHERE id_match = v_match_id) THEN
            UPDATE match_hybride 
            SET nombre_pions_nord = nombre_pions_nord - 1
            WHERE id_match = v_match_id;
        ELSE
            UPDATE match_hybride 
            SET nombre_pions_sud = nombre_pions_sud - 1
            WHERE id_match = v_match_id;
        END IF;
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_piece_count
AFTER UPDATE ON piece_echecs
FOR EACH ROW
EXECUTE FUNCTION update_piece_count();

-- ==================== COMMENTAIRES ====================

COMMENT ON TABLE terrains IS 'Stocke les configurations des terrains de jeu';
COMMENT ON TABLE joueurs IS 'Stocke les informations des joueurs';
COMMENT ON TABLE match_hybride IS 'Stocke les matchs de ping-pong/échecs';
COMMENT ON TABLE piece_echecs IS 'Stocke les pièces d''échecs de chaque match';
COMMENT ON TABLE balle IS 'Stocke l''état des balles pendant les matchs';
COMMENT ON TABLE coup_pingpong IS 'Stocke les coups de ping-pong joués';
COMMENT ON TABLE collision_balle_piece IS 'Stocke les collisions entre balle et pièces';
COMMENT ON TABLE configuration_pieces IS 'Stocke les configurations des pièces';

COMMENT ON COLUMN piece_echecs.valeur IS 'Valeur de la pièce pour les statistiques (pion=1, cavalier/fou=3, tour=5, reine=9, roi=100)';
COMMENT ON COLUMN match_hybride.raison_victoire IS 'Comment la victoire a été obtenue';

-- ==================== FIN ====================

-- Message de confirmation
DO $$
BEGIN
    RAISE NOTICE 'Base de données créée avec succès!';
    RAISE NOTICE 'Tables créées: terrains, joueurs, match_hybride, piece_echecs, balle, coup_pingpong, collision_balle_piece, configuration_pieces';
    RAISE NOTICE 'Vues créées: vue_matchs_en_cours, vue_statistiques_joueurs, vue_pieces_vulnerables';
    RAISE NOTICE 'Fonctions créées: initialiser_pieces_match, verifier_victoire_match, mettre_a_jour_statistiques';
    RAISE NOTICE 'Triggers créés: update_match_duration, update_piece_count';
END $$;