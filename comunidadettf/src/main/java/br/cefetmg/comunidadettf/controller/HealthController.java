package br.cefetmg.comunidadettf.controller;

import java.util.Map;

import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
public class HealthController {

    @GetMapping({ "/", "/health" })
    public Map<String, Object> health() {
        return Map.of(
                "service", "comunidadettf",
                "status", "online",
                "endpoints", new String[] {
                        "/health",
                        "/auth/register",
                        "/auth/login",
                        "/saves",
                        "/api/v1/usuarios"
                }
        );
    }
}