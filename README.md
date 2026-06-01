# Black and White

A command-line strategy game of deduction and bluffing, written in F# (.NET 10).

## Overview

You play against a computer Enemy. Each side holds nine tiles numbered 0-8.

Tiles are color-coded: **Black** for even numbers (0, 2, 4, 6, 8) and **White**
for odd numbers (1, 3, 5, 7). 

Every round, one player leads by placing a tile face-down; the opponent sees only its *color* before answering with a tile of their own. 

The higher number wins the round and scores one point.

The Enemy's exact numbers are never shown — you must deduce its remaining hand from the
colors it plays and the results of past rounds.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

Verify your installation:

```
dotnet --version
```

## How to Run

From the project folder (the folder that contains `BlackAndWhite.fsproj`):

```
dotnet run
```

This builds and starts the game in the terminal.

The board is drawn as cards: **black cards** (white outline, white number) for
even tiles and **solid white cards** for odd ones. On a terminal that supports
it the cards and results are shown in color.

To run without color, pass `--no-color` (short form `-n`):

```
dotnet run -- --no-color
```

> The `--` separates `dotnet run`'s own options from arguments passed to the
> program. Color is also automatically off when output is redirected (e.g.
> piped to a file) or when the `NO_COLOR` environment variable is set.

## How to Play

- The match lasts up to 9 rounds.
- At the start of each round the game shows your full hand as face-up cards
  (numbers and colors), the Enemy's hand as face-down cards with the black/white
  counts, and the current score.
- When prompted, type the number (0-8) of the tile you want to play and press
  Enter. Invalid input — a non-number, a value outside 0-8, or a tile you have
  already used — is rejected, and the prompt is repeated.
- Type `exit` (case-insensitive) at any prompt to quit the game immediately.
- The lead player plays first, face-down. The second player sees only the
  *color* of that tile before choosing their own.
- The higher number wins the round (+1 point); equal numbers are a draw.
- The winner of a round leads the next round; after a draw the same player
  keeps the lead. You lead Round 1.
- The match ends after 9 rounds, or earlier the moment one player's lead can no
  longer be overcome even if the other side wins every remaining round (for
  example, reaching 5 points).
- When the match ends, the game prints the full match history and the result.

## Project Structure

| File | Responsibility |
|------|----------------|
| `Tile.fs` | Tile values and Black/White color logic |
| `GameState.fs` | Core types: sides, outcomes, round records, match state |
| `Enemy.fs` | The Enemy's strategy (leading and responding) |
| `Display.fs` | All console output (board, prompts, results, history) |
| `Game.fs` | The interactive round loop, input validation, early-end check |
| `Program.fs` | Entry point |

Files are listed in compile order in `BlackAndWhite.fsproj`.

## The Enemy

The Enemy is a strategic (not random) opponent. It never sees your face-down
tile — only its color, exactly as you only ever see the color of its tiles.
From that color and the tiles you are publicly holding, it works out which
values you might have played and decides accordingly:

- **When responding**, it wins as cheaply as it can: it plays the smallest tile
  that is more likely to win than not, and sacrifices its lowest tile when a
  win is unlikely.
- **When leading**, it leads a mid-range tile, keeping its highest tile as a
  safety net and its lowest as a future sacrifice.
- It plays more aggressively when it must win to stay in the match, and throws
  rounds away cheaply once the match is already secured.

## Changes from the Proposal

The implementation follows the submitted requirements document. Every rule in
the proposal still holds: the tile/color system, lead handling, face-down play
with only the color revealed, the hidden-number deduction, and the 9-round /
early-decision ending. The differences below are presentation and a clarified
detail, not changes to the rules.

- **Presentation:** the proposal's Example Interaction shows a plain-text board
  (for example `Player : [B : 0, 2, 4, 6, 8 | W : 1, 3, 5, 7]` and
  `Round 1 Result : You Win!`). The final game shows the *same information* as a
  colored card layout instead — your hand as face-up cards, the Enemy's hand as
  face-down cards with black/white counts, and a card showdown for each round.
  This is a console-UI improvement that keeps the observable behavior identical;
  the interactive prompt (`Round N - Your Lead. Select a tile to play:`) is
  unchanged. Color is automatically disabled when output is not a terminal or
  when `NO_COLOR` is set, so the game remains usable everywhere.

- **Clarification (not a requirement change):** the proposal does not state how
  the Enemy chooses its tiles. This implementation gives the Enemy a *strategic*
  opponent rather than a purely random one, which makes the proposal's stated
  theme of "deduction and psychological warfare" meaningful. The Enemy still
  only ever sees the color of your face-down tile, exactly as you only see the
  color of its tiles, so no hidden information is used. See **The Enemy** above.

- **Quality-of-life addition — `exit` to quit:** typing `exit` (case-insensitive)
  at any prompt ends the game immediately. End-of-input (EOF) is treated the
  same. This is not required by the proposal but is a standard CLI convenience
  and is documented in the on-screen rules and in *How to Play* above.

## Use of Large Language Models

I used an LLM as a refinement assistant after the core game logic was
already working. I remained responsible for the final implementation and
read through every change before accepting it.

- **What I used the LLM for.** Four things:
  1. A security review before making the repository public — to flag and
     remove anything that should not be exposed (API keys, hard-coded
     paths, credentials, etc.).
  2. Adding ANSI color output so that tile colors, prompts, and round
     results would be visually distinct.
  3. A card-style display, because the original plain-text board was hard
     to read at a glance.
  4. Wiring an `exit` shortcut so the player can quit the game cleanly at
     any prompt, with end-of-input handled the same way.

- **What I had to change or re-prompt.** The first display draft conveyed
  all the required information correctly, but it read as a plain-text
  board rather than an actual card game. I re-prompted to redo it with
  box-drawn cards, ANSI colors, and longer round/section separators
  before it looked like the kind of card game I had in mind.

- **What the LLM could not do correctly.** The LLM could not judge the
  actual visual result on a real terminal. I ran the game repeatedly,
  observed the output myself, and re-prompted with specific improvements —
  spacing, color contrast, card borders — based on what I actually saw.
  The visual polish required this manual feedback loop; the LLM alone
  could not converge on a result that looked right.