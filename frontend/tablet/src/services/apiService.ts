import { MenuItemResponse, CreateOrderRequest, OrderResponse, PaymentQrCodeDto } from '../types/api';

class ApiService {
  private baseUrl: string = '/api';

  // Menu API methods
  async getMenuItems(): Promise<MenuItemResponse[]> {
    try {
      const response = await fetch(`${this.baseUrl}/menu`);
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      return await response.json();
    } catch (error) {
      console.error('Failed to fetch menu items:', error);
      throw error;
    }
  }

  async getMenuCategories(): Promise<string[]> {
    try {
      const response = await fetch(`${this.baseUrl}/menu/categories`);
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      return await response.json();
    } catch (error) {
      console.error('Failed to fetch menu categories:', error);
      throw error;
    }
  }

  async getMenuItemById(id: number): Promise<MenuItemResponse | null> {
    try {
      const response = await fetch(`${this.baseUrl}/menu/${id}`);
      if (response.status === 404) {
        return null;
      }
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      return await response.json();
    } catch (error) {
      console.error('Failed to fetch menu item:', error);
      throw error;
    }
  }

  // Order API methods
  async createOrder(order: CreateOrderRequest): Promise<OrderResponse> {
    try {
      const response = await fetch(`${this.baseUrl}/order`, { // Changed from /orders to /order
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(order),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
      }

      return await response.json();
    } catch (error) {
      console.error('Failed to create order:', error);
      throw error;
    }
  }

  async getOrderById(id: number): Promise<OrderResponse | null> {
    try {
      const response = await fetch(`${this.baseUrl}/order/${id}`); // Changed from /orders to /order
      if (response.status === 404) {
        return null;
      }
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      return await response.json();
    } catch (error) {
      console.error('Failed to fetch order:', error);
      throw error;
    }
  }

  async updateOrderStatus(id: number, status: string): Promise<OrderResponse> {
    try {
      const response = await fetch(`${this.baseUrl}/order/${id}/status`, { // Changed from /orders to /order
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ status }),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
      }

      return await response.json();
    } catch (error) {
      console.error('Failed to update order status:', error);
      throw error;
    }
  }

  // Payment API methods
  async getPaymentQrCode(orderId: number): Promise<PaymentQrCodeDto> {
    try {
      const response = await fetch(`${this.baseUrl}/payments/qrcode/${orderId}`);
      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
      }
      return await response.json();
    } catch (error) {
      console.error('Failed to get payment QR code:', error);
      throw error;
    }
  }

  async confirmPayment(paymentData: {
    orderId: number;
    transactionId: string;
    status: string;
    amount?: number;
    paymentMethod?: string;
  }): Promise<any> {
    try {
      const response = await fetch(`${this.baseUrl}/payments/confirm`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(paymentData),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
      }

      return await response.json();
    } catch (error) {
      console.error('Failed to confirm payment:', error);
      throw error;
    }
  }

  async getPaymentStatus(orderId: number): Promise<any[]> {
    try {
      const response = await fetch(`${this.baseUrl}/payments/status/${orderId}`);
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      return await response.json();
    } catch (error) {
      console.error('Failed to get payment status:', error);
      throw error;
    }
  }

  // Table API methods
  async getTables(): Promise<any[]> {
    try {
      const response = await fetch(`${this.baseUrl}/tables`);
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      return await response.json();
    } catch (error) {
      console.error('Failed to fetch tables:', error);
      throw error;
    }
  }

  async getTableById(id: number): Promise<any | null> {
    try {
      const response = await fetch(`${this.baseUrl}/tables/${id}`);
      if (response.status === 404) {
        return null;
      }
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      return await response.json();
    } catch (error) {
      console.error('Failed to fetch table:', error);
      throw error;
    }
  }

  async getTableByNumber(number: number): Promise<any | null> {
    try {
      const response = await fetch(`${this.baseUrl}/tables/number/${number}`);
      if (response.status === 404) {
        return null;
      }
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      return await response.json();
    } catch (error) {
      console.error('Failed to fetch table by number:', error);
      throw error;
    }
  }

  async updateTableStatus(id: number, status: string): Promise<any> {
    try {
      const response = await fetch(`${this.baseUrl}/tables/${id}/status/${status}`, {
        method: 'PUT',
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
      }

      return await response.json();
    } catch (error) {
      console.error('Failed to update table status:', error);
      throw error;
    }
  }

  // Health check
  async healthCheck(): Promise<boolean> {
    try {
      const response = await fetch(`${this.baseUrl}/health`, {
        method: 'GET',
        timeout: 5000,
      } as any);
      return response.ok;
    } catch (error) {
      console.error('Health check failed:', error);
      return false;
    }
  }
}

// Export singleton instance
export const apiService = new ApiService();
export default apiService;
