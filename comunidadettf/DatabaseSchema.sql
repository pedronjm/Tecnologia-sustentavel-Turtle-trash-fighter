-- Schema compartilhado para APIs C# e Java (turtle_trash_fighter)
-- Compatible com SaveApi.csproj e comunidadettf Spring Boot

CREATE DATABASE IF NOT EXISTS turtle_trash_fighter;
USE turtle_trash_fighter;

-- Tabela de usuários (compatível com ambas APIs)
CREATE TABLE IF NOT EXISTS game_users (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  login VARCHAR(80) NOT NULL,
  password_hash VARCHAR(255) NOT NULL,
  nome VARCHAR(120) NOT NULL,
  created_at_utc DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_game_users_login (login)
);

-- Compatibilidade com SaveApi (C#)
CREATE TABLE IF NOT EXISTS users (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  login VARCHAR(80) NOT NULL,
  password_hash VARCHAR(255) NOT NULL,
  password_salt VARCHAR(255) NOT NULL,
  nome VARCHAR(120) NOT NULL,
  created_at_utc DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_users_login (login)
);

-- Tabela de configurações de usuário
CREATE TABLE IF NOT EXISTS user_configs (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  user_id BIGINT UNSIGNED NOT NULL,
  volume_master FLOAT NOT NULL DEFAULT 1,
  volume_music FLOAT NOT NULL DEFAULT 1,
  volume_sfx FLOAT NOT NULL DEFAULT 1,
  keybinds_json JSON NOT NULL,
  updated_at_utc DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_user_configs_user (user_id),
  CONSTRAINT fk_user_configs_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

-- Tabela de saves de jogo
CREATE TABLE IF NOT EXISTS game_saves (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  user_id BIGINT UNSIGNED NOT NULL,
  slot_index INT NOT NULL,
  slot_name VARCHAR(120) NOT NULL,
  selected_character VARCHAR(40) NOT NULL,
  play_tutorial TINYINT(1) NOT NULL DEFAULT 1,
  difficulty VARCHAR(40) NOT NULL,
  scene_name VARCHAR(120) NOT NULL,
  checkpoint_id VARCHAR(120) NOT NULL,
  checkpoint_x FLOAT NOT NULL DEFAULT 0,
  checkpoint_y FLOAT NOT NULL DEFAULT 0,
  checkpoint_z FLOAT NOT NULL DEFAULT 0,
  collected_ids_json JSON NOT NULL,
  dead_enemy_ids_json JSON NOT NULL,
  completion_percent FLOAT NOT NULL DEFAULT 0,
  last_saved_at_utc DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_game_saves_user_slot (user_id, slot_index),
  CONSTRAINT fk_game_saves_user FOREIGN KEY (user_id) REFERENCES game_users(id) ON DELETE CASCADE
);

-- Compatibilidade com SaveApi (C#) - usa tabela com nome 'saves'
CREATE TABLE IF NOT EXISTS saves (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  user_id BIGINT UNSIGNED NOT NULL,
  slot_index INT NOT NULL,
  slot_name VARCHAR(120) NOT NULL,
  selected_character VARCHAR(40) NOT NULL,
  play_tutorial TINYINT(1) NOT NULL DEFAULT 1,
  difficulty VARCHAR(40) NOT NULL,
  scene_name VARCHAR(120) NOT NULL,
  checkpoint_id VARCHAR(120) NOT NULL,
  checkpoint_x FLOAT NOT NULL DEFAULT 0,
  checkpoint_y FLOAT NOT NULL DEFAULT 0,
  checkpoint_z FLOAT NOT NULL DEFAULT 0,
  collected_ids_json JSON NOT NULL,
  dead_enemy_ids_json JSON NOT NULL,
  completion_percent FLOAT NOT NULL DEFAULT 0,
  last_saved_at_utc DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_saves_user_slot (user_id, slot_index),
  CONSTRAINT fk_saves_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

-- Tabela de comunidade (apenas Java)
CREATE TABLE IF NOT EXISTS usuario (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  nome VARCHAR(60) NOT NULL,
  email VARCHAR(60) NOT NULL,
  PRIMARY KEY (id)
);
