# simulator | Poker table simulator

**Part of the PokerFill platform** · five repositories, one per component · [project overview](https://github.com/biresevit#pokerfill)

> **Permitted use:** These bots may only be used for research, poker practice in the [simulator](https://github.com/biresevit/simulator-portfolio), poker practice through a real poker-site client, or authorized load testing to assess a poker site's stability under high traffic. Running bots at real-money tables without disclosing their use to all human players is prohibited.

> Public portfolio snapshot with a fresh history. Deployment credentials are removed; configuration placeholders require your own services. Binary dependencies and generated PDF guides are omitted.

A Windows desktop poker table simulator. It deals the hands, runs the betting, and asks PDecision over REST for the bots' decisions (it can also call the decision library in process). The strategy, hand-history, and statistics libraries it works with are kept in a private repository and are not part of this snapshot.

![The simulator table: six bot seats on a 7-6-4 flop, one bot betting](images/simulator.png)

## Demo

The simulator (left) plays a six-handed table of bots. Each time a bot is to act, the simulator sends the table state to [PDecision](https://github.com/biresevit/PDecision-portfolio) over REST; the PDecision window (right) logs every decision it returns, and the simulator plays it at the table.

![Simulator playing hands while the PDecision window logs each decision it returns](images/simulator_demo.gif)

[Full-resolution video (MP4, 38 s)](images/simulator_demo.mp4)

## Engineering highlights

- A WPF table simulator for inspecting game state and exercising automated decisions.
- A REST client that sends table snapshots to the PDecision service and plays the decisions it returns.
- Human and bot seats, pause/resume, and fast execution for long unattended runs.
- Regression workflows comparing simulated actions with the hand histories PDecision records.

**Technology:** C#, .NET 8, WPF, REST/JSON, NUnit.

## Repository map

| Directory | Responsibility |
| --- | --- |
| `PokerTable/PokerTable/` | Desktop simulator and REST integration |
| `PokerTable/PokerTableNUnitTests/` | Unit tests |
| `images/` | Screenshot and demo video |

The strategy and data libraries (`DecisionMaking`, `HandHistoryLib`, `DBLib`, `PokerUtil`, `QueryDBLib`, `RulesLib`) are private. `PokerTable.csproj` and `PokerTable.sln` still reference `DecisionMaking` at its original relative path.

## Platform context

```text
PokerTable -- REST --> PDecision --> DecisionMaking (private)
```

[PokerBackoffice](https://github.com/biresevit/PokerBackoffice) | [ScheduleController](https://github.com/biresevit/ScheduleController) | [PokerController](https://github.com/biresevit/PokerController) | [PDecision](https://github.com/biresevit/PDecision-portfolio) | [simulator](https://github.com/biresevit/simulator-portfolio)

The web operations interface lives in PokerBackoffice.

## Building

This snapshot is published for code review and does not build on its own: PokerTable references the private `DecisionMaking` project and legacy WPF toolkit DLLs that are not included. In the original environment it builds on Windows with the .NET 8 SDK (`net8.0-windows`, WPF):

```powershell
dotnet build PokerTable/PokerTable/PokerTable.csproj -c Release
```

Running it also requires a PDecision service with its strategy resources, SQL/LocalDB, and external solvers.

## Simulation and validation

For REST integration, check out PDecision beside this repository and follow its simulator instructions. Start PDecision and wait for its health endpoint to report ready. PokerTable supports human and bot seats, pause/resume, fast execution, and switching between REST and in-process decisions.

PDecision's `tests/check_simulator_hh.py` compares recorded hand histories with the simulator's expected action trace. These integration checks require configured runtime resources.

## Configuration

Supply deployment credentials through local environment or secret configuration. Public portfolio copies contain configuration placeholders and require your own services and resources.

## Portfolio validation

Source scanning passed. PokerTable cannot build from this snapshot without the private `DecisionMaking` project and the omitted legacy WPF toolkit DLLs, including System.Windows.Controls.DataVisualization.Toolkit. Private packages and runtime resources are not bundled.
