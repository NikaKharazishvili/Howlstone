# ChatGPT Prompts

How to write a prompt for a picture generator (UI icons, frames, textures). Read this before writing any. Same method as the owner's other project.

## The game's look
- Stylized fantasy: hand-painted, toony but for adults, in the same family as Dota 2 and League of Legends. Not low poly, not too cartoony or childish, not realistic.
- UI: dark stone or steel bodies, antique gold trim, soft inner glow, strong contrast so it reads at a small size. No candy colors, no thick cartoon outlines.
- The player character is going to be a wolf: its own icons are made later to match this look.

## The prompt
- One block, ready to paste, for exactly what the owner named and nothing else.
- **Always exactly ONE image, with everything on it.** The owner has a small free quota of generations: a prompt that asks for several images (or "separate" images) spends it several times over. Never ask for more than one image, however many things are needed. The owner always gets ONE text to paste.
- The one image is a sheet: a grid on a transparent background, one thing per cell, things of one kind in one row (same size in a row), a clear empty gap between cells and between rows. About 12 to 14 cells fit well. When there are more things, leave out the second look of each (the steel row) first, then the least needed pieces, and say which were left out. Describe the sheet row by row ("row 1: ..., row 2: ...").
- Say what not to draw (text, letters, logos, people, hands, frames around the pictures, shadows, a checkerboard) and what to draw alone and centered. Use words the generator can not mix up.
- Anything the game draws itself (a letter on a key, a number, a label) is left empty in the picture: the game puts its text on it, so one picture works for all.
- A frame or bar that must stretch is described as 9-slice friendly: ornate corners, straight repeating edges, a plain flat center.

## What gives the game's look (picked by the owner)
- The dark stone version with gold trim is the look to ask for: smooth clean painted shapes, soft rounded bevels, simple clear highlights, a clean smooth gold gradient, dark stone body with a soft inner glow, strong contrast. Say what to avoid: scratches, grain, noise, dirt, realistic metal texture, thick outlines. The steel row is made but not used.
- The sheet has a true alpha channel (a drawn checkerboard or grey background is a failure) and one piece centered in each cell.
- Keys: empty square, empty wide (twice as wide as tall), square with a bright orange glowing outline for the selected one. The generator draws arrows badly, so arrows are never asked for: the game writes `<` and `>` on an empty key.
- Gamepad: round dark buttons with a glowing symbol (a letter-X shape made of two diagonal strokes, a ring, a square, a triangle; say "not a plus sign") and one long empty shoulder button.
- Frames: a wide banner with pointed ornate ends, a thin table header, a large square panel, a thin row, a selection frame with a soft orange glow, each 9-slice friendly.
- List slots: a wide plaque (about four times wider than tall) with plain rounded corners, small gold corner studs and a flat dark inner panel, in a normal and an orange-glow (selected) state. Say "not pointed ends, not a keycap, not a banner": pointed banners and keycaps did not suit a list row. When states differ by a glow, the pieces have different sizes: put them on one equal canvas before slicing.
- Arrows: a small rounded square gold-bordered button with one big, simple, bold chevron (left, and its mirror for right) works; the generator draws it well when told "symmetric, thick strokes, nothing else on the button".
- The generator's size is never exact: after the cut each picture is resized to a width and height that are multiples of 4 (the texture compressor needs it) and the slice borders are checked against the new size.

## After the picture
- The owner saves it with ChatGPT's download button as a PNG and gives its path: a paste or a screenshot loses the transparent background.
- Never set a picture in the game before the owner picks it. A picked picture is cut to what it shows. A frame is imported with its slice borders set (UI Toolkit `-unity-slice-*`).