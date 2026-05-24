package br.cefetmg.comunidadettf.dto.auth;

public record RegisterRequest(String login, String password, String nome) {
}