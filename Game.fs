namespace BlackAndWhite

open System

/// The interactive game loop.
module Game =

  /// Print a farewell and stop the program immediately.
  let private quit () : 'a =
    printfn ""
    printfn "You left the game. Goodbye!"
    Console.Out.Flush()
    exit 0

  /// Read a tile from the Player, re-prompting on any invalid input:
  /// a non-integer, a value outside 0-8, or a tile that is no longer in hand.
  /// Typing "exit" at any prompt quits the game. End-of-input also quits.
  let rec private readTile (prompt: string) (hand: Tile list) : Tile =
    Console.Write(prompt)
    let raw = Console.ReadLine()
    if isNull raw then quit ()   // input stream closed (EOF)
    let text = raw.Trim()
    if text.Equals("exit", StringComparison.OrdinalIgnoreCase) then quit ()
    match Int32.TryParse(text) with
    | false, _ ->
        printfn "Invalid input: please enter a whole number."
        readTile prompt hand
    | true, n when n < 0 || n > 8 ->
        printfn "Invalid input: a tile must be between 0 and 8."
        readTile prompt hand
    | true, n when not (List.contains n hand) ->
        printfn "Invalid input: tile %d is not in your hand." n
        readTile prompt hand
    | true, n -> n

  /// Play a single round and return the resulting state.
  let private playRound (rng: Random) (s: GameState) : GameState =
    Display.roundHeader s
    let ctx : ScoreContext =
      { MyScore = s.EnemyScore
        OppScore = s.PlayerScore
        RoundsLeft = GameState.roundsLeft s }

    // Decide the tile each side plays this round.
    let playerTile, enemyTile =
      match s.Lead with
      | Player ->
          // The Player leads face-down; the Enemy answers knowing only the
          // color. The Player's tiles of that color are what the Enemy can
          // legitimately deduce the face-down tile to be one of.
          let p =
            readTile
              (sprintf "Round %d - Your Lead. Select a tile to play: " s.Round)
              s.PlayerHand
          let possible =
            s.PlayerHand |> List.filter (fun t -> Tile.color t = Tile.color p)
          let e = Enemy.respond s.EnemyHand possible ctx
          p, e
      | Enemy ->
          // The Enemy leads face-down; the Player answers knowing the color.
          let e = Enemy.lead rng s.EnemyHand ctx
          Display.enemyLeads s.Round e
          let p = readTile "Select a tile to play: " s.PlayerHand
          p, e

    // Resolve the round: the higher number wins and scores 1 point.
    let outcome = GameState.outcome playerTile enemyTile
    let playerScore = s.PlayerScore + (if outcome = Win then 1 else 0)
    let enemyScore = s.EnemyScore + (if outcome = Lose then 1 else 0)
    Display.showdown s.Round playerTile enemyTile outcome playerScore enemyScore

    let record =
      { Number = s.Round
        LedBy = s.Lead
        PlayerTile = playerTile
        EnemyTile = enemyTile
        Outcome = outcome }
    { s with
        Round = s.Round + 1
        PlayerHand = s.PlayerHand |> List.filter (fun t -> t <> playerTile)
        EnemyHand = s.EnemyHand |> List.filter (fun t -> t <> enemyTile)
        PlayerScore = playerScore
        EnemyScore = enemyScore
        Lead = GameState.nextLead s.Lead outcome
        History = s.History @ [ record ] }

  /// True once the match is mathematically settled: one side's score can no
  /// longer be overtaken by the other, even if the other wins every remaining
  /// round. `s` is the state *after* a round has been played, so `roundsLeft`
  /// counts exactly the rounds that are still to come.
  let private isDecided (s: GameState) : bool =
    GameState.decided s.PlayerScore s.EnemyScore (GameState.roundsLeft s)

  /// Play rounds until all nine are done or the match is decided early.
  let rec private loop (rng: Random) (s: GameState) : GameState =
    if s.Round > GameState.totalRounds then s
    else
      let s' = playRound rng s
      if isDecided s' then s' else loop rng s'

  /// Run a complete match from the initial state.
  let run () : unit =
    let final = loop (Random()) GameState.initial
    Display.matchSummary final
