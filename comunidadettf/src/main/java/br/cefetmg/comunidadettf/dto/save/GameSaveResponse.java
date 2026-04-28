package br.cefetmg.comunidadettf.dto.save;

public record GameSaveResponse(
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
        String collectedIdsJson,
        String deadEnemyIdsJson,
        float completionPercent,
        String lastSavedAtUtc
) {
}