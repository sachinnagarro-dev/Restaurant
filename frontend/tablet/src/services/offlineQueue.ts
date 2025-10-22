import { CreateOrderRequest } from '../types/api';

interface QueuedOrder {
  id: string;
  order: CreateOrderRequest;
  timestamp: number;
  retryCount: number;
  maxRetries: number;
}

class OfflineQueueService {
  private queue: QueuedOrder[] = [];
  private isOnline: boolean = navigator.onLine;
  private retryInterval: number = 5000; // 5 seconds
  private maxRetries: number = 3;
  private apiBaseUrl: string = 'http://localhost:5000/api';

  constructor() {
    this.setupEventListeners();
    this.loadQueueFromStorage();
    this.startRetryProcess();
  }

  private setupEventListeners(): void {
    window.addEventListener('online', () => {
      console.log('Back online, processing queued orders...');
      this.isOnline = true;
      this.processQueue();
    });

    window.addEventListener('offline', () => {
      console.log('Gone offline, orders will be queued...');
      this.isOnline = false;
    });
  }

  private loadQueueFromStorage(): void {
    try {
      const stored = localStorage.getItem('offlineQueue');
      if (stored) {
        this.queue = JSON.parse(stored);
        console.log(`Loaded ${this.queue.length} queued orders from storage`);
      }
    } catch (error) {
      console.error('Failed to load offline queue from storage:', error);
      this.queue = [];
    }
  }

  private saveQueueToStorage(): void {
    try {
      localStorage.setItem('offlineQueue', JSON.stringify(this.queue));
    } catch (error) {
      console.error('Failed to save offline queue to storage:', error);
    }
  }

  public async submitOrder(order: CreateOrderRequest): Promise<void> {
    if (this.isOnline) {
      try {
        await this.sendOrderToServer(order);
        console.log('Order submitted successfully');
      } catch (error) {
        console.error('Failed to submit order, queuing for retry:', error);
        this.queueOrder(order);
      }
    } else {
      console.log('Offline, queuing order for later submission');
      this.queueOrder(order);
    }
  }

  private async sendOrderToServer(order: CreateOrderRequest): Promise<void> {
    const response = await fetch(`${this.apiBaseUrl}/orders`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(order),
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const result = await response.json();
    return result;
  }

  private queueOrder(order: CreateOrderRequest): void {
    const queuedOrder: QueuedOrder = {
      id: `order_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      order,
      timestamp: Date.now(),
      retryCount: 0,
      maxRetries: this.maxRetries,
    };

    this.queue.push(queuedOrder);
    this.saveQueueToStorage();
    console.log(`Order queued with ID: ${queuedOrder.id}`);
  }

  private async processQueue(): Promise<void> {
    if (!this.isOnline || this.queue.length === 0) {
      return;
    }

    console.log(`Processing ${this.queue.length} queued orders...`);

    const ordersToProcess = [...this.queue];
    const successfulOrders: string[] = [];
    const failedOrders: QueuedOrder[] = [];

    for (const queuedOrder of ordersToProcess) {
      try {
        await this.sendOrderToServer(queuedOrder.order);
        successfulOrders.push(queuedOrder.id);
        console.log(`Successfully processed queued order: ${queuedOrder.id}`);
      } catch (error) {
        console.error(`Failed to process queued order ${queuedOrder.id}:`, error);
        
        queuedOrder.retryCount++;
        if (queuedOrder.retryCount < queuedOrder.maxRetries) {
          failedOrders.push(queuedOrder);
        } else {
          console.error(`Max retries exceeded for order ${queuedOrder.id}, removing from queue`);
        }
      }
    }

    // Update queue
    this.queue = failedOrders;
    this.saveQueueToStorage();

    if (successfulOrders.length > 0) {
      console.log(`Successfully processed ${successfulOrders.length} orders`);
      this.notifyQueueProcessed(successfulOrders);
    }
  }

  private startRetryProcess(): void {
    setInterval(() => {
      if (this.isOnline && this.queue.length > 0) {
        this.processQueue();
      }
    }, this.retryInterval);
  }

  private notifyQueueProcessed(orderIds: string[]): void {
    // Dispatch custom event for UI to react to successful order processing
    window.dispatchEvent(new CustomEvent('offlineQueueProcessed', {
      detail: { orderIds }
    }));
  }

  public getQueueStatus(): { count: number; items: QueuedOrder[] } {
    return {
      count: this.queue.length,
      items: [...this.queue]
    };
  }

  public clearQueue(): void {
    this.queue = [];
    this.saveQueueToStorage();
    console.log('Offline queue cleared');
  }

  public isOrderQueued(orderId: string): boolean {
    return this.queue.some(item => item.id === orderId);
  }
}

// Export singleton instance
export const offlineQueueService = new OfflineQueueService();
export default offlineQueueService;
