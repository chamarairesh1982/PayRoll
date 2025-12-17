# How to Know Codex Did It Right

Use this checklist after each prompt to quickly verify the payroll platform changes are correct and aligned with our expectations.

- [ ] **Migration added** – Ensure every data model change has a corresponding migration file committed and applied in the solution.
- [ ] **No deleted entities** – Confirm that domain entities are not removed outright; refactors should be additive or carefully deprecated.
- [ ] **PayrollService extended, not rewritten** – Review changes to `PayrollService` to confirm existing behavior is preserved and new logic is added through extensions rather than wholesale rewrites.
- [ ] **Tests exist** – Check that new or updated behavior is covered by automated tests in the backend and/or frontend as appropriate.
- [ ] **No hardcoded Sri Lanka rules** – Verify that business rules are configurable and not hardcoded specifically for Sri Lanka unless explicitly required.
- [ ] **Code matches MD docs** – Compare the implemented logic against the Markdown documentation in `docs/` to ensure alignment with documented workflows and requirements.

Complete all items before considering the work done.
