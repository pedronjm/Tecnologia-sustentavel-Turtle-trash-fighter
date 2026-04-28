package br.cefetmg.comunidadettf.repository;

import java.util.List;
import java.util.Optional;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import br.cefetmg.comunidadettf.model.game.GameSave;

@Repository
public interface GameSaveRepository extends JpaRepository<GameSave, Long> {

    List<GameSave> findByUserIdOrderBySlotIndexAsc(Long userId);

    Optional<GameSave> findByUserIdAndSlotIndex(Long userId, Integer slotIndex);

    long deleteByUserIdAndSlotIndex(Long userId, Integer slotIndex);
}