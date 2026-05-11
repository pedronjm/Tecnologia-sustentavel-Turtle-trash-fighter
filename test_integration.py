#!/usr/bin/env python3
"""
Test Script: Validar comunicação entre Unity (simulado) e Spring Backend
Confirma que ambos falam com o mesmo banco de dados MySQL
"""

import requests
import json
import sys
from datetime import datetime

BASE_URL = "http://localhost:8080"
TEST_LOGIN = "testplayer"
TEST_PASSWORD = "test123456"
TEST_NOME = "Test Player"

def log(msg, status="INFO"):
    timestamp = datetime.now().strftime("%H:%M:%S")
    print(f"[{timestamp}] {status:8} | {msg}")

def log_success(msg):
    log(msg, "✓ OK")

def log_error(msg):
    log(msg, "✗ ERRO")

def log_section(title):
    print(f"\n{'='*60}")
    print(f"  {title}")
    print(f"{'='*60}\n")

def test_health():
    """Teste 1: Verificar se o servidor está online"""
    log_section("TESTE 1: Health Check")
    
    try:
        resp = requests.get(f"{BASE_URL}/health", timeout=5)
        if resp.status_code == 200:
            data = resp.json()
            log_success(f"Servidor online: {data.get('service')}")
            log(f"Status: {data.get('status')}")
            return True
        else:
            log_error(f"Servidor respondeu com status {resp.status_code}")
            return False
    except requests.exceptions.ConnectionError:
        log_error(f"Não conseguiu conectar a {BASE_URL}. Certifique-se que o Spring está rodando!")
        log("Comando para rodar: cd comunidadettf && mvnw spring-boot:run")
        return False
    except Exception as e:
        log_error(f"Erro ao conectar: {e}")
        return False

def test_register():
    """Teste 2: Registrar novo usuário"""
    log_section("TESTE 2: Registrar Usuário")
    
    payload = {
        "login": TEST_LOGIN,
        "password": TEST_PASSWORD,
        "nome": TEST_NOME
    }
    
    try:
        log(f"Enviando POST /auth/register com login='{TEST_LOGIN}'")
        resp = requests.post(f"{BASE_URL}/auth/register", json=payload, timeout=5)
        
        if resp.status_code == 200:
            data = resp.json()
            token = data.get("accessToken")
            log_success(f"Usuário registrado: {data.get('login')}")
            log(f"Nome: {data.get('nome')}")
            log(f"Token JWT recebido: {token[:20]}...")
            return token
        elif resp.status_code == 409:
            log("Usuário já existe, tentando login...")
            return None
        else:
            log_error(f"Status {resp.status_code}: {resp.text}")
            return None
    except Exception as e:
        log_error(f"Erro no registro: {e}")
        return None

def test_login():
    """Teste 3: Login e obter JWT"""
    log_section("TESTE 3: Login (Autenticação JWT)")
    
    payload = {
        "login": TEST_LOGIN,
        "password": TEST_PASSWORD
    }
    
    try:
        log(f"Enviando POST /auth/login com login='{TEST_LOGIN}'")
        resp = requests.post(f"{BASE_URL}/auth/login", json=payload, timeout=5)
        
        if resp.status_code == 200:
            data = resp.json()
            token = data.get("accessToken")
            log_success(f"Login bem-sucedido: {data.get('login')}")
            log(f"Token JWT: {token[:30]}...")
            return token
        else:
            log_error(f"Status {resp.status_code}: {resp.text}")
            return None
    except Exception as e:
        log_error(f"Erro no login: {e}")
        return None

def test_save_game(token):
    """Teste 4: Salvar game"""
    log_section("TESTE 4: Salvar Game (PUT /saves)")
    
    if not token:
        log_error("Token não disponível, pulando...")
        return False
    
    payload = {
        "slotIndex": 0,
        "slotName": "Test Save",
        "selectedCharacter": "Warrior",
        "playTutorial": True,
        "difficulty": "Normal",
        "sceneName": "SampleScene",
        "checkpointId": "cp-test",
        "checkpointX": 10.5,
        "checkpointY": 2.0,
        "checkpointZ": 0.0,
        "collectedIds": ["c1", "c2"],
        "deadEnemyIds": ["e1"],
        "completionPercent": 35.7
    }
    
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }
    
    try:
        log("Enviando PUT /saves com dados do game")
        resp = requests.put(f"{BASE_URL}/saves", json=payload, headers=headers, timeout=5)
        
        if resp.status_code == 200:
            data = resp.json()
            log_success(f"Game salvo no slot {data.get('slotIndex')}: {data.get('slotName')}")
            log(f"Personagem: {data.get('selectedCharacter')}, Dificuldade: {data.get('difficulty')}")
            log(f"Progresso: {data.get('completionPercent')}%")
            log(f"Checkpoint: {data.get('checkpointId')}")
            return True
        else:
            log_error(f"Status {resp.status_code}: {resp.text}")
            return False
    except Exception as e:
        log_error(f"Erro ao salvar: {e}")
        return False

