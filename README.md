# Rugby League: Ultimate Footy 2027 — technical stop

This repository does not contain a game. Work stopped on 25 October 2026 (Australia/Sydney) because the existing project could not be inspected and this environment cannot produce a rugby game at the required standard.

The required standard is a modern mobile 3D rugby league game: full-size players, match laws, tactical AI, momentum-based tackles, cinematic replays, and Android performance presets. A browser prototype with stand-in characters would not meet that standard, so none was built.

## What was inspected

| Check | Result |
| --- | --- |
| This repository | Empty apart from the initial commit. No models, code, Android project, or save data. |
| `C:\Users\Jye\Rugby-League-Ultimate-Footy` | Not present. This session runs on a Linux cloud VM with no Windows drive. |
| Public search | No public repository named Rugby League: Ultimate Footy. |
| Connected Windows machine | A personal worker named `laptop` was online (connected 24 October 2026, 3:16pm Australia/Sydney) and registered no repositories. Subagents launched from this session ran on the cloud VM. They could not be placed on that laptop, so the folder was not read. |

Package identity, player progress, kits, and the current renderer are unknown. Nothing below is a review of that codebase.

## Why a build was not started

1. **The character bar needs an art and motion pipeline this session does not have.** Realistic heads, hands, kits, cloth, muscle deformation, and rugby actions (play-the-ball, tackle variants, kicks, get-up) require original meshes, PBR textures, a shared rig, and a large animation set licensed for a commercial game. Generating or downloading generic people and calling them a rugby cast would miss the brief. Rugby Nations assets cannot be copied.

2. **WebGL is the wrong target for this brief.** A Three.js scene can show a pitch and skinned models. It is not a practical host for 26 deforming players, a stadium, a replay buffer, and mid-range Android GPUs at broadcast quality. Unity URP or Unreal are the engines that can carry that content. Choosing between them requires seeing the current project. Migrating blindly would throw away working match logic if it already exists.

3. **The match is a simulation, not a scene.** Six-tackle sets, play-the-balls, kicks, offside, markers, fatigue, and referee decisions have to be authoritative state. Animation has to follow that state. Context-sensitive crash tackles need impact speed, angle, attributes, and nearby defenders, then a real replay of recorded transforms. Screen shake is not that system. Validating it takes a playable match and device time this session cannot honestly claim.

4. **Android presets need content budgets.** Performance, Balanced, High, and Ultra only mean something after LOD meshes, texture sizes, shadow distance, crowd representation, and a named GPU tier exist. The directive also ended mid-sentence at the Ultra preset, so later requirements were not received.

## What to do next

Run the next session **on the machine that has the folder**, with that folder as the workspace (Cursor desktop opened on `C:\Users\Jye\Rugby-League-Ultimate-Footy`, or a My Machines worker started with `--worker-dir` pointed at that checkout). Then the first deliverable is an audit of the real engine, assets, and licences, not new gameplay.

After that audit, the build order is:

1. Keep package identity, saves, and any rules code that already matches rugby league. Replace the renderer only if the audit shows it cannot host the character pipeline.
2. Lock an original, commercially licensed character: body types, kit shader, numbers, hair, boots, LOD0 for replays and LOD1/LOD2 for the match. No cloth simulation on the full cast on mobile; bone cloth on replay heroes only.
3. License or capture a rugby animation set and retarget it. Minimum: accel, sprint, decelerate, sidestep, fend, short and long pass, dummy, catch, grubber, chip, bomb, punt, play-the-ball, tackles from the front, side, and behind, fall, get-up, limp, celebration, referee signals. Foot and hand IK after the clips exist.
4. Separate the laws from the animator: possession, tackle count, field zones, offside line, knock-on, penalty, clock, score. AI roles read that state.
5. Add crash tackles only when a physics impulse and a recorded replay can show the same collision. Injuries stay within a teen-appropriate presentation: limp, medic, substitution. No graphic trauma.
6. Cameras, broadcast UI, crowd, and graphics presets come after a match is readable. Presets change LOD, shadows, crowd, and resolution scale. They do not change the laws.

Until the existing project is open in the session, there is nothing safe to implement.
