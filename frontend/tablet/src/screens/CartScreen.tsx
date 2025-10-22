import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCart } from '../contexts/CartContext';
import CartSummary from '../components/CartSummary';
import OrderButton from '../components/OrderButton';
import offlineQueue from '../services/offlineQueue';
import { CreateOrderRequest } from '../types/api';
import './CartScreen.css';

const CartScreen: React.FC = () => {
  const navigate = useNavigate();
  const { cartItems, updateQuantity, removeFromCart, clearCart } = useCart();
  const [loading, setLoading] = useState(false);
  const [tableNumber, setTableNumber] = useState<number>(1);

  const handleUpdateQuantity = (menuItemId: number, quantity: number) => {
    updateQuantity(menuItemId, quantity);
  };

  const handleRemoveItem = (menuItemId: number) => {
    removeFromCart(menuItemId);
  };

  const handleClearCart = () => {
    clearCart();
  };

  const handleProceedToCheckout = async () => {
    if (cartItems.length === 0) return;

    setLoading(true);
    try {
      const order: CreateOrderRequest = {
        tableId: tableNumber,
        items: cartItems.map(item => ({
          menuItemId: item.menuItem.id,
          quantity: item.quantity,
          specialInstructions: item.specialInstructions
        }))
      };
      await offlineQueue.submitOrder(order);

      // Navigate to order status screen
      navigate('/orders');
    } catch (error) {
      console.error('Failed to create order:', error);
      // TODO: Show error message
    } finally {
      setLoading(false);
    }
  };

  const handleBackToMenu = () => {
    navigate('/');
  };

  const handleContinueShopping = () => {
    navigate('/');
  };

  if (cartItems.length === 0) {
    return (
      <div className="cart-screen">
        <header className="cart-header">
          <div className="cart-header-content">
            <button className="back-btn" onClick={handleBackToMenu}>
              ← Back to Menu
            </button>
            
            <h1>Shopping Cart</h1>
            
            <div className="table-selector">
              <label htmlFor="tableNumber">Table:</label>
              <select
                id="tableNumber"
                value={tableNumber}
                onChange={(e) => setTableNumber(Number(e.target.value))}
                className="table-select"
              >
                {Array.from({ length: 20 }, (_, i) => i + 1).map(num => (
                  <option key={num} value={num}>Table {num}</option>
                ))}
              </select>
            </div>
          </div>
        </header>

        <main className="cart-main">
          <div className="empty-cart-container">
            <div className="empty-cart-icon">🛒</div>
            <h2>Your cart is empty</h2>
            <p>Add some delicious items to get started!</p>
            <OrderButton
              onClick={handleContinueShopping}
              variant="primary"
              size="large"
            >
              Continue Shopping
            </OrderButton>
          </div>
        </main>
      </div>
    );
  }

  return (
    <div className="cart-screen">
      <header className="cart-header">
        <div className="cart-header-content">
          <button className="back-btn" onClick={handleBackToMenu}>
            ← Back to Menu
          </button>
          
          <h1>Shopping Cart</h1>
          
          <div className="table-selector">
            <label htmlFor="tableNumber">Table:</label>
            <select
              id="tableNumber"
              value={tableNumber}
              onChange={(e) => setTableNumber(Number(e.target.value))}
              className="table-select"
            >
              {Array.from({ length: 20 }, (_, i) => i + 1).map(num => (
                <option key={num} value={num}>Table {num}</option>
              ))}
            </select>
          </div>
        </div>
      </header>

      <main className="cart-main">
        <div className="cart-container">
          <div className="cart-content">
            <CartSummary
              items={cartItems}
              onUpdateQuantity={handleUpdateQuantity}
              onRemoveItem={handleRemoveItem}
              onClearCart={handleClearCart}
              onProceedToCheckout={handleProceedToCheckout}
              tableNumber={tableNumber}
              disabled={loading}
            />
          </div>
          
          <div className="cart-actions">
            <OrderButton
              onClick={handleContinueShopping}
              variant="secondary"
              size="medium"
            >
              Continue Shopping
            </OrderButton>
            
            <OrderButton
              onClick={handleProceedToCheckout}
              variant="success"
              size="large"
              loading={loading}
              disabled={cartItems.length === 0}
            >
              Place Order
            </OrderButton>
          </div>
        </div>
      </main>
    </div>
  );
};

export default CartScreen;
