namespace BlackAndWhite

open System

/// Everything the game prints to the console, drawn as a simple card game.
module Display =

  // ----- ANSI color support -----

  /// Color is used only on a real terminal. When output is redirected (for
  /// example piped to a file) or NO_COLOR is set, plain text is printed instead.
  let private useColor =
    not Console.IsOutputRedirected
    && isNull (Environment.GetEnvironmentVariable "NO_COLOR")

  /// Wrap a string in an ANSI escape so it prints with the given style.
  let private esc (codes: string) (s: string) : string =
    if useColor then "\x1b[" + codes + "m" + s + "\x1b[0m" else s

  let private bold s = esc "1" s
  let private dim s = esc "2" s
  let private green s = esc "1;92" s
  let private red s = esc "1;91" s
  let private yellow s = esc "1;93" s
  let private cyan s = esc "96" s

  // ----- Cards -----

  /// A Black (even) tile is a black card with a white frame; a White (odd) tile
  /// is a solid white card. The white frame keeps a black card visible even on
  /// a dark terminal background.
  let private cardCodes (c: Color) : string =
    match c with
    | Black -> "97;40"
    | White -> "30;107"

  /// A face-up card showing its number, as three lines.
  let private faceCard (t: Tile) : string list =
    let codes = cardCodes (Tile.color t)
    [ esc codes "┌───┐"; esc codes (sprintf "│ %d │" t); esc codes "└───┘" ]

  /// A face-down card: its color shows, but the number is hidden.
  let private backCard (c: Color) : string list =
    let codes = cardCodes c
    [ esc codes "┌───┐"; esc codes "│▒▒▒│"; esc codes "└───┘" ]

  /// A compact one-line "chip" version of a face-up card, used inline in the
  /// match history (e.g. " 3 " coloured by the tile's colour).
  let private chip (t: Tile) : string =
    esc (cardCodes (Tile.color t)) (sprintf " %d " t)

  /// Join the i-th line of every card in a group side by side.
  let private groupLines (cards: string list list) : string list =
    [ for i in 0 .. 2 -> cards |> List.map (List.item i) |> String.concat "" ]

  /// Print a row of cards split into the Black group and the White group.
  let private printHand (blacks: string list list) (whites: string list list) : unit =
    let bl = groupLines blacks
    let wl = groupLines whites
    let sep = if not (List.isEmpty blacks) && not (List.isEmpty whites) then "  " else ""
    for i in 0 .. 2 do
      printfn "   %s%s%s" (List.item i bl) sep (List.item i wl)

  /// Split a hand into its Black tiles and White tiles, each sorted ascending.
  let private split (hand: Tile list) : Tile list * Tile list =
    let pick c = hand |> List.filter (fun t -> Tile.color t = c) |> List.sort
    pick Black, pick White

  // ----- Banners -----

  /// A centred title padded out to a fixed width with the given fill character.
  let private banner (fill: char) (title: string) : string =
    let width = 48
    let t = sprintf "  %s  " title
    let pad = max 2 (width - t.Length)
    let l = pad / 2
    String(fill, l) + t + String(fill, pad - l)

  /// "You 3  —  1 Enemy", with a leading label.
  let private scoreLine (label: string) (playerScore: int) (enemyScore: int) : string =
    sprintf "   %s    %s %s   —   %s %s"
            (dim label) (cyan "You") (bold (string playerScore))
            (bold (string enemyScore)) (red "Enemy")

  // ----- Public output -----

  /// A short explanation of the rules, printed once when the game starts.
  let intro () : unit =
    printfn ""
    printfn "%s" (bold (banner '═' "BLACK  AND  WHITE"))
    printfn ""
    printfn "  Each side holds 9 tiles, played as cards:"
    printfn "    %s  even numbers 0 2 4 6 8" (esc "97;40" " BLACK ")
    printfn "    %s  odd numbers 1 3 5 7" (esc "30;107" " WHITE ")
    printfn ""
    printfn "  The lead plays a card face-down; you see only its color,"
    printfn "  then answer. The higher number wins the round (+1 point) and"
    printfn "  leads the next one. The enemy's numbers are never shown - read"
    printfn "  the colors and the results to deduce its hand."
    printfn "  The match lasts 9 rounds, or ends once the score is decided."
    printfn ""
    printfn "  %s to quit the game at any prompt." (bold "Type \"exit\"")
    printfn ""

  /// The board printed at the start of every round: the enemy's hidden hand,
  /// the score, and the player's open hand.
  let roundHeader (s: GameState) : unit =
    printfn ""
    printfn "%s" (bold (banner '─' (sprintf "Round %d" s.Round)))
    printfn ""
    let eb, ew = split s.EnemyHand
    printfn "  %s  (numbers hidden)" (dim "ENEMY")
    printHand (eb |> List.map (fun _ -> backCard Black))
              (ew |> List.map (fun _ -> backCard White))
    printfn "   %s   %s"
            (esc "97;40" (sprintf " black x%d " (List.length eb)))
            (esc "30;107" (sprintf " white x%d " (List.length ew)))
    printfn ""
    printfn "%s" (scoreLine "SCORE" s.PlayerScore s.EnemyScore)
    printfn ""
    let pb, pw = split s.PlayerHand
    printfn "  %s    (your hand)" (dim "YOU")
    printHand (pb |> List.map faceCard) (pw |> List.map faceCard)

  /// Shown when the enemy leads: its face-down card (color visible) and an
  /// announcement of that color, before the player answers.
  let enemyLeads (round: int) (t: Tile) : unit =
    let c = Tile.color t
    printfn ""
    printfn "  %s" (bold (sprintf "Round %d - Enemy's Lead, face-down:" round))
    printHand (if c = Black then [ backCard Black ] else [])
              (if c = White then [ backCard White ] else [])
    printfn "  Enemy leads with a %s tile (%s)." (Tile.colorName c) (Tile.parityName c)

  /// The reveal: the player's card face-up beside the enemy's card (color only,
  /// number still hidden), the enemy's color, the round result and the score.
  let showdown (round: int) (playerTile: Tile) (enemyTile: Tile)
               (outcome: Outcome) (playerScore: int) (enemyScore: int) : unit =
    let you = faceCard playerTile
    let enemy = backCard (Tile.color enemyTile)
    printfn ""
    printfn "   %s            %s" (cyan "YOU") (red "ENEMY")
    for i in 0 .. 2 do
      let mid = if i = 1 then "    vs    " else "          "
      printfn "   %s%s%s" (List.item i you) mid (List.item i enemy)
    let ec = Tile.color enemyTile
    printfn "   Enemy has played a %s tile (%s)." (Tile.colorName ec) (Tile.parityName ec)
    printfn ""
    let text, paint =
      match outcome with
      | Win -> sprintf "Round %d  >  You Win!" round, green
      | Lose -> sprintf "Round %d  >  You Lose!" round, red
      | Draw -> sprintf "Round %d  >  Draw!" round, yellow
    printfn "   %s" (paint text)
    printfn "%s" (scoreLine "SCORE" playerScore enemyScore)

  /// The match history and the final verdict, printed once the match is over.
  let matchSummary (s: GameState) : unit =
    printfn ""
    printfn "%s" (bold (banner '═' "Match History"))
    for r in s.History do
      let lead =
        match r.LedBy with
        | Player -> "You"
        | Enemy -> "Enemy"
      let res, paint =
        match r.Outcome with
        | Win -> "Win", green
        | Lose -> "Lose", red
        | Draw -> "Draw", yellow
      printfn "   Round %d   lead %-5s   you %s   vs   enemy %s   %s"
              r.Number lead (chip r.PlayerTile) (chip r.EnemyTile) (paint res)
    printfn "%s" (dim (String('═', 48)))
    let played = List.length s.History
    if played < GameState.totalRounds then
      printfn "   (decided early after %d round%s)" played (if played = 1 then "" else "s")
    printfn "%s" (scoreLine "FINAL" s.PlayerScore s.EnemyScore)
    let verdict, paint =
      if s.PlayerScore > s.EnemyScore then "You Win the match!", green
      elif s.EnemyScore > s.PlayerScore then "Enemy Wins the match!", red
      else "The match is a Tie!", yellow
    printfn "   %s" (paint (sprintf ">  %s" verdict))
    printfn ""
