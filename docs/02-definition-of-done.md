# Definition of Done

A backlog item is **Done** when all of these are true:

1. All acceptance criteria in `01-product-backlog.md` are met in **Play Mode** and the PO (User) has seen it working.
2. No errors, and no new warnings, in the Unity Console. Batch compile shows 0 `error CS`.
3. Code follows `CLAUDE.md` conventions (namespace, folder, one MonoBehaviour per file, serialized private fields with tooltips, Input System only, theme-agnostic, identify by component).
4. **Puzzle Master → Validate Wiring** reports no empty required references (scene and prefabs).
5. Any `DEMO TOGGLE` added by the item is marked in code and listed in `guides/demo-toggles.md`, and **both** states compile.
6. The User can explain the code. For AI-generated code, Claude walks through it on request (required by the spec: "you may be asked to explain").
7. Hierarchy objects and assets have clear names.
8. The traceability row in `00-requirements-traceability.md` is updated to ✅ with a note on how it was verified.
9. Committed with `type(PB-xx): … (#issue)` and pushed.
10. Any significant AI interaction is logged in `ai-log/prompt-log.md`. Any third-party asset used is added to `asset-credits.md`.
