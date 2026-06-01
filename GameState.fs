namespace BlackAndWhite

/// A side of the match.
type Side =
  | Player
  | Enemy

/// The result of a single round, described from the Player's point of view.
type Outcome =
  | Win
  | Lose
  | Draw

/// A finished round, kept so the match history can be printed at the end.
/// The Enemy's actual tile is stored so the final match summary can reveal it;
/// during play the Enemy's number is never shown to the Player.
type RoundRecord =
  { Number: int
    LedBy: Side
    PlayerTile: Tile
    EnemyTile: Tile
    Outcome: Outcome }

/// The complete state of a match.
type GameState =
  { Round: int                 // 1-based number of the round about to start
    PlayerHand: Tile list      // tiles the Player still holds
    EnemyHand: Tile list       // tiles the Enemy still holds
    PlayerScore: int
    EnemyScore: int
    Lead: Side                 // who leads the round about to start
    History: RoundRecord list  // finished rounds, oldest first
  }

/// Helpers for working with the match state.
module GameState =

  /// A match always lasts at most nine rounds.
  let totalRounds = 9

  /// The starting state of a fresh match: full hands, score 0-0, Player leads.
  let initial : GameState =
    { Round = 1
      PlayerHand = Tile.initialHand
      EnemyHand = Tile.initialHand
      PlayerScore = 0
      EnemyScore = 0
      Lead = Player
      History = [] }

  /// How many rounds are still to be played, counting the current one.
  let roundsLeft (s: GameState) : int =
    totalRounds - s.Round + 1

  /// The outcome of a round from the Player's point of view: the higher number
  /// wins, equal numbers are a draw.
  let outcome (playerTile: Tile) (enemyTile: Tile) : Outcome =
    if playerTile > enemyTile then Win
    elif playerTile < enemyTile then Lose
    else Draw

  /// Who leads the next round: the round's winner leads, and on a draw the side
  /// that led this round keeps the lead.
  let nextLead (currentLead: Side) (o: Outcome) : Side =
    match o with
    | Win -> Player
    | Lose -> Enemy
    | Draw -> currentLead

  /// True when the match is already settled: with `remaining` rounds still to
  /// play, one side leads by more points than the other can possibly make up,
  /// even by winning every remaining round.
  let decided (playerScore: int) (enemyScore: int) (remaining: int) : bool =
    playerScore > enemyScore + remaining
    || enemyScore > playerScore + remaining
