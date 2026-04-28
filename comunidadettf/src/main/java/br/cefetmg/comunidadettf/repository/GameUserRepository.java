package br.cefetmg.comunidadettf.repository;

import java.util.Optional;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import br.cefetmg.comunidadettf.model.game.GameUser;

@Repository
public interface GameUserRepository extends JpaRepository<GameUser, Long> {

    Optional<GameUser> findByLoginIgnoreCase(String login);

    boolean existsByLoginIgnoreCase(String login);
}