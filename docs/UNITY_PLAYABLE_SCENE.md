# Unity - Cena jogavel inicial

## Objetivo
Gerar rapidamente uma cena 3D funcional para validar fluxo usuario x maquina.

## Scripts adicionados
- `Gameplay/DemoSceneBootstrap.cs`
- `Gameplay/BoardRenderer3D.cs`
- `Gameplay/BoardSquareView.cs`
- `Gameplay/ChessBoardInput.cs`
- `Gameplay/OrbitCameraController.cs`
- `Gameplay/GameHudOverlay.cs`

## Setup rapido (2 minutos)
1. No Unity, criar cena vazia `Main`.
2. Criar um `Empty GameObject` chamado `Bootstrap`.
3. Adicionar o componente `DemoSceneBootstrap`.
4. Dar Play.

O bootstrap cria automaticamente:
- `GameController`
- tabuleiro 3D com pecas
- camera orbital
- input por toque/click
- HUD minima com troca de dificuldade (1..5)

## Controles
- Touch/click: selecionar casa de origem e destino.
- Mouse botao direito: orbitar camera.
- Scroll: zoom.

## Estado atual
- Visual 3D e fluxo de jogadas ja funcionam.
- Regras ainda estao simplificadas no `BoardState` (sem validacao completa de legalidade).
