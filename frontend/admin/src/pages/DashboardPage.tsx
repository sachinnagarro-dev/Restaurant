import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { apiService } from '../services/apiService';
import type { DailyAnalytics, OrderSummary } from '../types';

export default function DashboardPage() {
  const { logout } = useAuth();
  const [analytics, setAnalytics] = useState<DailyAnalytics | null>(null);
  const [recentOrders, setRecentOrders] = useState<OrderSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    try {
      setLoading(true);
      const [analyticsData, ordersData] = await Promise.all([
        apiService.getDailyAnalytics(),
        apiService.getOrders()
      ]);
      
      setAnalytics(analyticsData);
      setRecentOrders(ordersData.slice(0, 10)); // Show last 10 orders
    } catch (error) {
      console.error('Failed to load dashboard data:', error);
      setError('Failed to load dashboard data');
    } finally {
      setLoading(false);
    }
  };

  const handleExportCSV = async () => {
    try {
      const salesData = await apiService.exportDailySales();
      const today = new Date().toISOString().split('T')[0];
      apiService.downloadCSV(salesData, `daily-sales-${today}.csv`);
    } catch (error) {
      console.error('Failed to export CSV:', error);
      alert('Failed to export CSV');
    }
  };

  if (loading) {
    return (
      <div style={{ padding: '2rem', textAlign: 'center' }}>
        <div>Loading dashboard...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div style={{ padding: '2rem', textAlign: 'center', color: '#dc3545' }}>
        <div>{error}</div>
        <button onClick={loadDashboardData} style={{ marginTop: '1rem', padding: '0.5rem 1rem' }}>
          Retry
        </button>
      </div>
    );
  }

  return (
    <div style={{ padding: '2rem', fontFamily: 'system-ui, -apple-system, sans-serif' }}>
      <header style={{
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        marginBottom: '2rem',
        paddingBottom: '1rem',
        borderBottom: '1px solid #eee'
      }}>
        <h1 style={{ margin: 0, color: '#333' }}>Admin Dashboard</h1>
        <nav style={{ display: 'flex', gap: '1rem', alignItems: 'center' }}>
          <Link
            to="/menu"
            style={{
              padding: '0.5rem 1rem',
              backgroundColor: '#6c757d',
              color: 'white',
              textDecoration: 'none',
              borderRadius: '4px'
            }}
          >
            Menu Management
          </Link>
          <button
            onClick={handleExportCSV}
            style={{
              padding: '0.5rem 1rem',
              backgroundColor: '#28a745',
              color: 'white',
              border: 'none',
              borderRadius: '4px',
              cursor: 'pointer'
            }}
          >
            Export CSV
          </button>
          <button
            onClick={logout}
            style={{
              padding: '0.5rem 1rem',
              backgroundColor: '#dc3545',
              color: 'white',
              border: 'none',
              borderRadius: '4px',
              cursor: 'pointer'
            }}
          >
            Logout
          </button>
        </nav>
      </header>

      {analytics && (
        <div style={{ marginBottom: '2rem' }}>
          <h2 style={{ color: '#333', marginBottom: '1rem' }}>Today's Analytics</h2>
          <div style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
            gap: '1rem',
            marginBottom: '2rem'
          }}>
            <div style={{
              padding: '1rem',
              backgroundColor: '#f8f9fa',
              borderRadius: '8px',
              textAlign: 'center'
            }}>
              <div style={{ fontSize: '2rem', fontWeight: 'bold', color: '#007bff' }}>
                {analytics.totalOrders}
              </div>
              <div style={{ color: '#666' }}>Total Orders</div>
            </div>
            <div style={{
              padding: '1rem',
              backgroundColor: '#f8f9fa',
              borderRadius: '8px',
              textAlign: 'center'
            }}>
              <div style={{ fontSize: '2rem', fontWeight: 'bold', color: '#28a745' }}>
                ${analytics.totalRevenue.toFixed(2)}
              </div>
              <div style={{ color: '#666' }}>Total Revenue</div>
            </div>
            <div style={{
              padding: '1rem',
              backgroundColor: '#f8f9fa',
              borderRadius: '8px',
              textAlign: 'center'
            }}>
              <div style={{ fontSize: '2rem', fontWeight: 'bold', color: '#ffc107' }}>
                ${analytics.averageOrderValue.toFixed(2)}
              </div>
              <div style={{ color: '#666' }}>Avg Order Value</div>
            </div>
          </div>

          <div style={{
            padding: '1rem',
            backgroundColor: '#f8f9fa',
            borderRadius: '8px'
          }}>
            <h3 style={{ marginTop: 0, color: '#333' }}>Top 10 Menu Items</h3>
            <div style={{ display: 'grid', gap: '0.5rem' }}>
              {analytics.topItems.map((item, index) => (
                <div key={item.menuItemId} style={{
                  display: 'flex',
                  justifyContent: 'space-between',
                  padding: '0.5rem',
                  backgroundColor: 'white',
                  borderRadius: '4px'
                }}>
                  <span>
                    {index + 1}. {item.menuItemName}
                  </span>
                  <span style={{ fontWeight: 'bold' }}>
                    {item.quantitySold} sold (${item.revenue.toFixed(2)})
                  </span>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}

      <div>
        <h2 style={{ color: '#333', marginBottom: '1rem' }}>Recent Orders</h2>
        <div style={{
          backgroundColor: '#f8f9fa',
          borderRadius: '8px',
          overflow: 'hidden'
        }}>
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr style={{ backgroundColor: '#e9ecef' }}>
                <th style={{ padding: '1rem', textAlign: 'left' }}>Order ID</th>
                <th style={{ padding: '1rem', textAlign: 'left' }}>Table</th>
                <th style={{ padding: '1rem', textAlign: 'left' }}>Status</th>
                <th style={{ padding: '1rem', textAlign: 'left' }}>Total</th>
                <th style={{ padding: '1rem', textAlign: 'left' }}>Items</th>
                <th style={{ padding: '1rem', textAlign: 'left' }}>Time</th>
              </tr>
            </thead>
            <tbody>
              {recentOrders.map((order) => (
                <tr key={order.id} style={{ borderTop: '1px solid #dee2e6' }}>
                  <td style={{ padding: '1rem' }}>#{order.id}</td>
                  <td style={{ padding: '1rem' }}>Table {order.tableNumber}</td>
                  <td style={{ padding: '1rem' }}>
                    <span style={{
                      padding: '0.25rem 0.5rem',
                      borderRadius: '4px',
                      fontSize: '0.875rem',
                      backgroundColor: order.status === 'Received' ? '#fff3cd' : 
                                      order.status === 'Preparing' ? '#d1ecf1' :
                                      order.status === 'Ready' ? '#d4edda' :
                                      order.status === 'Served' ? '#cce5ff' : '#f8d7da',
                      color: order.status === 'Received' ? '#856404' :
                             order.status === 'Preparing' ? '#0c5460' :
                             order.status === 'Ready' ? '#155724' :
                             order.status === 'Served' ? '#004085' : '#721c24'
                    }}>
                      {order.status}
                    </span>
                  </td>
                  <td style={{ padding: '1rem', fontWeight: 'bold' }}>
                    ${order.totalAmount.toFixed(2)}
                  </td>
                  <td style={{ padding: '1rem' }}>{order.itemCount}</td>
                  <td style={{ padding: '1rem', color: '#666' }}>
                    {new Date(order.createdAt).toLocaleTimeString()}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
