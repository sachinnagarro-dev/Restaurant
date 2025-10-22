// Auth types
export interface AuthState {
  isAuthenticated: boolean;
  adminKey: string | null;
}

// Menu item types
export interface MenuItem {
  id: number;
  name: string;
  description: string;
  price: number;
  category: string;
  isAvailable: boolean;
  isVegetarian: boolean;
  imageUrl?: string;
  preparationTimeMinutes: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateMenuItemRequest {
  name: string;
  description: string;
  price: number;
  category: string;
  isAvailable: boolean;
  isVegetarian: boolean;
  imageUrl?: string;
  preparationTimeMinutes: number;
}

export interface UpdateMenuItemRequest extends Partial<CreateMenuItemRequest> {
  id: number;
}

// Order types
export interface OrderSummary {
  id: number;
  tableId: number;
  tableNumber: number;
  status: string;
  totalAmount: number;
  itemCount: number;
  createdAt: string;
}

export interface OrderItem {
  id: number;
  menuItemId: number;
  menuItemName: string;
  quantity: number;
  unitPrice: number;
  specialInstructions?: string;
}

export interface Order {
  id: number;
  tableId: number;
  tableNumber: number;
  status: string;
  subTotal: number;
  taxAmount: number;
  totalAmount: number;
  specialInstructions?: string;
  items: OrderItem[];
  createdAt: string;
  updatedAt: string;
}

// Analytics types
export interface DailyAnalytics {
  date: string;
  totalOrders: number;
  totalRevenue: number;
  averageOrderValue: number;
  topItems: Array<{
    menuItemId: number;
    menuItemName: string;
    quantitySold: number;
    revenue: number;
  }>;
}

export interface SalesExportData {
  orderId: number;
  tableNumber: number;
  itemName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  orderDate: string;
  status: string;
}
