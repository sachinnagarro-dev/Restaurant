import Dexie, { Table } from 'dexie';
import { MenuItemResponse } from '../types/api';

export interface CachedMenuItem extends MenuItemResponse {
  cachedAt: number;
  expiresAt: number;
}

export interface CachedMenuCategory {
  category: string;
  items: CachedMenuItem[];
  cachedAt: number;
  expiresAt: number;
}

export interface CachedOrder {
  id: string;
  order: any;
  timestamp: number;
  status: 'pending' | 'submitted' | 'failed';
}

class TableOrderDatabase extends Dexie {
  menuItems!: Table<CachedMenuItem>;
  menuCategories!: Table<CachedMenuCategory>;
  orders!: Table<CachedOrder>;

  constructor() {
    super('TableOrderDatabase');
    this.version(1).stores({
      menuItems: 'id, name, category, cachedAt, expiresAt',
      menuCategories: 'category, cachedAt, expiresAt',
      orders: 'id, timestamp, status'
    });
  }
}

export const db = new TableOrderDatabase();

export class CacheService {
  private static readonly CACHE_DURATION = 24 * 60 * 60 * 1000; // 24 hours
  private static readonly API_BASE_URL = 'http://localhost:5000/api';

  // Menu caching methods
  static async cacheMenuItems(items: MenuItemResponse[]): Promise<void> {
    try {
      const now = Date.now();
      const expiresAt = now + this.CACHE_DURATION;

      const cachedItems: CachedMenuItem[] = items.map(item => ({
        ...item,
        cachedAt: now,
        expiresAt
      }));

      await db.menuItems.clear();
      await db.menuItems.bulkAdd(cachedItems);
      
      console.log(`Cached ${items.length} menu items`);
    } catch (error) {
      console.error('Failed to cache menu items:', error);
    }
  }

  static async getCachedMenuItems(): Promise<MenuItemResponse[]> {
    try {
      const now = Date.now();
      const cachedItems = await db.menuItems
        .where('expiresAt')
        .above(now)
        .toArray();

      if (cachedItems.length === 0) {
        console.log('No valid cached menu items found');
        return [];
      }

      console.log(`Retrieved ${cachedItems.length} cached menu items`);
      return cachedItems.map(item => ({
        id: item.id,
        name: item.name,
        description: item.description,
        price: item.price,
        category: item.category,
        isAvailable: item.isAvailable,
        imageUrl: item.imageUrl,
        isVegetarian: item.isVegetarian,
        preparationTimeMinutes: item.preparationTimeMinutes
      }));
    } catch (error) {
      console.error('Failed to get cached menu items:', error);
      return [];
    }
  }

  static async cacheMenuCategories(categories: string[]): Promise<void> {
    try {
      const now = Date.now();
      const expiresAt = now + this.CACHE_DURATION;

      const cachedCategories: CachedMenuCategory[] = categories.map(category => ({
        category,
        items: [],
        cachedAt: now,
        expiresAt
      }));

      await db.menuCategories.clear();
      await db.menuCategories.bulkAdd(cachedCategories);
      
      console.log(`Cached ${categories.length} menu categories`);
    } catch (error) {
      console.error('Failed to cache menu categories:', error);
    }
  }

  static async getCachedMenuCategories(): Promise<string[]> {
    try {
      const now = Date.now();
      const cachedCategories = await db.menuCategories
        .where('expiresAt')
        .above(now)
        .toArray();

      console.log(`Retrieved ${cachedCategories.length} cached menu categories`);
      return cachedCategories.map(cat => cat.category);
    } catch (error) {
      console.error('Failed to get cached menu categories:', error);
      return [];
    }
  }

  // Order caching methods
  static async cacheOrder(orderId: string, order: any): Promise<void> {
    try {
      await db.orders.add({
        id: orderId,
        order,
        timestamp: Date.now(),
        status: 'pending'
      });
      
      console.log(`Cached order: ${orderId}`);
    } catch (error) {
      console.error('Failed to cache order:', error);
    }
  }

  static async getCachedOrders(): Promise<CachedOrder[]> {
    try {
      const orders = await db.orders.toArray();
      console.log(`Retrieved ${orders.length} cached orders`);
      return orders;
    } catch (error) {
      console.error('Failed to get cached orders:', error);
      return [];
    }
  }

  static async updateOrderStatus(orderId: string, status: 'pending' | 'submitted' | 'failed'): Promise<void> {
    try {
      await db.orders.update(orderId, { status });
      console.log(`Updated order status: ${orderId} -> ${status}`);
    } catch (error) {
      console.error('Failed to update order status:', error);
    }
  }

  // Cache management methods
  static async clearExpiredCache(): Promise<void> {
    try {
      const now = Date.now();
      
      await db.menuItems.where('expiresAt').below(now).delete();
      await db.menuCategories.where('expiresAt').below(now).delete();
      
      console.log('Cleared expired cache');
    } catch (error) {
      console.error('Failed to clear expired cache:', error);
    }
  }

  static async clearAllCache(): Promise<void> {
    try {
      await db.menuItems.clear();
      await db.menuCategories.clear();
      await db.orders.clear();
      
      console.log('Cleared all cache');
    } catch (error) {
      console.error('Failed to clear all cache:', error);
    }
  }

  // Network status methods
  static async fetchMenuItemsWithCache(): Promise<MenuItemResponse[]> {
    if (navigator.onLine) {
      try {
        const response = await fetch(`${this.API_BASE_URL}/menu`);
        if (response.ok) {
          const items = await response.json();
          await this.cacheMenuItems(items);
          return items;
        }
      } catch (error) {
        console.error('Failed to fetch menu items from server:', error);
      }
    }

    // Fallback to cached data
    return await this.getCachedMenuItems();
  }

  static async fetchMenuCategoriesWithCache(): Promise<string[]> {
    if (navigator.onLine) {
      try {
        const response = await fetch(`${this.API_BASE_URL}/menu/categories`);
        if (response.ok) {
          const categories = await response.json();
          await this.cacheMenuCategories(categories);
          return categories;
        }
      } catch (error) {
        console.error('Failed to fetch menu categories from server:', error);
      }
    }

    // Fallback to cached data
    return await this.getCachedMenuCategories();
  }
}

export default CacheService;
