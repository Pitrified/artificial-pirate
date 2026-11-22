---
status: draft
priority: 0
description: |
  From an upload key to a tester installing from the Play Store: keystore, signed AAB, Play Console
  app and forms, internal testing track, the demo-ready checklist, tag v0.2.0.
---

# Private alpha release

Moved on 2026-11-17 from phases 11, 12 and 13 of [unity-setup-project's `docs/tracking.md`](https://github.com/Pitrified/unity-setup-project/blob/main/docs/tracking.md#phase-11---generate-upload-key), when the game was split into this repo; one folder, since each step gates the next.
Nothing of them was started there. The source's phase 8 left one device check, logcat clean, which is in [`01_bootstrap/05_verify.md`](../01_bootstrap/05_verify.md) instead.

## Plan

All HUMAN except where marked; the steps are in [`docs/build-and-release.md`](../../docs/build-and-release.md) and [`docs/google-play-private-alpha.md`](../../docs/google-play-private-alpha.md).

Upload key (source phase 11):

- Generate the upload keystore (`keytool -genkeypair -keystore upload.keystore -alias upload -keyalg RSA -keysize 2048 -validity 10000`); keystore and passwords in the password manager, never committed.
- Fill `~/.config/artificial-pirate/build.env` with the real values.
- `Tools/build-android.sh release` produces a signed AAB.

Play Console (source phase 12):

- Create the app ([`google-play-private-alpha.md` §4](../../docs/google-play-private-alpha.md#4-app-creation-one-time)) with id `com.pitrified.artificialpirate`, permanent from the first upload.
- Every pre-launch form in [§5](../../docs/google-play-private-alpha.md#5-required-pre-launch-forms-even-for-internal-track).
- Opt in to Play App Signing on the first AAB upload.
- Internal testing track, testers added by email or Google Group.
- Upload the first release AAB, bumping `versionCode` if needed.
- Share the opt-in URL with at least one external tester, who installs from the Play Store and launches the app.
- The [release checklist](../../docs/build-and-release.md#9-release-checklist) fully checked.

Demo-ready (source phase 13):

- Every item of [`functional-specs.md`, Release checks](../../docs/functional-specs.md#release-checks) on the Play Store build.
- Tag the commit `v0.2.0` (AI may prepare the tag command; the push is the user's).

## Done when

- A tester outside the project has installed from the Play Store, the demo-ready list is green, and `v0.2.0` is tagged.
