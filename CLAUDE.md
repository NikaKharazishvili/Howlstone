# Answers
- Shortest, simplest answers, always: my English isn't good, so basic words and numbered points, no technical words or long "why" unless I ask. Max 5 points, one line each (about 15 words). Answer first, no tables. A big topic gets the short answer and a question about what I want in detail.

# How To Work
- Do exactly what I say, in full, the first time: check every item of my message before the answer. Never swap it for something smaller or different, skip it quietly or decide it is not needed. If an item can't be done, say which and why.
- Do every step yourself (builds, menu items, tools, checking the result): never hand a step back or ask "should I run it?". Ask only when a choice is really mine and has no sensible default.
- Simple code and simple work win: no extra lines for a tiny optimization (but prevent it if that adds no complexity), and no extra reads or checks the task doesn't need.
- I test in Play mode while you work, so only edit files: I recompile and test after, never tell me to. Recompile or leave Play mode only when you cannot go on without it, once, after all edits. Test in Play only when I say you are the tester. Reading the console is always fine.
- Wait for a compile with one blocking command (no Monitor, no message when nothing is new); over 30 seconds, find the cause.
- Before a Unity task run `unity status`: never assume no Editor is connected. With none open, see Documentation/Automation.md.
- Create and change files only with the Write and Edit tools (no shell redirects, heredocs, sed or perl): the hook sees only those.
- Commit and push yourself after each finished task or safe step, with what changed and why, and commit before a risky change. Never commit the private toolkit, secrets or a broken state. When something breaks, check `git log` and `git diff` first.
- Before building any system, tool, shader or asset, look for a finished one in my main project (`D:\Evolveon`) and port it, reviewing what differs here.

# Rules And Docs
- Never save memories: suggest a rule instead, as the general idea in few words with nothing from this game in it. Choose its place yourself, cheapest first: the system's doc in Documentation, an area rules doc or `.claude/rules`, a hook (a check a script can do), or this file only if every task needs it.
- When code or an asset changes what a doc or rule says, fix it in the same task. Tell me in one line if you notice a rule or doc that is wrong, outdated or missing, with the new text.

# Also Tell Me (one line each, only when it happens)
- Anything off in code you read (a possible bug, a useless or dead line or file, a misleading name), or a simpler way that already exists (Unity feature, package, my tool).
- A repeated manual step that can be automated (Documentation/Automation.md lists ours).
- With a game feature I ask for: one better or extra idea, naming the action RPG it is from.
- An asset or script changed: a listed optimization in Documentation/Optimizations.md that applies and is not done, or a result not measured.

# Read Before Working On
- Any script: `.claude/rules/scripts.md` (if not loaded yet) and every doc that lists it under `Code:` (search Documentation for its file name), whole. Their rules and bug notes must not be broken.
- Any area: its doc in Documentation (TODO.md lists what is not built).
- A new game feature: Documentation/AboutTheGame.md.
- Animation: Documentation/Characters/ClipAnimator.md. Saving: Documentation/Saving.md.
- Lights, shadows, shaders, materials, URP, how the world is drawn: Documentation/Rendering.md.
- Creating, touching, naming, moving or importing assets: Documentation/ProjectStructure.md.
- A level's world (ground, water, placing objects): Documentation/Environment.md.
- Packs in `Assets/Objects`: Documentation/ThirdPartyAssets.md.
- Trying an optimization: Documentation/Optimizations.md.
- A prompt for a picture generator: Documentation/ChatGPTPrompts.md.

# The Project
- Howlstone: a small single-player 3D action-adventure in Unity 6.3 (URP), to be a small but really good game. Never add networking, a store or money.
- The repository is public: only `Assets/Scripts`, `ProjectSettings`, `Packages`, `Documentation`, `.claude`, this file and the README are tracked. Art, audio, scenes and packs never are. Track only what I am happy to show.
- `Assets/Scripts/Nikson`, `Documentation/Nikson` and `.claude/rules/nikson.md` are my private toolkit: never commit them, copy their code into the game, use their namespace in game code or describe how they work in a public file or commit message. Edit them only when I ask.