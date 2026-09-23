# Menu behavior

`UiController` controls screen visibility, local selection highlights, menu volume, and reduced motion. Buttons call its public methods through serialized UnityEvents. Renaming a UI object does not change its menu action. The volume slider calls `SetVolume`; volume and reduced motion use local PlayerPrefs keys prefixed `rr.ui.v1`.

Route names and package descriptions in `SelectRoute` and `SelectPackage` are display examples. They do not load levels, select gameplay equipment, or define real progression. Replace those example labels with confirmed data when the team supplies a route/package source. SELECT currently opens the static Placeholder page; its On Click event is the future integration point. The Controls screen is explanatory only.

`Click Sound` accepts a supplied UI AudioClip. Leaving it empty keeps the short menu tone. Menu volume affects that sound only.

The scene's EventSystem uses `TouchUI.inputactions` with touchscreen position and press bindings. Keep it separate from gameplay input and use only one EventSystem when integrating the menu into another scene.

`UIReview` contains the optional `UiMobileChecks` and `UiWalkthrough` tools. They only run with `-uiQA` or `-uiVideo` in the Editor or a development build. They are not part of the main UI prefab and are not needed when integrating that prefab. Their button-name lookups serve automated captures only. Save captures outside the repository.
