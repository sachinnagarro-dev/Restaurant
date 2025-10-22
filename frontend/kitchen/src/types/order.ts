export const OrderStatus = {
  Received: 'Received',
  Preparing: 'Preparing',
  Ready: 'Ready',
  Served: 'Served',
  Closed: 'Closed'
} as const;

export type OrderStatus = typeof OrderStatus[keyof typeof OrderStatus];

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
  status: OrderStatus;
  subTotal: number;
  taxAmount: number;
  totalAmount: number;
  specialInstructions?: string;
  items: OrderItem[];
  createdAt: string;
  updatedAt: string;
}

export interface OrderStatusUpdate {
  orderId: number;
  status: OrderStatus;
  timestamp: string;
}

export interface KitchenFilters {
  status: OrderStatus | 'All';
  searchTerm: string;
}

export interface KitchenState {
  orders: Order[];
  filters: KitchenFilters;
  isLoading: boolean;
  error: string | null;
  connectionStatus: 'connected' | 'disconnected' | 'connecting';
}
