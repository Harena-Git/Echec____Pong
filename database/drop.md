# Script de Suppression des Tables - Echec-Pong

Ce script supprime toutes les tables, vues, fonctions et triggers de la base de données.

## ⚠️ ATTENTION
**Ce script supprime TOUTES les données de la base de données !**
Utilisez-le uniquement si vous voulez réinitialiser complètement la base.

## Utilisation

```bash
# Se connecter à PostgreSQL
psql -U postgres -d echec_pong

# Exécuter le script
\i database/drop.sql
```

Ou directement :
```bash
psql -U postgres -d echec_pong -f database/drop.sql
```

## Script SQL (drop.sql)

```sql
-- ============================================
-- SCRIPT DE SUPPRESSION COMPLETE
-- Echec-Pong Database
-- ============================================

-- Supprimer les triggers d'abord
DROP TRIGGER IF EXISTS trigger_update_preferences_timestamp ON preferences_joueur CASCADE;
DROP TRIGGER IF EXISTS trigger_update_piece_count ON piece_echecs CASCADE;
DROP TRIGGER IF EXISTS trigger_update_match_duration ON match_hybride CASCADE;

-- Supprimer les fonctions
DROP FUNCTION IF EXISTS update_preferences_timestamp() CASCADE;
DROP FUNCTION IF EXISTS update_piece_count() CASCADE;
DROP FUNCTION IF EXISTS update_match_duration() CASCADE;
DROP FUNCTION IF EXISTS enregistrer_collision_precise(INTEGER, INTEGER, INTEGER, INTEGER, INTEGER, FLOAT, BOOLEAN, BOOLEAN, INTEGER) CASCADE;
DROP FUNCTION IF EXISTS colonne_est_defendue(INTEGER, INTEGER, INTEGER) CASCADE;
DROP FUNCTION IF EXISTS initialiser_pieces_paralleles(INTEGER, INTEGER, INTEGER) CASCADE;

-- Supprimer les vues
DROP VIEW IF EXISTS vue_pieces_vulnerables CASCADE;
DROP VIEW IF EXISTS vue_defenses_colonnes CASCADE;
DROP VIEW IF EXISTS vue_statistiques_precision CASCADE;
DROP VIEW IF EXISTS vue_matchs_paralleles CASCADE;

-- Supprimer les tables (dans l'ordre pour respecter les contraintes de clés étrangères)
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

-- Message de confirmation
DO $$
BEGIN
    RAISE NOTICE 'Toutes les tables, vues, fonctions et triggers ont ete supprimes avec succes!';
END $$;
```

