# Gomoku AI

A cross-platform Gomoku (Five-in-a-Row) game with AI opponent, built with C# and Avalonia. The AI uses Minimax algorithm with Alpha-Beta pruning for intelligent decision-making.

![Gomoku](https://img.shields.io/badge/Game-Gomoku-blue)
![C#](https://img.shields.io/badge/Language-C%23-green)
![Avalonia](https://img.shields.io/badge/Framework-Avalonia-purple)
![.NET 8](https://img.shields.io/badge/.NET-8.0-orange)
![Cross-Platform](https://img.shields.io/badge/Platform-macOS%20|%20Linux%20|%20Windows-brightgreen)

![Demo](./demo/demo1.jpg)

## 🎮 Features

- **15×15 Standard Board** - Classic Gomoku board
- **Human vs AI Mode** - Player plays Black (first), AI plays White
- **Intelligent AI** - Minimax algorithm with Alpha-Beta pruning
- **Difficulty Selection** - Easy / Medium / Hard (configurable search depth)
- **Undo/Redo** - Stack-based state management for reversible operations
- **Real-time Win Detection** - Four-direction consecutive check
- **Modern UI Design** - Dark theme, clean interface
- **Cross-Platform** - macOS, Linux, Windows

## 🎚️ Difficulty Levels

| Level | Search Depth | Description |
|-------|--------------|-------------|
| Easy | Depth 2 | Quick response, less challenging |
| Medium | Depth 3 | Balanced difficulty |
| Hard | Depth 4 | Strong AI, slower response |

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

## 🔄 Undo/Redo System

Stack-based state management for clean reversible operations:

```csharp
Stack<Move> _undoStack;  // Move history
Stack<Move> _redoStack;  // Undone moves

Undo() → Pop from undoStack, push to redoStack
Redo() → Pop from redoStack, push to undoStack
```

## 🚀 Requirements

- macOS / Linux / Windows
- .NET 8.0 SDK

## 📦 Build and Run

```bash
cd GomokuAI
dotnet restore
dotnet build
dotnet run
```

## 🎯 Game Rules

1. Black (Player) moves first
2. Players take turns placing pieces on board intersections
3. First to connect five pieces horizontally, vertically, or diagonally wins
4. Draw if board is full with no winner

## 📝 Core Classes

| Class | Description |
|-------|-------------|
| `Board` | Board state, Stack-based undo/redo |
| `Move` | Single move data structure |
| `Difficulty` | Enum for AI difficulty levels |
| `GameService` | Game flow control |
| `AIService` | Minimax + Alpha-Beta AI engine |
| `WinChecker` | Five-in-a-row detection |

## 🛠️ Tech Stack

- **C#** - Programming language
- **.NET 8.0** - Runtime
- **Avalonia 11.2** - Cross-platform UI framework
- **Fluent Theme** - Modern dark theme

