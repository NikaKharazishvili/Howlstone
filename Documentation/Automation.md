# Automation

Automation saves the most development time. A repeated manual step is added here when it is automated.

- The owner's private editor toolkit is installed in the project but is not in the repository ([AboutTheGame](AboutTheGame.md)). It automates asset import settings, builds and environment work. Its docs are private too.
- Unity CLI: a headless import or compile (`Unity.exe -batchmode -quit -projectPath ... -logFile ...`) shows script errors with no editor open; `-executeMethod` runs a one-time editor script (used to move every Animator Controller to `ClipAnimator`).
- Moving assets: files and their `.meta` move together with a script, so no reference breaks ([ProjectStructure](ProjectStructure.md)).
- Claude Code hook (`.claude/CodeRules.ps1`): cuts the final newline of a hand-written file, and reports a broken code rule, a date in a doc or a doc that names a missing file.