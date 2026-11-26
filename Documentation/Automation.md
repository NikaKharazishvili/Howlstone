# Automation

Automation saves the most development time. A repeated manual step is added here when it is automated.

- The owner's private editor toolkit is installed in the project but is not in the repository ([AboutTheGame](AboutTheGame.md)). It automates asset import settings, builds and environment work. Its docs are private too.
- With no Unity Editor open for this project, a headless run imports and compiles: `Unity.exe -batchmode -nographics -quit -projectPath <project> -logFile <file>`; read the log for `error CS` lines. `-executeMethod` runs a one-time editor script, deleted after it ran.
- Level work: a level is built by one script of the environment tools' commands; scripts in `.claude/Level` add what the game needs to it, report its mistakes as text, picture it in one image and show a set of prefabs ([Environment](Environment.md)).
- Moving assets: files and their `.meta` move together with a script, so no reference breaks ([ProjectStructure](ProjectStructure.md)).
- Claude Code hook (`.claude/CodeRules.ps1`): cuts the final newline of a hand-written file, and reports a broken code rule, a date in a doc or a doc that names a missing file.