import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
// import { MenuItemResponse } from '../types/api';
// import { apiService } from '../services/apiService';
import { CacheService } from '../services/database';
import './HomeScreen.css';

const HomeScreen: React.FC = () => {
  const navigate = useNavigate();
  const [categories, setCategories] = useState<string[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [tableNumber, setTableNumber] = useState<number>(1);

  useEffect(() => {
    loadCategories();
  }, []);

  const loadCategories = async () => {
    try {
      setLoading(true);
      setError(null);
      
      // Try to fetch from cache first, then from API
      const categoriesData = await CacheService.fetchMenuCategoriesWithCache();
      setCategories(categoriesData);
    } catch (err) {
      console.error('Failed to load categories:', err);
      setError('Failed to load menu categories. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleCategorySelect = (category: string) => {
    navigate(`/menu/${encodeURIComponent(category)}`);
  };

  const handleViewCart = () => {
    navigate('/cart');
  };

  const handleViewOrders = () => {
    navigate('/orders');
  };

  if (loading) {
    return (
      <div className="home-screen">
        <div className="loading-container">
          <div className="loading-spinner"></div>
          <p>Loading menu categories...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="home-screen">
        <div className="error-container">
          <div className="error-icon">⚠️</div>
          <h3>Error Loading Menu</h3>
          <p>{error}</p>
          <button className="retry-btn" onClick={loadCategories}>
            Try Again
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="home-screen">
      <header className="home-header">
        <div className="restaurant-info">
          <h1>TableOrder Restaurant</h1>
          <div className="table-selector">
            <label htmlFor="tableNumber">Table Number:</label>
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
        
        <div className="header-actions">
          <button className="action-btn cart-btn" onClick={handleViewCart}>
            🛒 View Cart
          </button>
          <button className="action-btn orders-btn" onClick={handleViewOrders}>
            📋 My Orders
          </button>
        </div>
      </header>

      <main className="home-main">
        <div className="welcome-section">
          <h2>Welcome to Table {tableNumber}!</h2>
          <p>Please select a category to browse our menu</p>
        </div>

        <div className="categories-grid">
          {categories.map((category) => (
            <div
              key={category}
              className="category-card"
              onClick={() => handleCategorySelect(category)}
            >
              <div className="category-icon">
                {getCategoryIcon(category)}
              </div>
              <h3 className="category-name">{category}</h3>
              <p className="category-description">
                Browse our {category.toLowerCase()} selection
              </p>
            </div>
          ))}
        </div>

        {categories.length === 0 && (
          <div className="empty-state">
            <div className="empty-icon">🍽️</div>
            <h3>No Categories Available</h3>
            <p>Please check back later or contact our staff.</p>
          </div>
        )}
      </main>

      <footer className="home-footer">
        <p>Powered by TableOrder - Digital Restaurant Experience</p>
      </footer>
    </div>
  );
};

const getCategoryIcon = (category: string): string => {
  const iconMap: { [key: string]: string } = {
    'Appetizers': '🥗',
    'Main Course': '🍖',
    'Desserts': '🍰',
    'Beverages': '🥤',
    'Salads': '🥙',
    'Soups': '🍲',
    'Pizza': '🍕',
    'Pasta': '🍝',
    'Burgers': '🍔',
    'Sandwiches': '🥪',
    'Seafood': '🐟',
    'Vegetarian': '🥬',
    'Non-Vegetarian': '🍗',
    'Breakfast': '🥞',
    'Lunch': '🍽️',
    'Dinner': '🍴'
  };
  
  return iconMap[category] || '🍽️';
};

export default HomeScreen;
