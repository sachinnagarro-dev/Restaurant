import React from 'react';
import { OrderStatus } from '../types/order';
import { useKitchen } from '../contexts/KitchenContext';

const KitchenFilters: React.FC = () => {
  const { state, setFilters } = useKitchen();

  const handleStatusFilter = (status: OrderStatus | 'All') => {
    setFilters({ status });
  };

  const handleSearchChange = (searchTerm: string) => {
    setFilters({ searchTerm });
  };

  const getStatusCount = (status: OrderStatus | 'All'): number => {
    if (status === 'All') {
      return state.orders.length;
    }
    return state.orders.filter(order => order.status === status).length;
  };

  const statusOptions: (OrderStatus | 'All')[] = [
    'All',
    OrderStatus.Received,
    OrderStatus.Preparing,
    OrderStatus.Ready,
    OrderStatus.Served,
    OrderStatus.Closed
  ];

  return (
    <div className="bg-white p-4 rounded-lg shadow-sm border border-gray-200 mb-6">
      <div className="flex flex-col lg:flex-row gap-4">
        {/* Search */}
        <div className="flex-1">
          <label htmlFor="search" className="block text-sm font-medium text-gray-700 mb-1">
            Search Orders
          </label>
          <input
            id="search"
            type="text"
            placeholder="Search by order ID, table number, or item name..."
            value={state.filters.searchTerm}
            onChange={(e) => handleSearchChange(e.target.value)}
            className="input w-full"
          />
        </div>

        {/* Status Filter */}
        <div className="lg:w-64">
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Filter by Status
          </label>
          <div className="flex flex-wrap gap-2">
            {statusOptions.map((status) => (
              <button
                key={status}
                onClick={() => handleStatusFilter(status)}
                className={`px-3 py-1 rounded-full text-sm font-medium transition-colors ${
                  state.filters.status === status
                    ? 'bg-kitchen-600 text-white'
                    : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                }`}
              >
                {status} ({getStatusCount(status)})
              </button>
            ))}
          </div>
        </div>
      </div>

      {/* Quick Stats */}
      <div className="mt-4 pt-4 border-t border-gray-200">
        <div className="flex flex-wrap gap-4 text-sm text-gray-600">
          <span>
            <strong>Total Orders:</strong> {state.orders.length}
          </span>
          <span>
            <strong>Active Orders:</strong> {state.orders.filter(o => o.status !== OrderStatus.Closed).length}
          </span>
          <span>
            <strong>Pending:</strong> {state.orders.filter(o => o.status === OrderStatus.Received).length}
          </span>
          <span>
            <strong>In Progress:</strong> {state.orders.filter(o => o.status === OrderStatus.Preparing).length}
          </span>
          <span>
            <strong>Ready:</strong> {state.orders.filter(o => o.status === OrderStatus.Ready).length}
          </span>
        </div>
      </div>
    </div>
  );
};

export default KitchenFilters;

