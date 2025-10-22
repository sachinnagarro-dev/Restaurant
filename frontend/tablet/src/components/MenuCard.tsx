import React from 'react';
import { MenuItemResponse } from '../types/api';
import './MenuCard.css';

interface MenuCardProps {
  menuItem: MenuItemResponse;
  onAddToCart: (menuItem: MenuItemResponse) => void;
  onViewDetails: (menuItem: MenuItemResponse) => void;
  disabled?: boolean;
}

const MenuCard: React.FC<MenuCardProps> = ({
  menuItem,
  onAddToCart,
  onViewDetails,
  disabled = false
}) => {
  const handleAddToCart = () => {
    if (!disabled && menuItem.isAvailable) {
      onAddToCart(menuItem);
    }
  };

  const handleViewDetails = () => {
    onViewDetails(menuItem);
  };

  return (
    <div className={`menu-card ${!menuItem.isAvailable ? 'unavailable' : ''} ${disabled ? 'disabled' : ''}`}>
      <div className="menu-card-image" onClick={handleViewDetails}>
        {menuItem.imageUrl ? (
          <img 
            src={menuItem.imageUrl} 
            alt={menuItem.name}
            onError={(e) => {
              (e.target as HTMLImageElement).src = '/placeholder-food.jpg';
            }}
          />
        ) : (
          <div className="placeholder-image">
            <span>📷</span>
          </div>
        )}
        {!menuItem.isAvailable && (
          <div className="unavailable-overlay">
            <span>Unavailable</span>
          </div>
        )}
      </div>
      
      <div className="menu-card-content">
        <div className="menu-card-header">
          <h3 className="menu-card-title">{menuItem.name}</h3>
          <div className="menu-card-badges">
            {menuItem.isVegetarian && (
              <span className="vegetarian-badge">🥬 Veg</span>
            )}
            <span className="prep-time">{menuItem.preparationTimeMinutes}min</span>
          </div>
        </div>
        
        <p className="menu-card-description">{menuItem.description}</p>
        
        <div className="menu-card-footer">
          <div className="menu-card-price">
            <span className="currency">₹</span>
            <span className="amount">{menuItem.price.toFixed(2)}</span>
          </div>
          
          <div className="menu-card-actions">
            <button 
              className="view-details-btn"
              onClick={handleViewDetails}
              disabled={disabled}
            >
              View Details
            </button>
            <button 
              className="add-to-cart-btn"
              onClick={handleAddToCart}
              disabled={disabled || !menuItem.isAvailable}
            >
              Add to Cart
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default MenuCard;
