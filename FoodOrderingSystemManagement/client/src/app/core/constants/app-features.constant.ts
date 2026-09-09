export interface FeatureItem {
  key: string;
  label: string;
}

export interface FeatureGroup {
  module: string;
  moduleKey: string;
  icon: string;
  features: FeatureItem[];
}

export const APP_FEATURES: FeatureGroup[] = [
  {
    module: 'Super Admin', moduleKey: 'super_admin', icon: 'admin_panel_settings',
    features: [
      { key: 'super_admin.view', label: 'View Organizations' },
      { key: 'super_admin.create', label: 'Create Organization' },
      { key: 'super_admin.edit', label: 'Edit Organization' },
      { key: 'super_admin.delete', label: 'Delete Organization' },
      { key: 'super_admin.manage_admins', label: 'Manage Organization Admins' },
    ],
  },
  {
    module: 'Dashboard', moduleKey: 'dashboard', icon: 'dashboard',
    features: [
      { key: 'dashboard.view', label: 'View Dashboard' },
    ],
  },
  {
    module: 'POS', moduleKey: 'pos', icon: 'point_of_sale',
    features: [
      { key: 'pos.view',        label: 'Open POS Screen' },
      { key: 'pos.place_order', label: 'Place / Modify Order' },
    ],
  },
  {
    module: 'Waiter', moduleKey: 'waiter', icon: 'room_service',
    features: [
      { key: 'waiter.view',         label: 'Open Waiter Screen' },
      { key: 'waiter.take_order',   label: 'Take Order from Table' },
      { key: 'waiter.edit_order',   label: 'Edit / Update Pending Order' },
      { key: 'waiter.request_bill', label: 'Request Bill for Table' },
    ],
  },
  {
    module: 'Orders', moduleKey: 'orders', icon: 'receipt_long',
    features: [
      { key: 'orders.view',            label: 'View Orders' },
      { key: 'orders.view_details',    label: 'View Order Details' },
      { key: 'orders.search',          label: 'Search Orders' },
      { key: 'orders.history',         label: 'View Order History' },
      { key: 'orders.online',          label: 'View Online (Takeaway) Orders' },
      { key: 'orders.offline',         label: 'View Offline (Dine-in) Orders' },
      { key: 'orders.cancel',          label: 'Cancel Order' },
      { key: 'orders.reopen',          label: 'Reopen Cancelled Order' },
      { key: 'orders.complete',        label: 'Complete / Confirm Payment' },
      { key: 'orders.generate_bill',   label: 'Generate Bill' },
      { key: 'orders.delete',          label: 'Delete Order (Permanent)' },
    ],
  },
  {
    module: 'Kitchen Display', moduleKey: 'kds', icon: 'restaurant',
    features: [
      { key: 'kds.view',          label: 'View Kitchen Screen' },
      { key: 'kds.accept_order',  label: 'Accept & Start Preparing' },
      { key: 'kds.reject_order',  label: 'Reject Order' },
      { key: 'kds.mark_ready',    label: 'Mark Order Ready' },
      { key: 'kds.mark_served',   label: 'Mark Order Served' },
      { key: 'kds.reject_item',   label: 'Reject Individual Item' },
      { key: 'kds.delay_order',   label: 'Delay Order' },
      { key: 'kds.print_slip',    label: 'Print Kitchen Slip' },
      { key: 'kds.kitchen_notes', label: 'Add Kitchen Notes' },
    ],
  },
  {
    module: 'Tables', moduleKey: 'tables', icon: 'table_restaurant',
    features: [
      { key: 'tables.view', label: 'View Tables' },
      { key: 'tables.add',  label: 'Add Table' },
      { key: 'tables.edit', label: 'Edit / Delete Table' },
    ],
  },
  {
    module: 'Reports', moduleKey: 'reports', icon: 'bar_chart',
    features: [
      { key: 'reports.view', label: 'View Reports' },
    ],
  },
  {
    module: 'Categories', moduleKey: 'categories', icon: 'category',
    features: [
      { key: 'categories.view', label: 'View Categories' },
      { key: 'categories.add',  label: 'Add Category' },
      { key: 'categories.edit', label: 'Edit / Delete Category' },
    ],
  },
  {
    module: 'Food Items', moduleKey: 'food_items', icon: 'fastfood',
    features: [
      { key: 'food_items.view', label: 'View Food Items' },
      { key: 'food_items.add',  label: 'Add Food Item' },
      { key: 'food_items.edit', label: 'Edit / Delete Food Item' },
    ],
  },
  {
    module: 'Add-Ons', moduleKey: 'addons', icon: 'add_circle',
    features: [
      { key: 'addons.view', label: 'View Add-Ons' },
      { key: 'addons.add',  label: 'Add Add-On' },
      { key: 'addons.edit', label: 'Edit / Delete Add-On' },
    ],
  },
  {
    module: 'Inventory', moduleKey: 'inventory', icon: 'inventory_2',
    features: [
      { key: 'inventory.view',     label: 'View Inventory' },
      { key: 'inventory.add',      label: 'Add Inventory Item' },
      { key: 'inventory.edit',     label: 'Edit / Delete Item' },
      { key: 'inventory.purchase', label: 'Purchase Stock' },
      { key: 'inventory.waste',    label: 'Record Waste' },
      { key: 'inventory.transfer', label: 'Transfer Stock' },
      { key: 'inventory.audit',    label: 'View Audit Trail' },
    ],
  },
  {
    module: 'Suppliers', moduleKey: 'suppliers', icon: 'storefront',
    features: [
      { key: 'suppliers.view',           label: 'View Suppliers' },
      { key: 'suppliers.add',            label: 'Add Supplier' },
      { key: 'suppliers.edit',           label: 'Edit Supplier' },
      { key: 'suppliers.delete',         label: 'Delete Supplier' },
      { key: 'suppliers.purchase_order', label: 'Create Purchase Order' },
      { key: 'suppliers.payment',        label: 'Record Payment' },
    ],
  },
  {
    module: 'Finance (Ledger)', moduleKey: 'ledger', icon: 'menu_book',
    features: [
      { key: 'ledger.view', label: 'View Ledger' },
      { key: 'ledger.add',  label: 'Add Ledger Entry' },
    ],
  },
  {
    module: 'CRM (Customers)', moduleKey: 'customers', icon: 'people',
    features: [
      { key: 'customers.view',     label: 'View Customers' },
      { key: 'customers.add',      label: 'Add Customer' },
      { key: 'customers.edit',     label: 'Edit Customer' },
      { key: 'customers.delete',   label: 'Delete Customer' },
      { key: 'customers.wallet',   label: 'Manage Wallet / Adjust Points' },
      { key: 'customers.loyalty',  label: 'View Loyalty Points & History' },
      { key: 'customers.membership', label: 'View Membership Tier' },
      { key: 'customers.addresses', label: 'Manage Saved Addresses' },
      { key: 'customers.notes',    label: 'View / Edit Customer Notes' },
      { key: 'customers.history',  label: 'View Purchase History' },
    ],
  },
  {
    module: 'Users', moduleKey: 'users', icon: 'manage_accounts',
    features: [
      { key: 'users.view',          label: 'View Users' },
      { key: 'users.add',           label: 'Add User' },
      { key: 'users.edit',          label: 'Edit User' },
      { key: 'users.delete',        label: 'Delete User' },
      { key: 'users.toggle_active', label: 'Activate / Deactivate User' },
    ],
  },
  {
    module: 'Employees (HR)', moduleKey: 'employees', icon: 'badge',
    features: [
      { key: 'employees.view',        label: 'View Employees' },
      { key: 'employees.edit',        label: 'Edit Employee Info' },
      { key: 'employees.delete',      label: 'Delete Employee' },
      { key: 'employees.attendance',  label: 'Manage Attendance' },
      { key: 'employees.salary',      label: 'Manage Salary' },
      { key: 'employees.shifts',      label: 'Manage Shifts' },
      { key: 'employees.performance', label: 'Manage Performance Reviews' },
      { key: 'employees.assign_role', label: 'Assign / Change Role' },
    ],
  },
  {
    module: 'Roles & Permissions', moduleKey: 'roles', icon: 'badge',
    features: [
      { key: 'roles.view',        label: 'View Roles' },
      { key: 'roles.add',         label: 'Add Role' },
      { key: 'roles.permissions', label: 'Manage Permissions' },
    ],
  },
  {
    module: 'Offers & Promotions', moduleKey: 'promotions', icon: 'local_offer',
    features: [
      { key: 'promotions.view',   label: 'View Promotions' },
      { key: 'promotions.create', label: 'Create Promotion' },
      { key: 'promotions.edit',   label: 'Edit / Update Promotion' },
      { key: 'promotions.delete', label: 'Delete Promotion' },
      { key: 'promotions.toggle', label: 'Toggle Active Status' },
    ],
  },
  {
    module: 'Delivery Management', moduleKey: 'delivery', icon: 'local_shipping',
    features: [
      { key: 'delivery.view',        label: 'View Deliveries' },
      { key: 'delivery.assign',      label: 'Assign Driver' },
      { key: 'delivery.update_status', label: 'Update Delivery Status' },
      { key: 'delivery.manage_drivers', label: 'Manage Drivers' },
      { key: 'delivery.manage_charges', label: 'Manage Delivery Charges' },
      { key: 'delivery.manage_integrations', label: 'Manage 3rd Party Integrations' },
    ],
  },
  {
    module: 'Feature Toggles', moduleKey: 'feature_toggles', icon: 'toggle_on',
    features: [
      { key: 'feature_toggles.view',   label: 'View Feature Toggles' },
      { key: 'feature_toggles.toggle', label: 'Enable / Disable Features' },
    ],
  },
  {
    module: 'Integration Settings', moduleKey: 'integrations', icon: 'cable',
    features: [
      { key: 'integrations.view',      label: 'View Integrations' },
      { key: 'integrations.configure', label: 'Configure Integration' },
    ],
  },
  {
    module: 'Settings', moduleKey: 'settings', icon: 'settings',
    features: [
      { key: 'settings.view', label: 'View Settings' },
      { key: 'settings.edit', label: 'Edit Settings' },
    ],
  },
];

export const ALL_FEATURE_KEYS: string[] = APP_FEATURES.flatMap(g => g.features.map(f => f.key));
