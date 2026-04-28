package br.cefetmg.comunidadettf.model.game;

import java.time.LocalDateTime;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.PrePersist;
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
        name = "game_users",
        uniqueConstraints = @UniqueConstraint(name = "uq_game_users_login", columnNames = "login")
)
public class GameUser {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @Column(length = 80, nullable = false, unique = true)
    private String login;

    @Column(name = "password_hash", length = 255, nullable = false)
    private String passwordHash;

    @Column(length = 120, nullable = false)
    private String nome;

    @Column(name = "created_at_utc", nullable = false)
    private LocalDateTime createdAtUtc;

    @PrePersist
    public void prePersist() {
        if (createdAtUtc == null) {
            createdAtUtc = LocalDateTime.now(java.time.ZoneOffset.UTC);
        }
    }
}