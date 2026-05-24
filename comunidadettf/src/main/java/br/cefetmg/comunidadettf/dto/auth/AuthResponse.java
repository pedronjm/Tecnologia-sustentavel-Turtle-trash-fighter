package br.cefetmg.comunidadettf.dto.auth;

public record AuthResponse(String accessToken, String login, String nome) {
}