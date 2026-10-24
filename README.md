# Ultimate Footy

Rugby league match laws for a Unity URP game. The graphics scene is not in this repository yet.

## What ran

`dotnet test` on 25 October 2026: **31 passed, 0 failed**.

```bash
dotnet test UltimateFooty.sln
```

The tests cover the laws kernel in `src/UltimateFooty.Match`. They do not open Unity, render a player, or build an Android app.

## Graphics milestone: blocked

The first milestone is a Unity URP scene with full-size players, a pitch, a tackle, and a real render. That scene was not created.

| Check | Result |
| --- | --- |
| `C:\Users\Jye\Rugby-League-Ultimate-Footy` | Not on this machine. This session is a Linux cloud VM. |
| Unity Editor | Not installed here. The laptop worker named `laptop` was connected and registered no repositories. Commands from this session cannot run on that laptop, so the Unity version, Android module, GPU, and project files were not read. |
| GPU on this VM | None. No `/dev/dri` and no NVIDIA device. A batch render would be software-only even after an editor install. |
| Unity licence | Activating the editor needs the Unity account on the laptop. No licence file is available here. |
| Player meshes | None in this repository. A capsule or an untextured mannequin would miss the required standard, so neither was added. |
| Play Console | Not opened. Package `com.ultimatefooty.national` and the Lounge Lizard account were not verified, and nothing was uploaded. |

Open the next session on the laptop, with `C:\Users\Jye\Rugby-League-Ultimate-Footy` as the workspace, before any URP scene or signed bundle.

## Laws that are implemented

The kernel follows the International Laws of the Game text published by the NRL, plus the ARLC changes reported on 4 February 2026. Kick success is supplied by the caller. There is no ball-flight simulation.

- Try 4, conversion 2, penalty goal 2, drop goal 1. The goal line is in-goal. Touch-in-goal is not a try.
- After points, the team that conceded them kicks off.
- Five completed tackles, then a sixth-tackle handover. The handover is not another tackle.
- A tackle break does not advance the count.
- Field knock-on or forward pass: scrum, and the receiving team's next tackle is a zero tackle.
- Attacking knock-on in-goal: possession changes and no seven-tackle set.
- Held up in-goal: play-the-ball 10 m out, unless it is the last tackle, which is a handover.
- High tackle uses contact height and closing speed. A chest tackle is never penalised. Strictness changes sin-bin and send-off thresholds.
- Play-the-ball infringement inside 20 m of the attacking goal line is a penalty. Further out, the set restarts.
- Halves are 40 minutes and do not end while the ball is live.
- Sin bin is 10 minutes.
- Interchange: 13 on the field, at most four of six reserves, at most eight swaps.
- Attribute upgrades spend experience and stop at 99.
- Running lanes are gaps between current defender positions. They close when a defender fills them.

Movement, animation, cameras, audio, AI positioning, and Android packaging are not in this library.
