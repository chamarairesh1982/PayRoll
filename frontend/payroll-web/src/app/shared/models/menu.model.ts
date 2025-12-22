import { MenuItem } from 'primeng/api';

export interface AppMenuItem extends MenuItem {
  items?: AppMenuItem[];
}