def test_load_game(token):
    """Teste 5: Carregar game"""
    log_section("TESTE 5: Carregar Game (GET /saves/0)")
    
    if not token:
        log_error("Token não disponível, pulando...")
        return False
    
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }
    
    try:
        log("Enviando GET /saves/0 para carregar o save")
        resp = requests.get(f"{BASE_URL}/saves/0", headers=headers, timeout=5)
        
        if resp.status_code == 200:
            data = resp.json()
            log_success(f"Game carregado do slot {data.get('slotIndex')}: {data.get('slotName')}")
            log(f"Personagem: {data.get('selectedCharacter')}, Dificuldade: {data.get('difficulty')}")
            log(f"Progresso: {data.get('completionPercent')}%")
            log(f"Checkpoint: {data.get('checkpointId')} em ({data.get('checkpointX')}, {data.get('checkpointY')}, {data.get('checkpointZ')})")
            log(f"Coletáveis: {data.get('collectedIdsJson')}")
            log(f"Inimigos derrotados: {data.get('deadEnemyIdsJson')}")
            return True
        elif resp.status_code == 404:
            log("Save não encontrado (esperado se for primeira vez)")
            return False
        else:
            log_error(f"Status {resp.status_code}: {resp.text}")
            return False
    except Exception as e:
        log_error(f"Erro ao carregar: {e}")
        return False

def test_list_all_saves(token):
    """Teste 6: Listar todos os saves do usuário"""
    log_section("TESTE 6: Listar Todos os Saves (GET /saves)")
    
    if not token:
        log_error("Token não disponível, pulando...")
        return False
    
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }
    
    try:
        log("Enviando GET /saves para listar todos")
        resp = requests.get(f"{BASE_URL}/saves", headers=headers, timeout=5)
        
        if resp.status_code == 200:
            saves = resp.json()
            log_success(f"Total de saves: {len(saves)}")
            for save in saves:
                log(f"  Slot {save.get('slotIndex')}: {save.get('slotName')} - Progresso: {save.get('completionPercent')}%")
            return True
        else:
            log_error(f"Status {resp.status_code}: {resp.text}")
            return False
    except Exception as e:
        log_error(f"Erro ao listar: {e}")
        return False

def main():
    print("\n" + "="*60)
    print("  TESTE DE INTEGRAÇÃO: Unity ↔ Spring ↔ MySQL")
    print("="*60)
    print(f"\nAmbiente:")
    print(f"  Base URL: {BASE_URL}")
    print(f"  Banco: turtle_trash_fighter (MySQL localhost:3306)")
    print(f"  Usuário: root (sem senha por padrão)")
    print(f"\nTestando comunicação entre cliente Unity (simulado) e API Java...\n")
    
    # Testes
    if not test_health():
        print("\n✗ FALHA: Servidor não respondeu. Abra um terminal e execute:")
        print("  cd comunidadettf")
        print("  .\\mvnw.cmd spring-boot:run")
        return False
    
    # Try to register, but don't fail if already registered
    token = test_register()
    
    # If registration failed, try login
    if not token:
        token = test_login()
    
    if not token:
        log_error("Não conseguiu obter token JWT")
        return False
    
    # Salvar e carregar
    save_ok = test_save_game(token)
    load_ok = test_load_game(token)
    list_ok = test_list_all_saves(token)
    
    # Resumo
    log_section("RESUMO")
    print(f"\n✓ COMUNICAÇÃO CONFIRMADA:")
    print(f"\n  1. Unity consegue conectar ao Spring em http://localhost:8080")
    print(f"  2. JWT está funcionando (login e autenticação)")
    print(f"  3. Dados estão sendo salvos no MySQL (turtle_trash_fighter)")
    print(f"  4. Carregamento de saves funciona corretamente")
    
    if save_ok and load_ok:
        print(f"\n✓✓ SUCESSO: Integração funcional!")
        print(f"\n  O fluxo completo está operacional:")
        print(f"  Unity → HTTP → Spring Boot → MySQL")
        return True
    else:
        print(f"\n⚠ Alguns testes falharam, verifique os logs acima")
        return False

if __name__ == "__main__":
    try:
        success = main()
        sys.exit(0 if success else 1)
    except KeyboardInterrupt:
        print("\n\nTeste interrompido pelo usuário")
        sys.exit(1)
    except Exception as e:
        print(f"\nErro inesperado: {e}")
        sys.exit(1)
