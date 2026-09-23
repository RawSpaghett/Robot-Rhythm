# UI checks

In the preview scene, check Home → Routes → Placeholder → Routes, Home → Controls → Home, and Home → Settings → Home. Confirm route/package highlights, menu volume, reduced motion, and logo entrance/idle motion.

Check a short landscape screen and a wide landscape screen. Text should be sharp, labels should fit, and buttons should stay inside safe areas. Controls should show the action first, with the colored gesture and arrow below it.

The optional `UiMobileChecks` component checks touch-only navigation, selections, the static placeholder, logo/reduced-motion behavior, text density and overflow, and safe-area bounds. `UiWalkthrough` records menus only. Neither adds keyboard/mouse bindings or gameplay.

Before Android delivery, verify notches, touch target size, text density, background/resume behavior, and saved settings on a real phone using the team's editor version. A local renderer is not a substitute for that device check.
