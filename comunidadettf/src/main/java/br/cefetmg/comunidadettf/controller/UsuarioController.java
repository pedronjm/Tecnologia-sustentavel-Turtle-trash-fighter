package br.cefetmg.comunidadettf.controller;

import org.springframework.web.bind.annotation.RestController;

import org.springframework.web.server.ResponseStatusException;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.http.HttpStatus;

import org.springframework.web.bind.annotation.DeleteMapping;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.PutMapping;
import org.springframework.web.bind.annotation.RequestBody;

import br.cefetmg.comunidadettf.model.Usuario;
import br.cefetmg.comunidadettf.repository.UsuarioRepository;

import java.util.List;

@RestController
@RequestMapping("/api/v1/usuarios")
public class UsuarioController {

    private final UsuarioRepository repository;

    public UsuarioController(UsuarioRepository repository) {
        this.repository = repository;
    }

    @GetMapping("")
    public List<Usuario> getAll() {
        return repository.findAll();
    }

    @GetMapping("/{id}")
    public Usuario getUsuarioById(@PathVariable Long id) {
        return repository.findById(id).orElse(null);
    }

    @PostMapping("")
    public Usuario inserir(@RequestBody Usuario usuario) {
        usuario.setId(null);
        return repository.save(usuario);

    }

    @DeleteMapping("/{id}")
    public Usuario excluir(@PathVariable Long id) {
        Usuario usuario = repository.findById(id).orElse(null);
        if (usuario != null) {
            repository.deleteById(id);
        }else {
           throw new ResponseStatusException(HttpStatus.NOT_FOUND, "Usuário não encontrado para exclusão");  
        }
        return usuario;
    }

    @PutMapping("")
    public Usuario atualizar(@RequestBody Usuario usuario) {
        if (usuario.getId() == null || !repository.existsById(usuario.getId())) {
            return null;
        }
        
        return repository.save(usuario);
    }
}
