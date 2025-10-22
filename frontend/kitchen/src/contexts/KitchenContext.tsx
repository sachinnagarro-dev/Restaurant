import React, { createContext, useContext, useReducer, useEffect } from 'react';
import type { ReactNode } from 'react';
import type { Order, OrderStatus, KitchenState, KitchenFilters, OrderStatusUpdate } from '../types/order';
import { signalRService } from '../services/signalRService';
import { apiService } from '../services/apiService';

type KitchenAction =
  | { type: 'SET_LOADING'; payload: boolean }
  | { type: 'SET_ERROR'; payload: string | null }
  | { type: 'SET_ORDERS'; payload: Order[] }
  | { type: 'ADD_ORDER'; payload: Order }
  | { type: 'UPDATE_ORDER'; payload: Order }
  | { type: 'REMOVE_ORDER'; payload: number }
  | { type: 'SET_FILTERS'; payload: Partial<KitchenFilters> }
  | { type: 'SET_CONNECTION_STATUS'; payload: 'connected' | 'disconnected' | 'connecting' };

const initialState: KitchenState = {
  orders: [],
  filters: {
    status: 'All',
    searchTerm: ''
  },
  isLoading: false,
  error: null,
  connectionStatus: 'disconnected'
};

function kitchenReducer(state: KitchenState, action: KitchenAction): KitchenState {
  switch (action.type) {
    case 'SET_LOADING':
      return { ...state, isLoading: action.payload };
    case 'SET_ERROR':
      return { ...state, error: action.payload };
    case 'SET_ORDERS':
      return { ...state, orders: action.payload };
    case 'ADD_ORDER':
      return { ...state, orders: [...state.orders, action.payload] };
    case 'UPDATE_ORDER':
      return {
        ...state,
        orders: state.orders.map(order =>
          order.id === action.payload.id ? action.payload : order
        )
      };
    case 'REMOVE_ORDER':
      return {
        ...state,
        orders: state.orders.filter(order => order.id !== action.payload)
      };
    case 'SET_FILTERS':
      return {
        ...state,
        filters: { ...state.filters, ...action.payload }
      };
    case 'SET_CONNECTION_STATUS':
      return { ...state, connectionStatus: action.payload };
    default:
      return state;
  }
}

interface KitchenContextType {
  state: KitchenState;
  loadOrders: () => Promise<void>;
  updateOrderStatus: (orderId: number, status: OrderStatus) => Promise<void>;
  setFilters: (filters: Partial<KitchenFilters>) => void;
  getFilteredOrders: () => Order[];
  printKitchenTicket: (order: Order) => void;
}

const KitchenContext = createContext<KitchenContextType | undefined>(undefined);

export const useKitchen = () => {
  const context = useContext(KitchenContext);
  if (context === undefined) {
    throw new Error('useKitchen must be used within a KitchenProvider');
  }
  return context;
};

interface KitchenProviderProps {
  children: ReactNode;
}

