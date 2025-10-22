import type { Order, OrderStatus } from '../types/order';

class ApiService {
  private baseUrl: string = '/api';

  // Order API methods
  async getOrders(): Promise<Order[]> {
    try {
      const response = await fetch(`${this.baseUrl}/order`);
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      const orderSummaries = await response.json();
      
      // Fetch full details for each order
      const fullOrders = await Promise.all(
        orderSummaries.map((summary: any) => this.getOrderById(summary.id))
      );
      
      return fullOrders.filter(order => order !== null) as Order[];
    } catch (error) {
      console.error('Failed to fetch orders:', error);
      throw error;
    }
  }

  async getOrderById(id: number): Promise<Order | null> {
    try {
      const response = await fetch(`${this.baseUrl}/order/${id}`);
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

  async updateOrderStatus(id: number, status: OrderStatus): Promise<Order> {
    try {
      const response = await fetch(`${this.baseUrl}/order/${id}/status`, {
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
