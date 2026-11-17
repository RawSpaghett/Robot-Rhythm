## Modifier menu

Open MainMenu. The level's Play button opens the modifier menu. Tutorial starts normally.

Select any modifiers you want, then press Play. Leaving everything off starts a standard run. Retry keeps the current selection. Choosing a level again clears the choices.

## Adding a modifier

Select ModifierDatabase in MainMenu and use Add modifier. The menu reads this list each time it opens, so you do not need to add a UI button yourself. The list scrolls when there are more options.

A modifier needs to inherit ModifierBase, have [System.Serializable], and implement Enabled(). The gameplay effect stays in that class. The UI passes the selected instances into ModifierManager.activeModifiers before they run.

Brutal is connected to Justin's existing modifier. Add the other modifiers once their gameplay code is ready.

## Records

Modified runs show their current score but do not overwrite standard best scores or award standard perfect ribbons. Standard runs keep the existing save behavior.
