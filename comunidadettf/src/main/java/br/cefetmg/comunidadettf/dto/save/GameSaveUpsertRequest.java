package br.cefetmg.comunidadettf.dto.save;

import java.util.List;

public record GameSaveUpsertRequest(
        Integer slotIndex,
        String slotName,
        String selectedCharacter,
        boolean playTutorial,
        String difficulty,
        String sceneName,
        String checkpointId,
        float checkpointX,
        float checkpointY,
        float checkpointZ,
        List<String> collectedIds,
        List<String> deadEnemyIds,
        float completionPercent
) {
}