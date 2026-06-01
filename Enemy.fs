namespace BlackAndWhite

open System

/// The score information the Enemy AI uses to decide how aggressively to play.
/// All fields are written from the Enemy's point of view.
type ScoreContext =
  { MyScore: int        // the Enemy's current score
    OppScore: int       // the Player's current score
    RoundsLeft: int }   // rounds still to play, including the current one

/// The Enemy's strategy.
///
/// The Enemy never sees the Player's face-down tile. When it responds, it is
/// told only the *color* of that tile, exactly as a human opponent would be.
/// From that color, and from the tiles the Player is publicly holding, it
/// reasons about which values the Player might have placed, then plays so as
/// to win rounds cheaply and avoid wasting strong tiles on lost causes.
module Enemy =

  /// True when the Enemy must win the current round: if it loses, the Player
  /// gains a point, one fewer round remains, and that already decides the
  /// match in the Player's favour.
  let private mustWin (ctx: ScoreContext) : bool =
    (ctx.OppScore + 1) > ctx.MyScore + (ctx.RoundsLeft - 1)

  /// True when the Enemy cannot lose the match even if it loses every
  /// remaining round, so it can safely throw the current round away.
  let private canCoast (ctx: ScoreContext) : bool =
    ctx.MyScore >= ctx.OppScore + ctx.RoundsLeft

  /// The chance that tile `e` beats a tile drawn uniformly at random from
  /// `possible`. Equal values are a draw, so a tie does not count as a win.
  let private winProb (possible: Tile list) (e: Tile) : float =
    let beaten = possible |> List.filter (fun p -> p < e) |> List.length
    float beaten / float (List.length possible)

  /// Choose the Enemy's tile when it RESPONDS (it plays second).
  /// `possible` is the list of tiles the Player could have placed face-down:
  /// every tile of the revealed color that the Player held this round.
  let respond (hand: Tile list) (possible: Tile list) (ctx: ScoreContext) : Tile =
    if mustWin ctx then
      // Last chance: take the tile with the best winning chance, breaking
      // ties towards the cheaper (smaller) tile.
      hand |> List.minBy (fun e -> (-(winProb possible e), e))
    else
      // Win as cheaply as possible: the smallest tile that is more likely to
      // win than not. If no such tile exists, give the round up and discard
      // the lowest tile rather than waste a strong one.
      let reliableWins = hand |> List.filter (fun e -> winProb possible e >= 0.5)
      match reliableWins with
      | [] -> List.min hand
      | wins -> List.min wins

  /// Choose the Enemy's tile when it LEADS (it plays first, face-down).
  /// `rng` is used only to vary the choice among mid-range tiles so that the
  /// Enemy does not become perfectly predictable.
  let lead (rng: Random) (hand: Tile list) (ctx: ScoreContext) : Tile =
    if mustWin ctx then
      List.max hand                 // strongest possible lead
    elif canCoast ctx then
      List.min hand                 // nothing to lose: lead the weakest tile
    else
      // Lead with a mid-range tile. This keeps the highest tile in reserve as
      // a safety net and the lowest tile as a future sacrifice, and hides
      // whether the Enemy is holding an extreme value.
      let sorted = List.sort hand
      let n = List.length sorted
      let lo = n / 3
      let hi = n - 1 - n / 3
      sorted |> List.item (lo + rng.Next(hi - lo + 1))
