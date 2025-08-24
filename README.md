# Multiplayer Third Person Controller — Mirror

## English

### Overview

This project is a multiplayer third person controller built with Unity 2022.3, [Mirror Networking](https://mirror-networking.com/), and the  
[EasyStart Third Person Controller](https://assetstore.unity.com/packages/tools/game-toolkits/easystart-third-person-controller-278977) asset.

**Features:**
- Players can enter or receive a nickname + ID on startup.
- Nicknames are shown above each player's head and synced for all clients.
- Each player controls only their own third person character (using EasyStart Third Person Controller).
- WASD — Move, Space — Jump, Left Shift — Sprint, Left Ctrl — Crouch.
- F — Spawn Cube (networked, visible to all)
- H — Say Hello! (message bubble above player visible to all)
- C — Unlock Cursor

### Project Structure

- `Assets/Scripts/`
  - `PlayerNetworkController.cs` — Handles networking, nickname sync, and player control.
  - `PlayerNicknameInputUI.cs` — Nickname input and validation.
  - `PlayerNicknameManager.cs` — Tracks all players for management.
  - `BubbleMessageUI.cs` — Nickname/message display above player.
  - Other utility/network scripts as needed.
- `Assets/Prefabs/`
  - `Player.prefab` — Networked third person character with UI and animator.
  - `Cube.prefab` — Network-spawned cube with Rigidbody.
- `Assets/Animations/` — (Optional) Idle, Run animations.

**Used Asset:**  
- [EasyStart Third Person Controller](https://assetstore.unity.com/packages/tools/game-toolkits/easystart-third-person-controller-278977)

---

## Русский

### Описание

Данный проект — мультиплеерный контроллер от третьего лица на Unity 2022.3 с использованием [Mirror Networking](https://mirror-networking.com/)  
и ассета [EasyStart Third Person Controller](https://assetstore.unity.com/packages/tools/game-toolkits/easystart-third-person-controller-278977).

**Функции:**
- Ввод или случайная генерация ника + ID при запуске.
- Никнеймы отображаются над персонажами и синхронизированы между всеми клиентами.
- Каждый игрок управляет только своим персонажем (EasyStart Third Person Controller).
- WASD — Движение, Space — Прыжок, Shift — Бег, Ctrl — Присесть.
- F — Спавн куба (виден всем)
- H — Сказать привет! (пузырь сообщения над игроком виден всем)
- C — Открыть курсор

### Структура проекта

- `Assets/Scripts/`
  - `PlayerNetworkController.cs` — Сетевое управление, синхронизация ника, управление игроком.
  - `PlayerNicknameInputUI.cs` — UI для ввода ника и проверки уникальности.
  - `PlayerNicknameManager.cs` — Отслеживание всех игроков.
  - `BubbleMessageUI.cs` — Отображение ника/сообщения над персонажем.
  - Другие вспомогательные скрипты.
- `Assets/Prefabs/`
  - `Player.prefab` — Сетевой персонаж с UI и аниматором.
  - `Cube.prefab` — Сетевой куб с Rigidbody.
- `Assets/Animations/` — (Опционально) Анимации Idle, Run.

**Используемый ассет:**  
- [EasyStart Third Person Controller](https://assetstore.unity.com/packages/tools/game-toolkits/easystart-third-person-controller-278977)

---
<img width="1919" height="1049" alt="image" src="https://github.com/user-attachments/assets/3e6582ac-45fb-4b74-88ae-d6d9008910f4" />
