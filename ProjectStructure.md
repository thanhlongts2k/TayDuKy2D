# Project Structure

## tayduky-client
```
 tayduky-client/
 ├─ Assets/
 │  ├─ Editor/                # Automation scripts (SetupTestScene.cs, SpriteSlicer.cs)
 │  ├─ Resources/             # JSON configs & map graphics
 │  │   ├─ Maps/
 │  │   │   ├─ maps.json
 │  │   │   ├─ dao_tri.png
 │  │   │   └─ hoi_ban_dao.png
 │  │   ├─ Companions/
 │  │   ├─ Items/
 │  │   └─ NPCs/
 │  ├─ Sprites/               # Character & companion sprite sheets
 │  │   ├─ Characters/         # than_toc_sheet.png, ma_toc_sheet.png, yeu_toc_sheet.png
 │  │   └─ Companions/         # Pet graphics
 │  └─ Scripts/
 │       ├─ Controllers/      # PlayerController.cs, PetController.cs
 │       ├─ Managers/         # MapManager.cs, QuestManager.cs, CombatManager.cs, MountAndPetManager.cs, ConfigManager.cs, **PetSystemManager.cs** (placeholder)
 │       ├─ Network/          # NetworkClient.cs
 │       └─ UI/               # UIManager.cs, LoginManager.cs, CharacterCreationManager.cs
```

## tayduky-server
```
 tayduky-server/
 ├─ cmd/server/main.go
 ├─ internal/
 │   ├─ database/            # PostgreSQL & Redis helpers
 │   ├─ models/               # character.go, quest.go, companion.go
 │   └─ gameplay/            # move/, combat/, quest/, chat/
 └─ config/                  # maps.json, quests.json
```
```
The above reflects the architecture defined in the Code Architecture Blueprint.
```
