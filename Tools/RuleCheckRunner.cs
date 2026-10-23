using System;
class RuleCheckRunner { static int Main() { try { Console.WriteLine("PASS: "+Hexbound.RuleChecks.Run()+" combat rule assertions"); return 0; } catch(Exception e) { Console.Error.WriteLine(e); return 1; } } }
