
<html>
    <p align="center">
    <img src="https://orig00.deviantart.net/d9d0/f/2018/108/1/e/logostormiumlongsmall_by_guerro323-dc972av.png" alt="Super Logo!" width="195" height="64" />
    </p>
    <h2 align="center">
    Stormium
    </h2>
</html>

___
### Packages:
This game (also Patapon 4) use a package system, for a better code maintenance.

-   [Core package](GameClient/Packages/pack.st.core)
-   [Default package](GameClient/Packages/pack.st.default)
-   [Shared package](GameClient/Packages/pack.guerro.shared)

Version: 1

___

### Game folders (exemple with my two games):
```
/Projects/
├── Common/
│   ├── Packages/
    │   ├── package.guerro.shared/
    │   ├── package.stormium.core/
    │   ├── package.patapon.core/

├── STORMIUM/
│   ├── <ProjectStormiumUnity>/
    │   ├── StormiumGameClient/
        │   ├── Packages/
            │   ├── manifest.json
│   ├── (related files and folders...)

├── Patapon/
│   ├── <ProjectPatapon4>/
    │   ├── Patapon4GameClient/
        │   ├── Packages/
            │   ├── manifest.json
│   ├── (related files and folders...)
```

`manifest.json`
```json
{
    "dependencies":
    {
        "package.guerro.shared": "file:../../../../Common/Packages/package.guerro.shared",
        "package.<game>.core": "file:../../../../Common/Packages/package.<game>.core",
        "other.packages": "..."
    }
}
```

### Game structure:
```
/<Game>

-- Private access
├── Internal/
│   ├── Packages/
    │   ├── <package.Game.core>/
    │   ├── package.guerro.shared/ -- Should this package be put into the game core instead?
│   ├── Assets/
    │   ├── <GameAssets>
    │   ├── StreamingAssets/
        │   ├── Mods/
            │   ├── <Mod1>/
                │   ├── Config/
                │   ├── Inputs/
                │   ├── ...

-- Public access
├── WorkSpace/
│   ├── Mods/
    │   ├── <Mod1>/
        -- Will override the configs from streamingassets folder
        │   ├── Config/
        │   ├── Inputs
        │   ├── ...
```