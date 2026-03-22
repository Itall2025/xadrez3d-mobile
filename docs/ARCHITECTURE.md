# Arquitetura tecnica

## Objetivo
Suportar xadrez 3D mobile com IA multi-nivel e analise pos-jogo, mantendo alta performance em celulares intermediarios.

## Camadas
1. Core de xadrez
- Estado do tabuleiro
- Regras e validacao de lances
- Serializacao para FEN/PGN

2. Gameplay
- Fluxo de turno (usuario vs maquina)
- Relogio opcional
- Estados da partida (inicio, em jogo, fim)

3. IA
- Conector UCI para Stockfish
- Perfis de dificuldade por Elo e tempo de pensamento
- Fallback local para dispositivos fracos

4. Analise
- Classificacao de lances (excelente, bom, imprecisao, erro, blunder)
- Identificacao de viradas
- Resumo por fases (abertura, meio-jogo, final)

5. Apresentacao 3D
- Camera orbital com presets
- Highlights de casas, ultimo lance e ameaças
- Animacoes suaves de movimento/captura/promocao

## Perfis de dificuldade (v1)
- Iniciante: Elo 600, 80 ms por lance
- Casual: Elo 1000, 150 ms por lance
- Intermediario: Elo 1400, 300 ms por lance
- Avancado: Elo 1800, 600 ms por lance
- Pro: Elo 2200, 1200 ms por lance

## Qualidade
- 60 FPS alvo em Android mid-tier
- 100% dos lances legais validados no Core
- Analise pos-jogo em ate 6 segundos para 40 lances
