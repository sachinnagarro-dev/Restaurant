import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { MenuItemResponse } from '../types/api';
// import { apiService } from '../services/apiService';
import { CacheService } from '../services/database';
import { useCart } from '../contexts/CartContext';
import MenuCard from '../components/MenuCard';
import OrderButton from '../components/OrderButton';
import './MenuListScreen.css';

const MenuListScreen: React.FC = () => {
  const { category } = useParams<{ category: string }>();
  const navigate = useNavigate();
  const { addToCart } = useCart();
  const [menuItems, setMenuItems] = useState<MenuItemResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterVegetarian, setFilterVegetarian] = useState(false);
  const [sortBy, setSortBy] = useState<'name' | 'price' | 'prepTime'>('name');

  useEffect(() => {
    if (category) {
      loadMenuItems();
    }
  }, [category]);

  const loadMenuItems = async () => {
    try {
      setLoading(true);
      setError(null);
      
      // Try to fetch from cache first, then from API
      const items = await CacheService.fetchMenuItemsWithCache();
      
      // Filter by category if specified
      const filteredItems = category 
        ? items.filter(item => item.category === decodeURIComponent(category))
        : items;
      
      setMenuItems(filteredItems);
    } catch (err) {
      console.error('Failed to load menu items:', err);
      setError('Failed to load menu items. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleAddToCart = (menuItem: MenuItemResponse) => {
    addToCart(menuItem, 1);
  };

  const handleViewDetails = (menuItem: MenuItemResponse) => {
    navigate(`/menu/item/${menuItem.id}`);
  };

  const handleBackToHome = () => {
    navigate('/');
  };

  const handleViewCart = () => {
    navigate('/cart');
  };

  const filteredAndSortedItems = menuItems
    .filter(item => {
      const matchesSearch = item.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
                           item.description.toLowerCase().includes(searchTerm.toLowerCase());
      const matchesVegetarian = !filterVegetarian || item.isVegetarian;
      return matchesSearch && matchesVegetarian && item.isAvailable;
    })
    .sort((a, b) => {
      switch (sortBy) {
        case 'name':
          return a.name.localeCompare(b.name);
        case 'price':
          return a.price - b.price;
        case 'prepTime':
          return a.preparationTimeMinutes - b.preparationTimeMinutes;
        default:
          return 0;
      }
    });

  if (loading) {
    return (
      <div className="menu-list-screen">
        <div className="loading-container">
          <div className="loading-spinner"></div>
          <p>Loading menu items...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="menu-list-screen">
        <div className="error-container">
          <div className="error-icon">⚠️</div>
          <h3>Error Loading Menu</h3>
          <p>{error}</p>
          <OrderButton onClick={loadMenuItems} variant="primary">
            Try Again
          </OrderButton>
        </div>
      </div>
    );
  }

  return (
    <div className="menu-list-screen">
      <header className="menu-header">
        <div className="menu-header-content">
          <button className="back-btn" onClick={handleBackToHome}>
            ← Back to Categories
          </button>
          
          <div className="menu-title">
            <h1>{category ? decodeURIComponent(category) : 'All Items'}</h1>
            <p>{filteredAndSortedItems.length} items available</p>
          </div>
          
          <button className="cart-btn" onClick={handleViewCart}>
            🛒 View Cart
          </button>
        </div>
      </header>

      <div className="menu-filters">
        <div className="search-container">
          <input
            type="text"
            placeholder="Search menu items..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="search-input"
          />
        </div>

        <div className="filter-controls">
          <label className="filter-checkbox">
            <input
              type="checkbox"
              checked={filterVegetarian}
              onChange={(e) => setFilterVegetarian(e.target.checked)}
            />
            <span>Vegetarian Only</span>
          </label>

          <select
            value={sortBy}
            onChange={(e) => setSortBy(e.target.value as 'name' | 'price' | 'prepTime')}
            className="sort-select"
          >
            <option value="name">Sort by Name</option>
            <option value="price">Sort by Price</option>
            <option value="prepTime">Sort by Prep Time</option>
          </select>
        </div>
      </div>

      <main className="menu-main">
        {filteredAndSortedItems.length === 0 ? (
          <div className="empty-state">
            <div className="empty-icon">🍽️</div>
            <h3>No items found</h3>
            <p>Try adjusting your search or filters</p>
          </div>
        ) : (
          <div className="menu-grid">
            {filteredAndSortedItems.map((item) => (
              <MenuCard
                key={item.id}
                menuItem={item}
                onAddToCart={handleAddToCart}
                onViewDetails={handleViewDetails}
              />
            ))}
          </div>
        )}
      </main>
    </div>
  );
};

export default MenuListScreen;
