import type { 
  MenuItem, 
  CreateMenuItemRequest, 
  UpdateMenuItemRequest, 
  OrderSummary, 
  Order, 
  DailyAnalytics,
  SalesExportData 
} from '../types';

class ApiService {
  private baseUrl: string = '/api';
  private authToken: string | null = null;

  // Set authentication token
  setAuthToken(token: string): void {
    this.authToken = token;
  }

  // Clear authentication token
  clearAuthToken(): void {
    this.authToken = null;
  }

  // Get headers with authentication
  private getHeaders(): HeadersInit {
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
    };
    
    if (this.authToken) {
      headers['Authorization'] = `Bearer ${this.authToken}`;
    }
    
    return headers;
  }

  // Auth methods
  async loginWithAdminKey(adminKey: string): Promise<{ token: string; role: string; expiresAt: string }> {
    try {
      const response = await fetch(`${this.baseUrl}/auth/validate`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ adminKey }),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || 'Invalid admin key');
      }

      const data = await response.json();
      this.setAuthToken(data.token);
      return data;
    } catch (error) {
      console.error('Failed to login with admin key:', error);
      throw error;
    }
  }

  async loginWithCredentials(username: string, password: string): Promise<{ token: string; role: string; expiresAt: string }> {
    try {
      const response = await fetch(`${this.baseUrl}/auth/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ username, password }),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || 'Invalid credentials');
      }

      const data = await response.json();
      this.setAuthToken(data.token);
      return data;
    } catch (error) {
      console.error('Failed to login with credentials:', error);
      throw error;
    }
  }

  async getCurrentUser(): Promise<{ username: string; role: string }> {
    try {
      const response = await fetch(`${this.baseUrl}/auth/me`, {
        headers: this.getHeaders(),
      });

      if (!response.ok) {
        throw new Error('Failed to get current user');
      }

      return await response.json();
    } catch (error) {
      console.error('Failed to get current user:', error);
      throw error;
    }
  }

  // Legacy method for backward compatibility
  async validateAdminKey(key: string): Promise<boolean> {
    try {
      const response = await this.loginWithAdminKey(key);
      return !!response.token;
    } catch (error) {
      return false;
    }
  }

  // Menu management methods
  async getMenuItems(): Promise<MenuItem[]> {
    try {
      const response = await fetch(`${this.baseUrl}/admin/menu`, {
        headers: this.getHeaders(),
      });
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      return await response.json();
    } catch (error) {
      console.error('Failed to fetch menu items:', error);
      throw error;
    }
  }

  async createMenuItem(item: CreateMenuItemRequest): Promise<MenuItem> {
    try {
      const response = await fetch(`${this.baseUrl}/admin/menu`, {
        method: 'POST',
        headers: this.getHeaders(),
        body: JSON.stringify(item),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
      }

      return await response.json();
    } catch (error) {
      console.error('Failed to create menu item:', error);
      throw error;
    }
  }

  async updateMenuItem(item: UpdateMenuItemRequest): Promise<MenuItem> {
    try {
      const response = await fetch(`${this.baseUrl}/admin/menu/${item.id}`, {
        method: 'PUT',
        headers: this.getHeaders(),
        body: JSON.stringify(item),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
      }

      return await response.json();
    } catch (error) {
      console.error('Failed to update menu item:', error);
      throw error;
    }
  }

  async deleteMenuItem(id: number): Promise<void> {
    try {
      const response = await fetch(`${this.baseUrl}/admin/menu/${id}`, {
        method: 'DELETE',
        headers: this.getHeaders(),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
      }
    } catch (error) {
      console.error('Failed to delete menu item:', error);
      throw error;
    }
  }

  async toggleMenuItemAvailability(id: number, isAvailable: boolean): Promise<MenuItem> {
    try {
      const response = await fetch(`${this.baseUrl}/admin/menu/${id}/availability`, {
        method: 'PATCH',
        headers: this.getHeaders(),
        body: JSON.stringify({ isAvailable }),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
      }

      return await response.json();
    } catch (error) {
      console.error('Failed to toggle menu item availability:', error);
      throw error;
    }
  }

  // Analytics methods
  async getDailyAnalytics(date?: string): Promise<DailyAnalytics> {
    try {
      const url = date 
        ? `${this.baseUrl}/admin/analytics/daily?date=${date}`
        : `${this.baseUrl}/admin/analytics/daily`;
      
      const response = await fetch(url, {
        headers: this.getHeaders(),
      });
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      return await response.json();
    } catch (error) {
      console.error('Failed to fetch daily analytics:', error);
      throw error;
    }
  }

  async getOrders(): Promise<OrderSummary[]> {
    try {
      const response = await fetch(`${this.baseUrl}/order`, {
        headers: this.getHeaders(),
      });
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      return await response.json();
    } catch (error) {
      console.error('Failed to fetch orders:', error);
      throw error;
    }
  }

  async getOrderById(id: number): Promise<Order> {
    try {
      const response = await fetch(`${this.baseUrl}/order/${id}`, {
        headers: this.getHeaders(),
      });
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      return await response.json();
    } catch (error) {
      console.error('Failed to fetch order:', error);
      throw error;
    }
  }

  // Export methods
  async exportDailySales(date?: string): Promise<SalesExportData[]> {
    try {
      const url = date 
        ? `${this.baseUrl}/admin/export/sales?date=${date}`
        : `${this.baseUrl}/admin/export/sales`;
      
      const response = await fetch(url, {
        headers: this.getHeaders(),
      });
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      return await response.json();
    } catch (error) {
      console.error('Failed to export sales data:', error);
      throw error;
    }
  }

  // Utility method to download CSV
  downloadCSV(data: SalesExportData[], filename: string): void {
    const headers = ['Order ID', 'Table Number', 'Item Name', 'Quantity', 'Unit Price', 'Total Price', 'Order Date', 'Status'];
    const csvContent = [
      headers.join(','),
      ...data.map(row => [
        row.orderId,
        row.tableNumber,
        `"${row.itemName}"`,
        row.quantity,
        row.unitPrice,
        row.totalPrice,
        row.orderDate,
        row.status
      ].join(','))
    ].join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    link.setAttribute('href', url);
    link.setAttribute('download', filename);
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }
}

export const apiService = new ApiService();
export default apiService;
