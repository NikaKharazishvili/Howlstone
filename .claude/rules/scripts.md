---
paths:
  - "Assets/Scripts/**"
---
# Design
- Write code so future changes touch one place, not logic scattered across many places.
- Write final, optimized code from the first line, even for a test or a first version: no Instantiate/Destroy for something that changes often (reuse a fixed object and swap its data), no allocations per frame or per change. Never leave "test code, rewrite later": nobody remembers. A fix must start from a measurement (a value, an axis, a size), never from a number copied from old code.
- Anything that always exists and whose settings are known in advance (a camera, a particle system, a child object, a mesh) is made in the editor, saved in the scene or prefab, and linked by a serialized field: never `new GameObject`, `AddComponent` or a lazy first-use setup in code. Runtime creation is only for what is not known until play or a pool that has no free item left. When code makes such an object, say so and ask once whether it can be pre-made.
- What is the same in every scene lives inside the scene manager, saved as one prefab that each scene reuses: never set it up again in each scene.
- Prefer fewer objects, components and references: add one only when it removes duplicate code or a startup-order problem.
- Never run an effect twice for one change. Before calling a method, check what it already does inside. Each effect has one owner method that the others call.
- One way over many: pick the option that's best overall and use it everywhere, even if another is slightly better in some cases. Balance performance with simplicity.
- Things of the same kind look and behave the same: before making a new one, find the existing one and reuse its shared style or code.
- Protect encapsulation: a class's state is changed only by that class's own methods. Other classes get only what they need, to read it or to ask for a change.

# Style
- Shorter, simpler code that is fast to read, with as few lines as possible: collapse short statements onto one line and declare same-type fields and constants together, one per line when they're long. A small plain data class (a few fields, no logic) goes in the file of the one script that owns it: only a MonoBehaviour or ScriptableObject needs its own file.
- Name a number once (game values in GameConstants) when it's used in more than one place or is a tuning value that may change. Keep one-off obvious numbers inline (0, 1, 0.5f for a half).
- GameConstants holds only values many scripts use, or game settings kept together. A value used by one script stays in that script.
- Name things so clearly they need no comment. A class that has a doc in Documentation gets no comments in its script: the doc explains it, including why, so change the doc instead. The one exception: a group of constants gets a one-line title comment over it. A script without a doc keeps short comments (max 2 lines, or 1 line up to 180 chars, no period at the end) until its doc exists.
- Name methods by what they do and to what, even if long, and never give two methods the same vague name. Rename unclear names in the code you change, except ones that animation events or scene buttons call by name.
- A switch over types or enum values lists every case it handles and throws NotImplementedException for the rest, never a quiet default.
- Never hide a missing or wrong value with a null or empty check, `?.` or a quiet return: let it throw, or let Unity warn by itself, and write no warning code of our own. A check is only for a value that is optional by design, and the doc says so.
- Never write `private` before a field or method (it is the default). Write a color as a literal the editor can show as a swatch: `new Color(0.5f, 0.2f, 0.1f)`, never a hex string.
- Remove `using` lines a file no longer needs from every file you edit.
- A change that removes the last use of something (a field, method, constant, parameter, asset slot, doc line) deletes it in the same change: search its name once before finishing.

# Mistakes To Avoid
- Static fields keep their values between Plays when the domain is not reloaded: a static list or field that holds per-Play state is cleared at the start of Play, or it is an instance field of a scene object.
- Never set `enabled = false` in `Awake` and `enabled = true` in the same call or frame: `OnEnable` runs twice.
- `GetComponentsInChildren` without `true` still returns components under a disabled root object: check `activeInHierarchy` when disabled objects must be left out.
- Anything read from a save file can be missing, empty or from an older version: read it through one method that gives a safe value, never index it directly. A file the game writes at run time never lives inside `Assets` (Documentation/Saving.md).
- A sound plays where it comes from: a character's sound on its own `AudioSource` (3D, mono clip). Only UI and the player's own sounds are 2D.
- `this.Wait` lambdas use the parameter they get, never `this` or a local variable (a closure allocates again).
- An animation event calls a method by name: renaming `DealDamage` or `PlayRandomFootstepSound` breaks the clips silently. Search the clips before renaming.
- Before moving, renaming or deleting an asset, search the code and docs for its path and name: an `AssetDatabase` call or a path string breaks only later, as an empty value or a pink object.
- Never use a type, namespace or file from the private toolkit in game code.
- No `Light` component and no shadow caster or receiver, ever: blob shadows and fake lights only, as in Documentation/Rendering.md. A system ported from the owner's top-down project is reviewed for top-down assumptions first.

# After Editor Scripts And Bugs
- After changing project settings through code (layers, tags, anything in ProjectSettings), read the file back to check it was saved.
- A one-time editor script that changes many assets is deleted after it ran.
- After a bug that took more than a quick fix to find, write its cause and the check for it (one or two lines) in the doc of that system, and add a rule here if it can happen again in other code. Check the doc first when a similar bug shows up.