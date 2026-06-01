module BlackAndWhite.Program

open System

/// Entry point: show the rules, then run one match.
[<EntryPoint>]
let main argv =
  // The board uses box-drawing characters, so make sure output is UTF-8.
  try Console.OutputEncoding <- Text.Encoding.UTF8 with _ -> ()
  // `--no-color` (or `-n`) turns off ANSI colors for the whole run.
  if argv |> Array.exists (fun a -> a = "--no-color" || a = "-n") then
    Display.disableColor ()
  Display.intro ()
  Game.run ()
  0
