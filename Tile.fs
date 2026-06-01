namespace BlackAndWhite

/// The color of a tile.
/// Even-numbered tiles are Black; odd-numbered tiles are White.
type Color =
  | Black
  | White

/// A tile is simply an integer from 0 to 8.
type Tile = int

/// Helpers for working with a single tile.
module Tile =

  /// The nine tiles that each side starts the match with.
  let initialHand : Tile list = [ 0 .. 8 ]

  /// The color of a tile: Black for even numbers (0, 2, 4, 6, 8) and
  /// White for odd numbers (1, 3, 5, 7). 0 counts as an even number.
  let color (t: Tile) : Color =
    if t % 2 = 0 then Black else White

  /// The display name of a color, e.g. "Black".
  let colorName (c: Color) : string =
    match c with
    | Black -> "Black"
    | White -> "White"

  /// The parity word shown beside a color, e.g. "Even".
  let parityName (c: Color) : string =
    match c with
    | Black -> "Even"
    | White -> "Odd"
