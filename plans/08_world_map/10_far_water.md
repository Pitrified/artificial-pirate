---
status: done
---

# Phase 10 - Waves out to the fog

Asked 2026-11-20 by the user, on the phone: "the waves end too soon, so there is a sharp change from choppy waters to almost flat". They took option A: the waves' shading out to the fog.

## Plan

- Cause: the drawn waves calm from 100 to 124 m so the fine grid meets the coarse one flat; the calm takes the waves' normal with the height, so the water past it is lit flat.
- The fragment works out the Gerstner normal and crest height itself, at each pixel's rest position: calmed in the shallows as before, not with distance.
- A wave fades as it gets shorter than a few pixels (from the screen-space size of a pixel on the water), so the 7 m wave does not shimmer far out.
- The vertex keeps the height and its far calm; the normal and crest varyings go.
- Check: a capture toward the horizon before and after; the gates; the cost on the phone.

## Done when

- The water past 124 m shows waves in its light, with no seam at the calm; the gates pass; a new dev APK.

## What the implementation found
- 2026-11-20. Done as planned: the fragment sums the four waves again at each pixel's rest position, each wave flattened as its wavelength nears 4 pixels of water (from `fwidth`). The vertex keeps the height with its far calm; the normal and crest varyings are gone.
- Captures toward the horizon, before and after: before, the water past the calm is lit flat with a seam to the waves; after, the waves show out to the fog with no seam, and no shimmer in the still frames.
- Gates: compile 0 warnings, EditMode 186/186, PlayMode 22/22.
- Left for the phone: the cost of the second wave sum per water pixel, and shimmer in motion. One APK after phase 11 carries both phases.
