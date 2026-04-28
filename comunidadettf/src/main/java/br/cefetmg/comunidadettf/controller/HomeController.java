package br.cefetmg.comunidadettf.controller;

import org.springframework.web.bind.annotation.RestController;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.GetMapping;

@RestController
   @RequestMapping("/home")
   
public class HomeController {
  @GetMapping("")
   public String gethome() {
       return "Bem-vindo à Comunidade TTF!";
   }
}
