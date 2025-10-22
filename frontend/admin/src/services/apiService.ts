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

  // Auth methods
  async validateAdminKey(key: string): Promise<boolean> {
    try {
      const response = await fetch(`${this.baseUrl}/admin/validate`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ adminKey: key }),
      });
      return response.ok;
    } catch (error) {
      console.error('Failed to validate admin key:', error);
      return false;
    }
  }

  // Menu management methods
  async getMenuItems(): Promise<MenuItem[]> {
    try {
      const response = await fetch(`${this.baseUrl}/admin/menu`);
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
        headers: {
          'Content-Type': 'application/json',
        },
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
        headers: {
          'Content-Type': 'application/json',
        },
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
        headers: {
          'Content-Type': 'application/json',
        },
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
      
      const response = await fetch(url);
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
      const response = await fetch(`${this.baseUrl}/order`);
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
      const response = await fetch(`${this.baseUrl}/order/${id}`);
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
      
      const response = await fetch(url);
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
