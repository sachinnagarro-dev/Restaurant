import * as signalR from '@microsoft/signalr';
import { OrderStatus, OrderStatusUpdate, PaymentStatusUpdate } from '../types/api';

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private isConnected: boolean = false;
  // private reconnectAttempts: number = 0;
  private maxReconnectAttempts: number = 5;
  private reconnectDelay: number = 5000; // 5 seconds

  // Event handlers
  private orderStatusHandlers: ((update: OrderStatusUpdate) => void)[] = [];
  private paymentStatusHandlers: ((update: PaymentStatusUpdate) => void)[] = [];
  private connectionHandlers: ((connected: boolean) => void)[] = [];

  constructor() {
    this.initializeConnection();
  }

  private initializeConnection(): void {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('/orderHub')
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: retryContext => {
          if (retryContext.previousRetryCount < this.maxReconnectAttempts) {
            return this.reconnectDelay;
          } else {
            return null; // Stop reconnecting
          }
        }
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.setupEventHandlers();
  }

  private setupEventHandlers(): void {
    if (!this.connection) return;

    // Order status updates
    this.connection.on('OrderStatusUpdated', (data: any) => {
      console.log('Order status update received:', data);
      const update: OrderStatusUpdate = {
        orderId: data.orderId,
        status: data.status as OrderStatus,
        timestamp: new Date().toISOString()
      };
      this.notifyOrderStatusHandlers(update);
    });

    // Payment status updates
    this.connection.on('PaymentCompleted', (data: any) => {
      console.log('Payment completed received:', data);
      const update: PaymentStatusUpdate = {
        orderId: data.orderId,
        paymentId: data.paymentId,
        status: 'completed',
        timestamp: new Date().toISOString()
      };
      this.notifyPaymentStatusHandlers(update);
    });

    this.connection.on('PaymentFailed', (data: any) => {
      console.log('Payment failed received:', data);
      const update: PaymentStatusUpdate = {
        orderId: data.orderId,
        paymentId: data.paymentId,
        status: 'failed',
        timestamp: new Date().toISOString()
      };
      this.notifyPaymentStatusHandlers(update);
    });

    // Connection events
    this.connection.onclose((error) => {
      console.log('SignalR connection closed:', error);
      this.isConnected = false;
      this.notifyConnectionHandlers(false);
    });

    this.connection.onreconnecting((error) => {
      console.log('SignalR reconnecting:', error);
      this.isConnected = false;
      this.notifyConnectionHandlers(false);
    });

    this.connection.onreconnected((connectionId) => {
      console.log('SignalR reconnected:', connectionId);
      this.isConnected = true;
      // this.reconnectAttempts = 0;
      this.notifyConnectionHandlers(true);
    });
  }

  public async start(): Promise<void> {
    if (!this.connection) {
      this.initializeConnection();
    }

    try {
      await this.connection!.start();
      this.isConnected = true;
      // this.reconnectAttempts = 0;
      console.log('SignalR connection started');
      this.notifyConnectionHandlers(true);
    } catch (error) {
      console.error('Failed to start SignalR connection:', error);
      this.isConnected = false;
      this.notifyConnectionHandlers(false);
      throw error;
    }
  }

  public async stop(): Promise<void> {
    if (this.connection) {
      try {
        await this.connection.stop();
        this.isConnected = false;
        console.log('SignalR connection stopped');
        this.notifyConnectionHandlers(false);
      } catch (error) {
        console.error('Failed to stop SignalR connection:', error);
      }
    }
  }

  public async joinTableGroup(tableNumber: number): Promise<void> {
    if (this.connection && this.isConnected) {
      try {
        await this.connection.invoke('JoinTableGroup', tableNumber);
        console.log(`Joined table group: Table_${tableNumber}`);
      } catch (error) {
        console.error('Failed to join table group:', error);
      }
    }
  }

  public async leaveTableGroup(tableNumber: number): Promise<void> {
    if (this.connection && this.isConnected) {
      try {
        await this.connection.invoke('LeaveTableGroup', tableNumber);
        console.log(`Left table group: Table_${tableNumber}`);
      } catch (error) {
        console.error('Failed to leave table group:', error);
      }
    }
  }

  public async joinKitchenGroup(): Promise<void> {
    if (this.connection && this.isConnected) {
      try {
        await this.connection.invoke('JoinKitchenGroup');
        console.log('Joined kitchen group');
      } catch (error) {
        console.error('Failed to join kitchen group:', error);
      }
    }
  }

  public async leaveKitchenGroup(): Promise<void> {
    if (this.connection && this.isConnected) {
      try {
        await this.connection.invoke('LeaveKitchenGroup');
        console.log('Left kitchen group');
      } catch (error) {
        console.error('Failed to leave kitchen group:', error);
      }
    }
  }

  // Event handler registration
  public onOrderStatusUpdate(handler: (update: OrderStatusUpdate) => void): void {
    this.orderStatusHandlers.push(handler);
  }

  public onPaymentStatusUpdate(handler: (update: PaymentStatusUpdate) => void): void {
    this.paymentStatusHandlers.push(handler);
  }

  public onConnectionChange(handler: (connected: boolean) => void): void {
    this.connectionHandlers.push(handler);
  }

  // Event handler removal
  public removeOrderStatusUpdateHandler(handler: (update: OrderStatusUpdate) => void): void {
    const index = this.orderStatusHandlers.indexOf(handler);
    if (index > -1) {
      this.orderStatusHandlers.splice(index, 1);
    }
  }

  public removePaymentStatusUpdateHandler(handler: (update: PaymentStatusUpdate) => void): void {
    const index = this.paymentStatusHandlers.indexOf(handler);
    if (index > -1) {
      this.paymentStatusHandlers.splice(index, 1);
    }
  }

  public removeConnectionChangeHandler(handler: (connected: boolean) => void): void {
    const index = this.connectionHandlers.indexOf(handler);
    if (index > -1) {
      this.connectionHandlers.splice(index, 1);
    }
  }

  // Notification methods
  private notifyOrderStatusHandlers(update: OrderStatusUpdate): void {
    this.orderStatusHandlers.forEach(handler => {
      try {
        handler(update);
      } catch (error) {
        console.error('Error in order status handler:', error);
      }
    });
  }

  private notifyPaymentStatusHandlers(update: PaymentStatusUpdate): void {
    this.paymentStatusHandlers.forEach(handler => {
      try {
        handler(update);
      } catch (error) {
        console.error('Error in payment status handler:', error);
      }
    });
  }

  private notifyConnectionHandlers(connected: boolean): void {
    this.connectionHandlers.forEach(handler => {
      try {
        handler(connected);
      } catch (error) {
        console.error('Error in connection handler:', error);
      }
    });
  }

  // Utility methods
  public isConnectionActive(): boolean {
    return this.isConnected;
  }

  public getConnectionState(): signalR.HubConnectionState | null {
    return this.connection?.state || null;
  }

  public getConnectionId(): string | null {
    return this.connection?.connectionId || null;
  }
}

// Export singleton instance
export const signalRService = new SignalRService();
export default signalRService;
