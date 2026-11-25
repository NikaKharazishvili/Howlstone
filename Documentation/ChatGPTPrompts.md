# ChatGPT Prompts

How to write a prompt for a picture generator (UI icons, frames, textures). Read this before writing any. Same method as the owner's other project.

## The game's look
- Stylized fantasy: hand-painted, toony but for adults, in the same family as Dota 2 and League of Legends. Not low poly, not too cartoony or childish, not realistic.
- UI: dark stone or steel bodies, antique gold trim, soft inner glow, strong contrast so it reads at a small size. No candy colors, no thick cartoon outlines.
- The player character is going to be a wolf: its own icons are made later to match this look.

## The prompt
- One block, ready to paste, for exactly what the owner named and nothing else.
- Each image is a grid with one thing per column and two different ideas per column (2 rows), on a transparent background. ChatGPT makes about 10 pictures in one image, so an image has at most 5 things. The owner always gets ONE text to paste, never several: when there are more than 5 things, that one text shares the style once and asks for several images one after another in the same answer, without questions.
- Say what not to draw (text, letters, logos, people, hands, frames around the pictures, shadows, a checkerboard) and what to draw alone and centered. Use words the generator can not mix up.
- Anything the game draws itself (a letter on a key, a number, a label) is left empty in the picture: the game puts its text on it, so one picture works for all.
- A frame or bar that must stretch is described as 9-slice friendly: ornate corners, straight repeating edges, a plain flat center.

## After the picture
- The owner saves it with ChatGPT's download button as a PNG and gives its path: a paste or a screenshot loses the transparent background.
- Never set a picture in the game before the owner picks it. A picked picture is cut to what it shows. A frame is imported with its slice borders set (UI Toolkit `-unity-slice-*`).