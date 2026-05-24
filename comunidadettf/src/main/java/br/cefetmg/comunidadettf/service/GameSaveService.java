package br.cefetmg.comunidadettf.service;

import java.time.format.DateTimeFormatter;
import java.util.List;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;

import org.springframework.http.HttpStatus;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;
import org.springframework.web.server.ResponseStatusException;

import br.cefetmg.comunidadettf.dto.save.GameSaveResponse;
import br.cefetmg.comunidadettf.dto.save.GameSaveUpsertRequest;
import br.cefetmg.comunidadettf.model.game.GameSave;
import br.cefetmg.comunidadettf.model.game.GameUser;
import br.cefetmg.comunidadettf.repository.GameSaveRepository;
import br.cefetmg.comunidadettf.repository.GameUserRepository;

@Service
@Transactional
public class GameSaveService {

    private static final int MIN_SLOT_INDEX = 0;
    private static final int MAX_SLOT_INDEX = 4;

    private final GameUserRepository userRepository;
    private final GameSaveRepository saveRepository;
    private final ObjectMapper objectMapper;

    public GameSaveService(GameUserRepository userRepository, GameSaveRepository saveRepository, ObjectMapper objectMapper) {
        this.userRepository = userRepository;
        this.saveRepository = saveRepository;
        this.objectMapper = objectMapper;
    }

    public List<GameSaveResponse> getAll(String login) {
        GameUser user = requireUser(login);
        return saveRepository.findByUserIdOrderBySlotIndexAsc(user.getId())
                .stream()
                .map(this::toResponse)
                .toList();
    }

    public GameSaveResponse getOne(String login, int slotIndex) {
        validateSlotIndex(slotIndex);
        GameUser user = requireUser(login);

        GameSave save = saveRepository.findByUserIdAndSlotIndex(user.getId(), slotIndex)
                .orElseThrow(() -> new ResponseStatusException(HttpStatus.NOT_FOUND, "Save nao encontrado."));

        return toResponse(save);
    }

    public GameSaveResponse upsert(String login, GameSaveUpsertRequest request) {
        validateSlotIndex(request.slotIndex());
        GameUser user = requireUser(login);

        GameSave save = saveRepository.findByUserIdAndSlotIndex(user.getId(), request.slotIndex())
                .orElseGet(GameSave::new);

        save.setUserId(user.getId());
        save.setSlotIndex(request.slotIndex());
        save.setSlotName(normalizeOrDefault(request.slotName(), slotLabel(request.slotIndex())));
        save.setSelectedCharacter(normalizeOrDefault(request.selectedCharacter(), "Warrior"));
        save.setPlayTutorial(request.playTutorial());
        save.setDifficulty(normalizeOrDefault(request.difficulty(), "Normal"));
        save.setSceneName(normalizeOrDefault(request.sceneName(), "SampleScene"));
        save.setCheckpointId(normalizeOrDefault(request.checkpointId(), ""));
        save.setCheckpointX(request.checkpointX());
        save.setCheckpointY(request.checkpointY());
        save.setCheckpointZ(request.checkpointZ());
        save.setCollectedIdsJson(toJsonArray(request.collectedIds()));
        save.setDeadEnemyIdsJson(toJsonArray(request.deadEnemyIds()));
        save.setCompletionPercent(request.completionPercent());

        return toResponse(saveRepository.save(save));
    }

    public boolean delete(String login, int slotIndex) {
        validateSlotIndex(slotIndex);
        GameUser user = requireUser(login);
        return saveRepository.deleteByUserIdAndSlotIndex(user.getId(), slotIndex) > 0;
    }

    private GameUser requireUser(String login) {
        String normalizedLogin = normalize(login);
        if (normalizedLogin.isEmpty()) {
            throw new ResponseStatusException(HttpStatus.UNAUTHORIZED, "Usuario nao autenticado.");
        }

        return userRepository.findByLoginIgnoreCase(normalizedLogin)
                .orElseThrow(() -> new ResponseStatusException(HttpStatus.UNAUTHORIZED, "Usuario nao autenticado."));
    }

    private void validateSlotIndex(int slotIndex) {
        if (slotIndex < MIN_SLOT_INDEX || slotIndex > MAX_SLOT_INDEX) {
            throw new ResponseStatusException(HttpStatus.BAD_REQUEST, "Slot invalido. Use um valor entre 0 e 4.");
        }
    }

    private GameSaveResponse toResponse(GameSave save) {
        return new GameSaveResponse(
                save.getSlotIndex(),
                save.getSlotName(),
                save.getSelectedCharacter(),
                Boolean.TRUE.equals(save.getPlayTutorial()),
                save.getDifficulty(),
                save.getSceneName(),
                save.getCheckpointId(),
                save.getCheckpointX(),
                save.getCheckpointY(),
                save.getCheckpointZ(),
                save.getCollectedIdsJson(),
                save.getDeadEnemyIdsJson(),
                save.getCompletionPercent(),
                save.getLastSavedAtUtc() == null
                        ? null
                        : save.getLastSavedAtUtc().format(DateTimeFormatter.ISO_LOCAL_DATE_TIME)
        );
    }

    private String normalize(String value) {
        return value == null ? "" : value.trim();
    }

    private String normalizeOrDefault(String value, String defaultValue) {
        String normalized = normalize(value);
        return normalized.isEmpty() ? defaultValue : normalized;
    }

    private String slotLabel(int slotIndex) {
        return "Slot " + (slotIndex + 1);
    }

    private String toJsonArray(List<String> values) {
        try {
            return objectMapper.writeValueAsString(values == null ? List.of() : values);
        } catch (JsonProcessingException exception) {
            throw new ResponseStatusException(HttpStatus.INTERNAL_SERVER_ERROR, "Falha ao serializar dados do save.", exception);
        }
    }
}