export const KitchenProvider: React.FC<KitchenProviderProps> = ({ children }) => {
  const [state, dispatch] = useReducer(kitchenReducer, initialState);

  // Load orders from API
  const loadOrders = async () => {
    try {
      dispatch({ type: 'SET_LOADING', payload: true });
      dispatch({ type: 'SET_ERROR', payload: null });
      
      const orders = await apiService.getOrders();
      dispatch({ type: 'SET_ORDERS', payload: orders });
    } catch (error) {
      console.error('Failed to load orders:', error);
      dispatch({ type: 'SET_ERROR', payload: 'Failed to load orders' });
    } finally {
      dispatch({ type: 'SET_LOADING', payload: false });
    }
  };

  // Update order status
  const updateOrderStatus = async (orderId: number, status: OrderStatus) => {
    try {
      const updatedOrder = await apiService.updateOrderStatus(orderId, status);
      dispatch({ type: 'UPDATE_ORDER', payload: updatedOrder });
    } catch (error) {
      console.error('Failed to update order status:', error);
      dispatch({ type: 'SET_ERROR', payload: 'Failed to update order status' });
    }
  };

  // Set filters
  const setFilters = (filters: Partial<KitchenFilters>) => {
    dispatch({ type: 'SET_FILTERS', payload: filters });
  };

  // Get filtered orders
  const getFilteredOrders = (): Order[] => {
    let filtered = state.orders;

    // Filter by status
    if (state.filters.status !== 'All') {
      filtered = filtered.filter(order => order.status === state.filters.status);
    }

    // Filter by search term
    if (state.filters.searchTerm) {
      const searchTerm = state.filters.searchTerm.toLowerCase();
      filtered = filtered.filter(order =>
        order.id.toString().includes(searchTerm) ||
        order.tableNumber.toString().includes(searchTerm) ||
        order.items.some(item => 
          item.menuItemName.toLowerCase().includes(searchTerm)
        )
      );
    }

    // Sort by creation time (newest first)
    return filtered.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
  };

  // Print kitchen ticket
  const printKitchenTicket = (order: Order) => {
    const printWindow = window.open('', '_blank');
    if (!printWindow) return;

    const printContent = `
      <!DOCTYPE html>
      <html>
        <head>
          <title>Kitchen Ticket - Order #${order.id}</title>
          <style>
            body { font-family: monospace; margin: 0; padding: 20px; }
            .ticket { border: 2px solid #000; padding: 20px; max-width: 400px; }
            .header { text-align: center; border-bottom: 2px solid #000; padding-bottom: 10px; margin-bottom: 20px; }
            .order-info { margin-bottom: 15px; }
            .items { margin-bottom: 15px; }
            .item { display: flex; justify-content: space-between; margin-bottom: 5px; padding-bottom: 5px; border-bottom: 1px solid #ccc; }
            .special { font-style: italic; color: #666; margin-top: 5px; }
            .total { border-top: 2px solid #000; padding-top: 10px; font-weight: bold; }
            .status { text-align: center; margin-top: 15px; font-weight: bold; }
          </style>
        </head>
        <body>
          <div class="ticket">
            <div class="header">
              <h1>KITCHEN TICKET</h1>
              <h2>Order #${order.id}</h2>
            </div>
            
            <div class="order-info">
              <p><strong>Table:</strong> ${order.tableNumber}</p>
              <p><strong>Time:</strong> ${new Date(order.createdAt).toLocaleString()}</p>
              ${order.specialInstructions ? `<p><strong>Special Instructions:</strong> ${order.specialInstructions}</p>` : ''}
            </div>
            
            <div class="items">
              <h3>Items:</h3>
              ${order.items.map(item => `
                <div class="item">
                  <span>${item.quantity}x ${item.menuItemName}</span>
                  <span>$${(item.quantity * item.unitPrice).toFixed(2)}</span>
                </div>
                ${item.specialInstructions ? `<div class="special">Note: ${item.specialInstructions}</div>` : ''}
              `).join('')}
            </div>
            
            <div class="total">
              <p>Subtotal: $${order.subTotal.toFixed(2)}</p>
              <p>Tax: $${order.taxAmount.toFixed(2)}</p>
              <p>Total: $${order.totalAmount.toFixed(2)}</p>
            </div>
            
            <div class="status">
              Status: ${order.status}
            </div>
          </div>
        </body>
      </html>
    `;

    printWindow.document.write(printContent);
    printWindow.document.close();
    printWindow.focus();
    printWindow.print();
    printWindow.close();
  };

  // Setup SignalR connection
  useEffect(() => {
    const setupSignalR = async () => {
      try {
        dispatch({ type: 'SET_CONNECTION_STATUS', payload: 'connecting' });
        await signalRService.start();
        await signalRService.joinKitchenGroup();
        dispatch({ type: 'SET_CONNECTION_STATUS', payload: 'connected' });
      } catch (error) {
        console.error('Failed to setup SignalR:', error);
        dispatch({ type: 'SET_CONNECTION_STATUS', payload: 'disconnected' });
      }
    };

    setupSignalR();

    // Handle order status updates
    const handleOrderStatusUpdate = (update: OrderStatusUpdate) => {
      dispatch({
        type: 'UPDATE_ORDER',
        payload: {
          id: update.orderId,
          status: update.status,
          // We'll need to fetch the full order or handle this differently
          // For now, we'll just update the status
        } as any
      });
    };

    signalRService.onOrderStatusUpdate(handleOrderStatusUpdate);

    // Handle connection changes
    const handleConnectionChange = (connected: boolean) => {
      dispatch({
        type: 'SET_CONNECTION_STATUS',
        payload: connected ? 'connected' : 'disconnected'
      });
    };

    signalRService.onConnectionChange(handleConnectionChange);

    return () => {
      signalRService.removeOrderStatusUpdateHandler(handleOrderStatusUpdate);
      signalRService.removeConnectionChangeHandler(handleConnectionChange);
    };
  }, []);

  const value: KitchenContextType = {
    state,
    loadOrders,
    updateOrderStatus,
    setFilters,
    getFilteredOrders,
    printKitchenTicket
  };

  return (
    <KitchenContext.Provider value={value}>
      {children}
    </KitchenContext.Provider>
  );
};
