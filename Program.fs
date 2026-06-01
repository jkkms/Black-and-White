module BlackAndWhite.Program

open System

/// Entry point: show the rules, then run one match.
[<EntryPoint>]
let main _ =
  // The board uses box-drawing characters, so make sure output is UTF-8.
  try Console.OutputEncoding <- Text.Encoding.UTF8 with _ -> ()
  Display.intro ()
  Game.run ()
  0
