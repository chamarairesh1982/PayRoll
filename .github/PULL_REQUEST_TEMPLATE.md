## Description
<!-- Provide a brief description of the changes in this PR -->

## Type of Change
<!-- Mark the relevant option with an 'x' -->

- [ ] 🐛 Bug fix (non-breaking change which fixes an issue)
- [ ] ✨ New feature (non-breaking change which adds functionality)
- [ ] 💥 Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] 📝 Documentation update
- [ ] 🎨 Code style update (formatting, renaming)
- [ ] ♻️ Refactoring (no functional changes)
- [ ] ⚡ Performance improvement
- [ ] ✅ Test update
- [ ] 🔧 Configuration change
- [ ] 🏗️ Infrastructure change

## Related Issues
<!-- Link to related issues using #issue_number -->

Closes #
Related to #

## Changes Made
<!-- List the main changes made in this PR -->

- 
- 
- 

## Testing
<!-- Describe the tests you ran and how to reproduce them -->

### Test Coverage
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] E2E tests added/updated
- [ ] Manual testing performed

### Test Instructions
<!-- Provide step-by-step instructions to test this PR -->

1. 
2. 
3. 

## Screenshots
<!-- If applicable, add screenshots to help explain your changes -->

## Definition of Done Checklist

### Code Quality
- [ ] Code follows the project's coding standards (see `.editorconfig`)
- [ ] Code has been self-reviewed
- [ ] Code is well-commented, particularly in hard-to-understand areas
- [ ] No unnecessary console.log or debug statements
- [ ] No commented-out code (unless with explanation)

### Testing
- [ ] All existing tests pass locally
- [ ] New tests have been added for new functionality
- [ ] Test coverage is maintained or improved
- [ ] E2E tests pass (if applicable)

### Documentation
- [ ] Code changes are reflected in documentation
- [ ] API documentation updated (if applicable)
- [ ] README updated (if applicable)
- [ ] ADR created for architectural decisions (if applicable)
- [ ] AGENTS.md updated (if structure/conventions changed)

### Database
- [ ] Database migrations created (if applicable)
- [ ] Migration tested locally
- [ ] Rollback tested (if applicable)
- [ ] Seed data updated (if applicable)

### Security
- [ ] No sensitive data (passwords, keys, tokens) in code
- [ ] Input validation added for user inputs
- [ ] Authorization checks in place
- [ ] SQL injection prevention verified (parameterized queries)
- [ ] XSS prevention verified

### Performance
- [ ] No N+1 query issues
- [ ] Appropriate indexes added (if database changes)
- [ ] Large lists use pagination
- [ ] Async/await used for I/O operations

### Accessibility (Frontend)
- [ ] Semantic HTML used
- [ ] ARIA labels added where needed
- [ ] Keyboard navigation works
- [ ] Color contrast meets WCAG standards

### Browser Compatibility (Frontend)
- [ ] Tested in Chrome
- [ ] Tested in Firefox
- [ ] Tested in Edge
- [ ] Tested in Safari (if available)

### Deployment
- [ ] Environment variables documented (if new ones added)
- [ ] Configuration changes documented
- [ ] Deployment instructions updated (if applicable)
- [ ] Rollback plan considered

### Clean Architecture (Backend)
- [ ] Dependencies flow inward (Domain ← Application ← Infrastructure)
- [ ] No domain logic in controllers
- [ ] DTOs used for API contracts
- [ ] Repository pattern followed
- [ ] CQRS pattern followed (Commands/Queries)

### Angular Best Practices (Frontend)
- [ ] Components follow single responsibility principle
- [ ] Services used for business logic
- [ ] OnPush change detection used (where applicable)
- [ ] Reactive forms used (not template-driven)
- [ ] Lazy loading used for feature modules
- [ ] trackBy used in *ngFor loops

### Git
- [ ] Branch name follows convention (feature/, bugfix/, hotfix/)
- [ ] Commits are atomic and well-described
- [ ] No merge commits (rebased on target branch)
- [ ] No conflicts with target branch

### AI Agent Friendly
- [ ] Code structure follows AGENTS.md guidelines
- [ ] Clear naming conventions used
- [ ] Bounded context boundaries respected
- [ ] File organization follows documented patterns

## Breaking Changes
<!-- If this PR introduces breaking changes, describe them here -->

**None** / **Yes** (describe below):

## Migration Guide
<!-- If breaking changes, provide migration guide for users/developers -->

## Additional Notes
<!-- Any additional information that reviewers should know -->

## Checklist for Reviewers
<!-- For code reviewers -->

- [ ] Code changes align with the description
- [ ] Code follows project conventions
- [ ] Tests are adequate and pass
- [ ] Documentation is sufficient
- [ ] No security concerns
- [ ] Performance considerations addressed
- [ ] Ready to merge

---

**By submitting this PR, I confirm that:**
- I have read and followed the [DEVELOPMENT.md](../DEVELOPMENT.md) guide
- I have followed the [AGENTS.md](../AGENTS.md) conventions
- I have tested my changes locally
- I am ready for code review
