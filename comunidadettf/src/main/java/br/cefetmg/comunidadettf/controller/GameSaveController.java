package br.cefetmg.comunidadettf.controller;

import java.security.Principal;
import java.util.List;

import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.DeleteMapping;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PutMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import br.cefetmg.comunidadettf.dto.save.GameSaveResponse;
import br.cefetmg.comunidadettf.dto.save.GameSaveUpsertRequest;
import br.cefetmg.comunidadettf.service.GameSaveService;

@RestController
@RequestMapping("/saves")
public class GameSaveController {

    private final GameSaveService saveService;

    public GameSaveController(GameSaveService saveService) {
        this.saveService = saveService;
    }

    @GetMapping
    public List<GameSaveResponse> getAll(Principal principal) {
        return saveService.getAll(principal.getName());
    }

    @GetMapping("/{slotIndex}")
    public GameSaveResponse getOne(Principal principal, @PathVariable int slotIndex) {
        return saveService.getOne(principal.getName(), slotIndex);
    }

    @PutMapping
    public GameSaveResponse upsert(Principal principal, @RequestBody GameSaveUpsertRequest request) {
        return saveService.upsert(principal.getName(), request);
    }

    @DeleteMapping("/{slotIndex}")
    public ResponseEntity<Void> delete(Principal principal, @PathVariable int slotIndex) {
        return saveService.delete(principal.getName(), slotIndex)
                ? ResponseEntity.noContent().build()
                : ResponseEntity.notFound().build();
    }
}