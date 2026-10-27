# ADR-0007: CharacterController player and a third-person, cursor-locked camera

- **Status:** Accepted
- **Date:** 2026-10-27
- **Deciders:** User (PO), proposed by Claude

## Context
The spec: move with WASD, rotate with the mouse, attack nearby monsters. Unlike PA1 there is no jump, speed toggle or pushing of physics objects, and spells must be able to hit the player. The PO chose a third-person view with the cursor locked during play (decision 10).

## Decision
- **Player:** Unity's `CharacterController` (not a Rigidbody as in PA1): WASD relative to the camera yaw, smoothed acceleration, simple gravity, no jumping. It collides with walls and props without physics tuning; monster spells (kinematic trigger Rigidbodies) still get trigger events from it.
- **Camera:** `CameraRig` (Cameras) orbits behind the player. Mouse X turns the player's yaw (the player always faces the camera direction), mouse Y pitches the camera (clamped). A SphereCast pulls the camera in front of walls. The cursor is locked and hidden while Playing and free on Intro / Won / Lost (ADR-0006).
- Speed and sensitivity come from `config.json` (ADR-0004).

## Consequences
Simpler and more predictable movement than a Rigidbody for a puzzle-adventure; no jump, by design (the spec doesn't ask for one). The melee attack (ADR-0008) uses the player's facing, so "face the camera" keeps aiming intuitive.
