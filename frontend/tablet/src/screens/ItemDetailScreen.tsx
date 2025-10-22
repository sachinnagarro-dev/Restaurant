import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { MenuItemResponse } from '../types/api';
import { apiService } from '../services/apiService';
import { useCart } from '../contexts/CartContext';
import ImageGallery from '../components/ImageGallery';
import OrderButton from '../components/OrderButton';
import './ItemDetailScreen.css';

const ItemDetailScreen: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { addToCart } = useCart();
  const [menuItem, setMenuItem] = useState<MenuItemResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [quantity, setQuantity] = useState(1);
  const [specialInstructions, setSpecialInstructions] = useState('');

  useEffect(() => {
    if (id) {
      loadMenuItem();
    }
  }, [id]);

  const loadMenuItem = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const item = await apiService.getMenuItemById(Number(id));
      if (item) {
        setMenuItem(item);
      } else {
        setError('Menu item not found');
      }
    } catch (err) {
      console.error('Failed to load menu item:', err);
      setError('Failed to load menu item. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleAddToCart = () => {
    if (menuItem) {
      addToCart(menuItem, quantity, specialInstructions);
      // Navigate to cart or show success message
      navigate('/cart');
    }
  };

  const handleBackToMenu = () => {
    if (menuItem) {
      navigate(`/menu/${encodeURIComponent(menuItem.category)}`);
    } else {
      navigate('/');
    }
  };

  const handleViewCart = () => {
    navigate('/cart');
  };

  const increaseQuantity = () => {
    setQuantity(prev => prev + 1);
  };

  const decreaseQuantity = () => {
    setQuantity(prev => Math.max(1, prev - 1));
  };

  if (loading) {
    return (
      <div className="item-detail-screen">
        <div className="loading-container">
          <div className="loading-spinner"></div>
          <p>Loading menu item...</p>
        </div>
      </div>
    );
  }

  if (error || !menuItem) {
    return (
      <div className="item-detail-screen">
        <div className="error-container">
          <div className="error-icon">⚠️</div>
          <h3>Error Loading Item</h3>
          <p>{error || 'Menu item not found'}</p>
          <OrderButton onClick={handleBackToMenu} variant="primary">
            Back to Menu
          </OrderButton>
        </div>
      </div>
    );
  }

  const images = menuItem.imageUrl ? [menuItem.imageUrl] : [];

  return (
    <div className="item-detail-screen">
      <header className="item-header">
        <div className="item-header-content">
          <button className="back-btn" onClick={handleBackToMenu}>
            ← Back to Menu
          </button>
          
          <button className="cart-btn" onClick={handleViewCart}>
            🛒 View Cart
          </button>
        </div>
      </header>

      <main className="item-main">
        <div className="item-detail-container">
          <div className="item-images">
            <ImageGallery images={images} alt={menuItem.name} />
          </div>

          <div className="item-info">
            <div className="item-header-info">
              <h1 className="item-name">{menuItem.name}</h1>
              <div className="item-badges">
                {menuItem.isVegetarian && (
                  <span className="vegetarian-badge">🥬 Vegetarian</span>
                )}
                <span className="prep-time-badge">
                  ⏱️ {menuItem.preparationTimeMinutes} min
                </span>
              </div>
            </div>

            <div className="item-description">
              <p>{menuItem.description}</p>
            </div>

            <div className="item-price">
              <span className="currency">₹</span>
              <span className="amount">{menuItem.price.toFixed(2)}</span>
            </div>

            <div className="item-availability">
              {menuItem.isAvailable ? (
                <span className="available">✅ Available</span>
              ) : (
                <span className="unavailable">❌ Currently Unavailable</span>
              )}
            </div>

            <div className="item-actions">
              <div className="quantity-selector">
                <label htmlFor="quantity">Quantity:</label>
                <div className="quantity-controls">
                  <button 
                    className="quantity-btn"
                    onClick={decreaseQuantity}
                    disabled={quantity <= 1}
                  >
                    −
                  </button>
                  <span className="quantity-display">{quantity}</span>
                  <button 
                    className="quantity-btn"
                    onClick={increaseQuantity}
                  >
                    +
                  </button>
                </div>
              </div>

              <div className="special-instructions">
                <label htmlFor="specialInstructions">Special Instructions:</label>
                <textarea
                  id="specialInstructions"
                  value={specialInstructions}
                  onChange={(e) => setSpecialInstructions(e.target.value)}
                  placeholder="Any special requests or modifications..."
                  className="instructions-textarea"
                  rows={3}
                />
              </div>

              <div className="action-buttons">
                <OrderButton
                  onClick={handleAddToCart}
                  variant="success"
                  size="large"
                  disabled={!menuItem.isAvailable}
                  className="add-to-cart-btn"
                >
                  Add to Cart - ₹{(menuItem.price * quantity).toFixed(2)}
                </OrderButton>
              </div>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
};

export default ItemDetailScreen;
