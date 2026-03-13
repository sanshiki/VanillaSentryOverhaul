# VanillaSentryExpansion

A **Terraria tModLoader** mod that expands Summoner content: sentries, flag weapons, summon overrides, and accessories.

## Features

- **Sentry staffs** — Custom sentries (e.g. Bunny, Dark Magic Tower, Rocket, Temple, Stardust) and related projectiles.
- **Flag weapons** — Flag-based weapons (Normal, Holy, Hell, One True, Tiki, Pirate, Santa, Goblin, Giant Leaves of Plantera, etc.) with anchors and blade shots.
- **Overrides** — Vanilla sentry overrides (Ballista, Flameburst Tower, Frost Hydra, Queen Spider, Moon Lord Turret, etc.) and optional armor/summon staff tweaks.
- **Accessories & armor** — Sentry anchor, Sentinel Talisman tiers, Super Earth Cloak, Spooky Skull, Tiki Visage, and vanilla armor overrides.
- **Extras** — Summon fatigue, dynamic param UI, hitbox drawer, localization (en-US, zh-Hans), and optional Calamity adapter.

## Requirements

- [tModLoader](https://store.steampowered.com/app/1281930/tModLoader/) for Terraria.

## Building

Build the mod inside tModLoader (Developer Mode → Build Mod). Do not use `dotnet build` for the final mod; use the tModLoader build pipeline.

## Project layout

- **SummonerExpansionMod.cs** — Mod entry; registers keys and calls `ModIDLoader.Load()`.
- **Initialization/** — ID loading, `ModGlobal`, `UILoader`.
- **ModUtils/** — Minion AI helper, config, state machine, animation, dynamic params, etc.
- **Content/** — Buffs, Items (accessories, armors, weapons), Projectiles (sentries, bullets, flags), NPCs, Players, Dusts, Systems, UI.
- **Assets/** — Textures, sounds.
- **Localization/** — en-US and zh-Hans `.hjson` files.
- **Scripts/** — Python scripts for assets/tests (separate from C# build).

## Author

Sanshiki

## License

See repository or author for license terms.
