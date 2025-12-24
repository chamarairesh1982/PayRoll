# Payroll System UI/UX Redesign Master Plan

## 1. Vision
To transform the existing Payroll system into a world-class institutional software with a focus on:
- **Clarity & Focus**: Minimalist design that highlights critical data.
- **Predictability**: Consistent patterns across all modules.
- **Efficiency**: Reduced clicks and improved data entry workflows.
- **Accessibility**: High contrast, readable typography, and keyboard navigation.

## 2. Progress Tracker

### Phase 1: Foundation (COMPLETED)
- [x] **Design Tokens**: Defined colors, spacing, typography, and shadows.
- [x] **Global Styles**: Applied overrides to PrimeNG components (Buttons, Inputs, Tables, Tags).
- [x] **Power Grid**: Created `app-data-table` with skeleton loading, advanced filtering, and responsive design.
- [x] **Shell Redesign**: Unified Sidebar and Topbar with modern aesthetics.

### Phase 2: Core Module Migration (IN PROGRESS)
- [x] **Employee List**: Migrated to Power Grid and new layout.
- [x] **Payroll Runs List**: Migrated to Power Grid and new layout.
- [ ] **Employee Detail View**: Pending redesign.
- [ ] **Payroll Run Detail/Processing**: Pending redesign.
- [ ] **Configuration Screens**: Pending redesign.

### Phase 3: Advanced Workflows (TODO)
- [ ] **Approvals Dashboard**: High-density list of pending actions.
- [ ] **Interactive Reporting**: Dynamic charts and drill-down tables.
- [ ] **Global Search**: Implementing actual search logic across the app.

## 3. Design Tokens Summary
- **Primary**: Indigo (#4F46E5)
- **Warning**: Amber (#D97706)
- **Danger**: Rose (#E11D48)
- **Success**: Emerald (#059669)
- **Neutral**: Slate (#475569)
- **Surface**: White (#FFFFFF) / Sub-surface (#F8FAFC)
- **Border Radius**: 8px (md)
- **Font**: Inter (Sans-serif)

## 4. Next Technical Tasks
1.  **Employee Details Redesign**:
    *   Split into Tabs (Personal, Employment, Financial, Documents).
    *   Use "Sticky Header" for employee overview and save actions.
2.  **Payroll Run Details**:
    *   Implement "Wizard" or "Step-by-Step" processing view.
    *   Add "Summary Cards" for Gross/Net comparisons.
3.  **Skeleton Loading Expansion**:
    *   Apply to all detail pages.
