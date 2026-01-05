import { MenuItem } from 'primeng/api';

export interface AppMenuItem extends MenuItem {
  items?: AppMenuItem[];
  /**
   * Optional list of roles (lower or mixed case) that can see this item. If omitted, the
   * item will be visible to all authenticated users.
   */
  roles?: string[];
}
