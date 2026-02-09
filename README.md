# Gomoku AI

A cross-platform Gomoku (Five-in-a-Row) game with AI opponent, built with C# and Avalonia. The AI uses Minimax algorithm with Alpha-Beta pruning for intelligent decision-making.

![Gomoku](https://img.shields.io/badge/Game-Gomoku-blue)
![C#](https://img.shields.io/badge/Language-C%23-green)
![Avalonia](https://img.shields.io/badge/Framework-Avalonia-purple)
![.NET 8](https://img.shields.io/badge/.NET-8.0-orange)
![Cross-Platform](https://img.shields.io/badge/Platform-macOS%20|%20Linux%20|%20Windows-brightgreen)

## 🎮 Features

- **15×15 Standard Board** - Classic Gomoku board
- **Human vs AI Mode** - Player plays Black (first), AI plays White
- **Intelligent AI** - Based on Minimax algorithm with Alpha-Beta pruning
- **Real-time Win Detection** - Four-direction consecutive check
- **Undo Function** - Support for move reversal
- **Move History** - Complete record of each move
- **Modern UI Design** - Dark theme, clean interface
- **Cross-Platform** - macOS, Linux, Windows



## 🧠 AI Algorithm

### Minimax Algorithm
The AI uses Minimax algorithm for decision-making:
- **Maximize** AI's score
- **Minimize** player's score
- Recursively evaluate future game states

### Alpha-Beta Pruning
Optimizes search efficiency through Alpha-Beta pruning:
- **Alpha** - Best option found for maximizing player
- **Beta** - Best option found for minimizing player
- Prunes branches that won't affect the final decision

### Heuristic Evaluation
Pattern evaluation scores:
| Pattern | Score |
|---------|-------|
| Five in a row | 100,000 |
| Open Four | 50,000 |
| Four | 10,000 |
| Open Three | 5,000 |
| Three | 1,000 |
| Open Two | 500 |
| Two | 100 |

### Search Optimization
- **Neighborhood Pruning** - Only search empty cells near existing pieces
- **Candidate Sorting** - Sort candidate positions by evaluation score
- **Search Width Limit** - Limit branches per search level

## 🚀 Requirements

- macOS / Linux / Windows
- .NET 8.0 SDK

## 📦 Build and Run

```bash
# Navigate to project directory
cd GomokuAI

# Restore dependencies
dotnet restore

# Build project
dotnet build

# Run project
dotnet run
```

## 🎯 Game Rules

1. Black (Player) moves first
2. Players take turns placing pieces on board intersections
3. First to connect five pieces horizontally, vertically, or diagonally wins
4. Draw if board is full with no winner

## 🎨 Interface Preview

The game features a dark theme design:
- Left: Board area with mouse hover preview
- Right: Control panel showing game status and history

## 📝 Development Notes

### Core Classes

- **Board** - Board state management, placing pieces, undo, empty cell queries
- **Move** - Single move data structure
- **Player** - Player information
- **GameService** - Game flow control
- **AIService** - AI decision engine
- **WinChecker** - Five-in-a-row detection

### Tech Stack

- **Avalonia 11.2** - Cross-platform UI framework
- **Fluent Theme** - Modern theme
- **.NET 8.0** - Runtime

### Extension Ideas

- Add difficulty levels (adjust search depth)
- Add opening book
- Implement forbidden move rules (professional rules)
- Add human vs human mode
- Save/load game records


