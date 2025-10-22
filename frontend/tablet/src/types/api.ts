// Types matching backend DTOs

export interface MenuItemResponse {
  id: number;
  name: string;
  description: string;
  price: number;
  category: string;
  isAvailable: boolean;
  imageUrl?: string;
  isVegetarian: boolean;
  preparationTimeMinutes: number;
}

export interface CreateOrderRequest {
  tableId: number;
  items: OrderItemRequest[];
  specialInstructions?: string;
}

export interface OrderItemRequest {
  menuItemId: number;
  quantity: number;
  specialInstructions?: string;
}

export interface OrderResponse {
  orderId: number;
  tableId: number;
  status: OrderStatus;
  subTotal: number;
  taxAmount: number;
  totalAmount: number;
  specialInstructions?: string;
  items: OrderItemResponse[];
  createdAt: string;
  updatedAt: string;
}

export interface OrderItemResponse {
  id: number;
  menuItemId: number;
  menuItemName: string;
  menuItemPrice: number;
  quantity: number;
  specialInstructions?: string;
  subtotal: number;
}

export enum OrderStatus {
  Received = 'Received',
  Preparing = 'Preparing',
  Ready = 'Ready',
  Served = 'Served',
  Closed = 'Closed'
}

export interface PaymentQrCodeDto {
  qrData: string;
  amount: number;
  orderId: number;
  merchantId: string;
  upiId: string;
  transactionNote: string;
}

export interface PaymentConfirmationDto {
  orderId: number;
  transactionId: string;
  status: string;
  amount?: number;
  paymentMethod?: string;
  gatewayResponse?: string;
}

export interface PaymentStatusDto {
  paymentId: number;
  orderId: number;
  amount: number;
  paymentMethod: string;
  status: string;
  transactionId: string;
  createdAt: string;
  completedAt?: string;
}

// Frontend-specific types
export interface CartItem {
  menuItem: MenuItemResponse;
  quantity: number;
  specialInstructions?: string;
}

export interface OrderStatusUpdate {
  orderId: number;
  status: OrderStatus;
  timestamp: string;
}

export interface PaymentStatusUpdate {
  orderId: number;
  paymentId: number;
  status: string;
  timestamp: string;
}

// API Response types
export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
}

export interface ApiError {
  message: string;
  statusCode: number;
}
