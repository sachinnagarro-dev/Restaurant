import React from 'react';
import type { Order } from '../types/order';
import { OrderStatus } from '../types/order';
import { useKitchen } from '../contexts/KitchenContext';

interface OrderCardProps {
  order: Order;
}

const OrderCard: React.FC<OrderCardProps> = ({ order }) => {
  const { updateOrderStatus, printKitchenTicket } = useKitchen();

  const getStatusColor = (status: OrderStatus): string => {
    switch (status) {
      case OrderStatus.Received:
        return 'status-received';
      case OrderStatus.Preparing:
        return 'status-preparing';
      case OrderStatus.Ready:
        return 'status-ready';
      case OrderStatus.Served:
        return 'status-served';
      case OrderStatus.Closed:
        return 'status-closed';
      default:
        return 'status-received';
    }
  };

  const getNextStatus = (currentStatus: OrderStatus): OrderStatus | null => {
    switch (currentStatus) {
      case OrderStatus.Received:
        return OrderStatus.Preparing;
      case OrderStatus.Preparing:
        return OrderStatus.Ready;
      case OrderStatus.Ready:
        return OrderStatus.Served;
      case OrderStatus.Served:
        return OrderStatus.Closed;
      default:
        return null;
    }
  };

  const getNextStatusButtonText = (currentStatus: OrderStatus): string => {
    switch (currentStatus) {
      case OrderStatus.Received:
        return 'Start Preparing';
      case OrderStatus.Preparing:
        return 'Mark Ready';
      case OrderStatus.Ready:
        return 'Mark Served';
      case OrderStatus.Served:
        return 'Close Order';
      default:
        return 'Update Status';
    }
  };

  const getNextStatusButtonClass = (currentStatus: OrderStatus): string => {
    switch (currentStatus) {
      case OrderStatus.Received:
        return 'btn-warning';
      case OrderStatus.Preparing:
        return 'btn-primary';
      case OrderStatus.Ready:
        return 'btn-success';
      case OrderStatus.Served:
        return 'btn-secondary';
      default:
        return 'btn-primary';
    }
  };

  const handleStatusUpdate = async () => {
    const nextStatus = getNextStatus(order.status);
    if (nextStatus) {
      await updateOrderStatus(order.id, nextStatus);
    }
  };

  const handlePrintTicket = () => {
    printKitchenTicket(order);
  };

  const formatTime = (dateString: string): string => {
    return new Date(dateString).toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit',
      hour12: true
    });
  };

  const nextStatus = getNextStatus(order.status);

  return (
    <div className="card hover:shadow-md transition-shadow">
      <div className="flex justify-between items-start mb-4">
        <div>
          <h3 className="text-lg font-semibold text-gray-900">
            Order #{order.id}
          </h3>
          <p className="text-sm text-gray-600">
            Table {order.tableNumber} • {formatTime(order.createdAt)}
          </p>
        </div>
        <div className="flex items-center gap-2">
          <span className={`status-badge ${getStatusColor(order.status)}`}>
            {order.status}
          </span>
          <button
            onClick={handlePrintTicket}
            className="btn-secondary text-xs py-1 px-2"
            title="Print Kitchen Ticket"
          >
            🖨️ Print
          </button>
        </div>
      </div>

      <div className="mb-4">
        <h4 className="font-medium text-gray-900 mb-2">Items:</h4>
        <div className="space-y-1">
          {order.items.map((item) => (
            <div key={item.id} className="flex justify-between items-start">
              <div className="flex-1">
                <span className="font-medium">
                  {item.quantity}x {item.menuItemName}
                </span>
                {item.specialInstructions && (
                  <p className="text-sm text-gray-600 italic">
                    Note: {item.specialInstructions}
                  </p>
                )}
              </div>
              <span className="text-sm font-medium">
                ${(item.quantity * item.unitPrice).toFixed(2)}
              </span>
            </div>
          ))}
        </div>
      </div>

      {order.specialInstructions && (
        <div className="mb-4 p-2 bg-yellow-50 border-l-4 border-yellow-400">
          <p className="text-sm text-yellow-800">
            <strong>Special Instructions:</strong> {order.specialInstructions}
          </p>
        </div>
      )}

      <div className="flex justify-between items-center">
        <div className="text-sm text-gray-600">
          Total: <span className="font-semibold">${order.totalAmount.toFixed(2)}</span>
        </div>
        <div className="flex gap-2">
          {nextStatus && (
            <button
              onClick={handleStatusUpdate}
              className={`${getNextStatusButtonClass(order.status)} text-sm py-1 px-3`}
            >
              {getNextStatusButtonText(order.status)}
            </button>
          )}
        </div>
      </div>
    </div>
  );
};

export default OrderCard;
