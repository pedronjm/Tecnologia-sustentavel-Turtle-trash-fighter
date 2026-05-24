package br.cefetmg.comunidadettf.model.game;

import java.time.LocalDateTime;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.PrePersist;
import jakarta.persistence.PreUpdate;
import jakarta.persistence.Table;
import jakarta.persistence.UniqueConstraint;
import lombok.AllArgsConstructor;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Getter
@Setter
@NoArgsConstructor
@AllArgsConstructor
@Entity
@Table(
        name = "game_saves",
        uniqueConstraints = @UniqueConstraint(name = "uq_game_saves_user_slot", columnNames = { "user_id", "slot_index" })
)
public class GameSave {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @Column(name = "user_id", nullable = false)
    private Long userId;

    @Column(name = "slot_index", nullable = false)
    private Integer slotIndex;

    @Column(name = "slot_name", length = 120, nullable = false)
    private String slotName;

    @Column(name = "selected_character", length = 40, nullable = false)
    private String selectedCharacter;

    @Column(name = "play_tutorial", nullable = false)
    private Boolean playTutorial;

    @Column(length = 40, nullable = false)
    private String difficulty;

    @Column(name = "scene_name", length = 120, nullable = false)
    private String sceneName;

    @Column(name = "checkpoint_id", length = 120, nullable = false)
    private String checkpointId;

    @Column(name = "checkpoint_x", nullable = false)
    private Float checkpointX;

    @Column(name = "checkpoint_y", nullable = false)
    private Float checkpointY;

    @Column(name = "checkpoint_z", nullable = false)
    private Float checkpointZ;

    @Column(name = "collected_ids_json", columnDefinition = "JSON", nullable = false)
    private String collectedIdsJson;

    @Column(name = "dead_enemy_ids_json", columnDefinition = "JSON", nullable = false)
    private String deadEnemyIdsJson;

    @Column(name = "completion_percent", nullable = false)
    private Float completionPercent;

    @Column(name = "last_saved_at_utc", nullable = false)
    private LocalDateTime lastSavedAtUtc;

    @PrePersist
    @PreUpdate
    public void touch() {
        lastSavedAtUtc = LocalDateTime.now(java.time.ZoneOffset.UTC);
    }
}