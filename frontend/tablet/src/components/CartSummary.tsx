import React from 'react';
import { CartItem } from '../types/api';
import './CartSummary.css';

interface CartSummaryProps {
  items: CartItem[];
  onUpdateQuantity: (menuItemId: number, quantity: number) => void;
  onRemoveItem: (menuItemId: number) => void;
  onClearCart: () => void;
  onProceedToCheckout: () => void;
  tableNumber?: number;
  disabled?: boolean;
}

const CartSummary: React.FC<CartSummaryProps> = ({
  items,
  onUpdateQuantity,
  onRemoveItem,
  onClearCart,
  onProceedToCheckout,
  tableNumber,
  disabled = false
}) => {
  const subtotal = items.reduce((sum, item) => sum + (item.menuItem.price * item.quantity), 0);
  const taxRate = 0.18; // 18% tax
  const taxAmount = subtotal * taxRate;
  const totalAmount = subtotal + taxAmount;

  const handleQuantityChange = (menuItemId: number, newQuantity: number) => {
    if (newQuantity <= 0) {
      onRemoveItem(menuItemId);
    } else {
      onUpdateQuantity(menuItemId, newQuantity);
    }
  };

  if (items.length === 0) {
    return (
      <div className="cart-summary empty">
        <div className="empty-cart">
          <div className="empty-cart-icon">🛒</div>
          <h3>Your cart is empty</h3>
          <p>Add some delicious items to get started!</p>
        </div>
      </div>
    );
  }

  return (
    <div className="cart-summary">
      <div className="cart-header">
        <h2>Your Order</h2>
        {tableNumber && (
          <div className="table-info">
            <span className="table-label">Table</span>
            <span className="table-number">{tableNumber}</span>
          </div>
        )}
      </div>

      <div className="cart-items">
        {items.map((item) => (
          <div key={item.menuItem.id} className="cart-item">
            <div className="cart-item-info">
              <div className="cart-item-image">
                {item.menuItem.imageUrl ? (
                  <img 
                    src={item.menuItem.imageUrl} 
                    alt={item.menuItem.name}
                    onError={(e) => {
                      (e.target as HTMLImageElement).src = '/placeholder-food.jpg';
                    }}
                  />
                ) : (
                  <div className="placeholder-image">📷</div>
                )}
              </div>
              
              <div className="cart-item-details">
                <h4 className="cart-item-name">{item.menuItem.name}</h4>
                <p className="cart-item-price">₹{item.menuItem.price.toFixed(2)} each</p>
                {item.specialInstructions && (
                  <p className="cart-item-instructions">
                    <em>Note: {item.specialInstructions}</em>
                  </p>
                )}
              </div>
            </div>

            <div className="cart-item-controls">
              <div className="quantity-controls">
                <button 
                  className="quantity-btn"
                  onClick={() => handleQuantityChange(item.menuItem.id, item.quantity - 1)}
                  disabled={disabled}
                >
                  −
                </button>
                <span className="quantity-display">{item.quantity}</span>
                <button 
                  className="quantity-btn"
                  onClick={() => handleQuantityChange(item.menuItem.id, item.quantity + 1)}
                  disabled={disabled}
                >
                  +
                </button>
              </div>
              
              <div className="cart-item-total">
                ₹{(item.menuItem.price * item.quantity).toFixed(2)}
              </div>
              
              <button 
                className="remove-item-btn"
                onClick={() => onRemoveItem(item.menuItem.id)}
                disabled={disabled}
                title="Remove item"
              >
                🗑️
              </button>
            </div>
          </div>
        ))}
      </div>

      <div className="cart-summary-section">
        <div className="summary-row">
          <span>Subtotal:</span>
          <span>₹{subtotal.toFixed(2)}</span>
        </div>
        <div className="summary-row">
          <span>Tax (18%):</span>
          <span>₹{taxAmount.toFixed(2)}</span>
        </div>
        <div className="summary-row total">
          <span>Total:</span>
          <span>₹{totalAmount.toFixed(2)}</span>
        </div>
      </div>

      <div className="cart-actions">
        <button 
          className="clear-cart-btn"
          onClick={onClearCart}
          disabled={disabled}
        >
          Clear Cart
        </button>
        <button 
          className="checkout-btn"
          onClick={onProceedToCheckout}
          disabled={disabled || items.length === 0}
        >
          Place Order
        </button>
      </div>
    </div>
  );
};

export default CartSummary;
