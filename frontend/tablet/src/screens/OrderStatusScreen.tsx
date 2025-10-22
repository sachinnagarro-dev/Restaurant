import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { OrderResponse, OrderStatus, PaymentQrCodeDto } from '../types/api';
import { apiService } from '../services/apiService';
import { signalRService } from '../services/signalRService';
import OrderButton from '../components/OrderButton';
import './OrderStatusScreen.css';

const OrderStatusScreen: React.FC = () => {
  const navigate = useNavigate();
  const [orders, setOrders] = useState<OrderResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [paymentQrCode, setPaymentQrCode] = useState<PaymentQrCodeDto | null>(null);
  const [showPaymentModal, setShowPaymentModal] = useState(false);

  useEffect(() => {
    loadOrders();
    setupSignalRConnection();
    
    return () => {
      signalRService.removeOrderStatusUpdateHandler(handleOrderStatusUpdate);
    };
  }, []);

  const loadOrders = async () => {
    try {
      setLoading(true);
      setError(null);
      
      // TODO: Load orders from API or context
      // For now, using mock data
      setOrders([]);
    } catch (err) {
      console.error('Failed to load orders:', err);
      setError('Failed to load orders. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const setupSignalRConnection = () => {
    signalRService.onOrderStatusUpdate(handleOrderStatusUpdate);
  };

  const handleOrderStatusUpdate = (update: any) => {
    console.log('Order status update received:', update);
    
    setOrders(prev => 
      prev.map(order => 
        order.orderId === update.orderId 
          ? { ...order, status: update.status, updatedAt: update.timestamp }
          : order
      )
    );
  };

  const handlePaymentRequest = async (orderId: number) => {
    try {
      const qrCode = await apiService.getPaymentQrCode(orderId);
      setPaymentQrCode(qrCode);
      setShowPaymentModal(true);
    } catch (err) {
      console.error('Failed to get payment QR code:', err);
      // TODO: Show error message
    }
  };

  const handleBackToMenu = () => {
    navigate('/');
  };

  const handleNewOrder = () => {
    navigate('/');
  };

  const getStatusColor = (status: OrderStatus): string => {
    switch (status) {
      case OrderStatus.Received:
        return '#ffc107';
      case OrderStatus.Preparing:
        return '#fd7e14';
      case OrderStatus.Ready:
        return '#28a745';
      case OrderStatus.Served:
        return '#20c997';
      case OrderStatus.Closed:
        return '#6c757d';
      default:
        return '#6c757d';
    }
  };

  const getStatusIcon = (status: OrderStatus): string => {
    switch (status) {
      case OrderStatus.Received:
        return '📝';
      case OrderStatus.Preparing:
        return '👨‍🍳';
      case OrderStatus.Ready:
        return '✅';
      case OrderStatus.Served:
        return '🍽️';
      case OrderStatus.Closed:
        return '💳';
      default:
        return '❓';
    }
  };

  if (loading) {
    return (
      <div className="order-status-screen">
        <div className="loading-container">
          <div className="loading-spinner"></div>
          <p>Loading orders...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="order-status-screen">
        <div className="error-container">
          <div className="error-icon">⚠️</div>
          <h3>Error Loading Orders</h3>
          <p>{error}</p>
          <OrderButton onClick={loadOrders} variant="primary">
            Try Again
          </OrderButton>
        </div>
      </div>
    );
  }

  return (
    <div className="order-status-screen">
      <header className="order-header">
        <div className="order-header-content">
          <button className="back-btn" onClick={handleBackToMenu}>
            ← Back to Menu
          </button>
          
          <h1>Order Status</h1>
          
          <button className="new-order-btn" onClick={handleNewOrder}>
            New Order
          </button>
        </div>
      </header>

      <main className="order-main">
        {orders.length === 0 ? (
          <div className="empty-orders-container">
            <div className="empty-orders-icon">📋</div>
            <h2>No orders yet</h2>
            <p>Place your first order to see it here!</p>
            <OrderButton
              onClick={handleNewOrder}
              variant="primary"
              size="large"
            >
              Place New Order
            </OrderButton>
          </div>
        ) : (
          <div className="orders-container">
            {orders.map((order) => (
              <div key={order.orderId} className="order-card">
                <div className="order-header-info">
                  <div className="order-id">
                    <span className="order-label">Order #</span>
                    <span className="order-number">{order.orderId}</span>
                  </div>
                  <div className="order-time">
                    {new Date(order.createdAt).toLocaleTimeString()}
                  </div>
                </div>

                <div className="order-status">
                  <div 
                    className="status-indicator"
                    style={{ backgroundColor: getStatusColor(order.status) }}
                  >
                    <span className="status-icon">{getStatusIcon(order.status)}</span>
                    <span className="status-text">{order.status}</span>
                  </div>
                </div>

                <div className="order-items">
                  <h4>Items ({order.items.length}):</h4>
                  <ul>
                    {order.items.map((item) => (
                      <li key={item.id}>
                        {item.quantity}x {item.menuItemName}
                        {item.specialInstructions && (
                          <span className="item-instructions">
                            (Note: {item.specialInstructions})
                          </span>
                        )}
                      </li>
                    ))}
                  </ul>
                </div>

                <div className="order-total">
                  <span className="total-label">Total:</span>
                  <span className="total-amount">₹{order.totalAmount.toFixed(2)}</span>
                </div>

                {order.specialInstructions && (
                  <div className="order-instructions">
                    <h4>Special Instructions:</h4>
                    <p>{order.specialInstructions}</p>
                  </div>
                )}

                <div className="order-actions">
                  {order.status === OrderStatus.Ready && (
                    <OrderButton
                      onClick={() => handlePaymentRequest(order.orderId)}
                      variant="success"
                      size="medium"
                    >
                      Pay Now
                    </OrderButton>
                  )}
                  
                  {order.status === OrderStatus.Closed && (
                    <div className="order-completed">
                      <span className="completed-icon">✅</span>
                      <span>Order Completed</span>
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}
      </main>

      {/* Payment Modal */}
      {showPaymentModal && paymentQrCode && (
        <div className="payment-modal-overlay" onClick={() => setShowPaymentModal(false)}>
          <div className="payment-modal" onClick={(e) => e.stopPropagation()}>
            <div className="payment-modal-header">
              <h3>Payment QR Code</h3>
              <button 
                className="close-btn"
                onClick={() => setShowPaymentModal(false)}
              >
                ×
              </button>
            </div>
            
            <div className="payment-modal-content">
              <div className="qr-code-container">
                <div className="qr-code-placeholder">
                  <span>📱</span>
                  <p>Scan QR Code to Pay</p>
                </div>
              </div>
              
              <div className="payment-details">
                <div className="payment-amount">
                  <span className="currency">₹</span>
                  <span className="amount">{paymentQrCode.amount.toFixed(2)}</span>
                </div>
                
                <div className="payment-info">
                  <p><strong>Order ID:</strong> {paymentQrCode.orderId}</p>
                  <p><strong>UPI ID:</strong> {paymentQrCode.upiId}</p>
                  <p><strong>Note:</strong> {paymentQrCode.transactionNote}</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default OrderStatusScreen;
